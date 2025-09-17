/*
 * * 
 * WorkerNode:
 * - Implements Akka.NET's ReceiveActor to handle messages related to ingestion jobs and coordination with worker actors and monitors.
 * - Initializes with parameters such as maximum number of concurrent workers (_maxNumberofConcurretWorker) and references to worker actors and monitors.
 * - Tracks ingestion jobs using events (JobProcess and JobComplete) for handling job status updates and completion notifications.
 * - Updates job status in a repository (Repository) and triggers job completion handling upon receiving JobDone messages.
 * - Manages the lifecycle of worker actors and ingestion monitors (IngestionMonitorActor) during initialization (PreStart method).
 * - Sends messages such as IngestionJob, JobDone, and CreateSchemaPostgres to appropriate actors for further processing.
 * - Handles ModelSizeData messages to update model sizes in the database based on ingestion job configurations.
 * - Implements asynchronous methods for sending job completion notifications to external services (SendAttempt method).
 * 
 * Overall, this actor plays a central role in coordinating and managing ingestion jobs within the Data Analytics Platform, ensuring efficient job execution and status reporting.
 */

using Akka.Actor;
using Akka.Routing;
using DataAccess.Models;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.Processors;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DataAnalyticsPlatform.Actors.Worker
{
    public class IngestedData
    {
        public int ProjectId { get; set; }
        public int SchemaId { get; set; }
        public int ModelId { get; set; }
        public int jobId { get; set; }
        public int UserId { get; set; }
    }

    public class WorkerNode : ReceiveActor
    {
        private const string WorkerPool = "WorkerPool";
        private const string IngestionMonitor = "IngestionMonitorActor";

        private readonly int _maxNumberofConcurretWorker;
        private IActorRef _workerActors;
        private IActorRef _ingestionMonitorActor;
        private IActorRef _masterActor;
        private Repository _repo;
        private readonly ILogger<WorkerNode> _logger;

        public event EventHandler<IngestionJob> JobProcess;
        public event EventHandler<JobDone> JobComplete;

        public string _connectionString { get; set; }
        public List<IngestionJob> ingestionJob { get; set; }

        public WorkerNode(int maxNumberofConcurretWorker)
        {
            ingestionJob = new List<IngestionJob>();
            _maxNumberofConcurretWorker = maxNumberofConcurretWorker;

            // Setup logger (replace with your DI or logger factory as needed)
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WorkerNode>();

            JobProcess += UpdateJobProcess;
            JobComplete += UpdateJobComplete;

            ReceiveBlock();
        }

        /// <summary>
        /// Handles job process update, sets connection string, updates job status in DB.
        /// </summary>
        public void UpdateJobProcess(object sender, IngestionJob job)
        {
            _logger.LogInformation("Worker Node UpdateJobProcess");
            ingestionJob.Add(job);

            _logger.LogInformation("Job at update process {ModelId} {ModelName}",
                job.ReaderConfiguration.TypeConfig.ModelInfoList[0].ModelId,
                job.ReaderConfiguration.TypeConfig.ModelInfoList[0].ModelName);

            string connectionString = job.ControlTableConnectionString;
            _connectionString = connectionString;

            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options, connectionString);
            _repo = new Repository(dbContext, null);

            _repo.UpdateJobStatusSync(job.JobId, 2, job.ReaderConfiguration.SourcePathId);
            _repo.UpdateJobStart(job.JobId, job.ReaderConfiguration.SourcePathId);
        }

        /// <summary>
        /// Handles job completion, updates job status and end time in DB.
        /// </summary>
        public async void UpdateJobComplete(object sender, JobDone job)
        {
            _logger.LogInformation("Worker Node UpdateJobComplete {FileId}", job.FileId);

            string connectionString = _connectionString;
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options);
            _repo = new Repository(dbContext, null);

            _repo.UpdateJobStatusSync(job.JobId, 3, job.FileId);
            _repo.UpdateJobEnd(job.JobId, job.FileId);
        }

        /// <summary>
        /// Updates job status asynchronously.
        /// </summary>
        public async Task<bool> UpdateJob(int updateStatus, int jobId)
        {
            Func<bool> func = () =>
            {
                if (updateStatus == 2)
                {
                    _repo.UpdateJobStatus(jobId, 2, 3);
                    _repo.UpdateJobStart(jobId, 3);
                }
                else
                {
                    _repo.UpdateJobStatus(jobId, 2, 3);
                    _repo.UpdateJobStart(jobId, 3);
                }
                return true;
            };
            return await Task.Factory.StartNew(func);
        }

        /// <summary>
        /// Defines message handlers for the actor.
        /// </summary>
        public void ReceiveBlock()
        {
            // Handles incoming IngestionJob messages
            Receive<IngestionJob>(x =>
            {
                _masterActor = Sender;
                JobProcess?.Invoke(this, x);
                _workerActors.Tell(x);
            });

            // Handles JobDone messages
            Receive<JobDone>(x =>
            {
                _logger.LogInformation("Worker Node Job Done");
                int st = 2;
                JobComplete?.Invoke(this, x);

                _logger.LogInformation("Worker Node before Master");
                _masterActor.Tell(x);

                // Send project trigger info to data service for completed jobs
                if (ingestionJob != null)
                {
                    _logger.LogInformation("IngestionJob Check");
                    IngestionJob tbd = null;
                    foreach (var j in ingestionJob)
                    {
                        if (j.ReaderConfiguration != null)
                        {
                            _logger.LogInformation("Checking jobs");
                            if (x.FileId == j.ReaderConfiguration.SourcePathId)
                            {
                                SendAttempt(x, j);
                                tbd = j;
                            }
                        }
                    }
                    if (tbd != null)
                    {
                        ingestionJob.Remove(tbd);
                    }
                }
                _logger.LogInformation("Worker Node before Master Done");
            });

            // Handles schema creation requests
            Receive<CreateSchemaPostgres>(x =>
            {
                if (_ingestionMonitorActor != null)
                {
                    _ingestionMonitorActor.Tell(x);
                }
            });

            // Handles model size data updates
            Receive<List<WriterActor.ModelSizeData>>(x =>
            {
                _logger.LogInformation("ModelSizeData received");
                foreach (var s in x)
                {
                    foreach (var j in ingestionJob)
                    {
                        if (j.ReaderConfiguration.TypeConfig?.ModelInfoList != null)
                        {
                            foreach (var model in j.ReaderConfiguration.TypeConfig.ModelInfoList)
                            {
                                if (model.ModelName == s.ModelName)
                                {
                                    _logger.LogInformation("ModelSizeData {ModelName}", model.ModelName);
                                    string connectionString = j.ControlTableConnectionString;
                                    _connectionString = connectionString;
                                    var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
                                    var dbContext = new DAPDbContext(options, connectionString);
                                    _repo = new Repository(dbContext, null);

                                    _logger.LogInformation("ModelSizeData Updating Call");
                                    _repo.UpdateModelSize(j.UserId, model.ModelId, (int)s.Size);
                                    _logger.LogInformation("ModelSizeData Updating done {Size} {ModelId} {ModelName}", s.Size, model.ModelId, s.ModelName);
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Sends job completion notification to external service.
        /// </summary>
        public async void SendAttempt(JobDone x, IngestionJob ij)
        {
            if (ij.JobId == x.JobId && ij.ReaderConfiguration.TypeConfig?.ModelInfoList != null)
            {
                    // Retrieve secret URL from environment variable
                    string baseUrl = Environment.GetEnvironmentVariable("WORKFLOW_ATTEMPT_URL");
                    if (string.IsNullOrEmpty(baseUrl))
                    {
                        _logger.LogError("Secret workflow attempt URL is not set.");
                        return;
                    }
                    string url = $"{baseUrl}/api/workflowattempts/projectrigger/{ij.ReaderConfiguration.ProjectId}";
                var client = new RestClient(url);

                foreach (var model in ij.ReaderConfiguration.TypeConfig.ModelInfoList)
                {
                    IngestedData modeData = new IngestedData
                    {
                        UserId = ij.UserId,
                        ModelId = model.ModelId,
                        SchemaId = ij.ReaderConfiguration.TypeConfig.SchemaId,
                        ProjectId = ij.ReaderConfiguration.ProjectId,
                        jobId = x.JobId
                    };

                    _logger.LogInformation("ProjectTrigger|SchemaId {SchemaId} {Url}", modeData.SchemaId, url);
                    _logger.LogInformation("ProjectTrigger|ProjectId {ProjectId}", modeData.ProjectId);

                    var requestRest = new RestRequest(Method.POST);
                    requestRest.AddHeader("Accept", "application/json");

                    _logger.LogInformation("ProjectTrigger|model {ModelId} {JobId}", model.ModelId, modeData.jobId);

                    var json = JsonConvert.SerializeObject(modeData);
                    _logger.LogInformation("json {Json}", json);

                    requestRest.AddParameter("application/json", json, ParameterType.RequestBody);
                    IRestResponse response = await client.ExecuteAsync(requestRest);

                    _logger.LogInformation("ProjectTrigger|done calling - {Content}", response.Content);
                }
            }
        }

        /// <summary>
        /// Initializes worker and monitor actors.
        /// </summary>
        protected override void PreStart()
        {
            if (Context.Child(WorkerPool).Equals(ActorRefs.Nobody))
            {
                _workerActors = Context.ActorOf(
                    Props.Create(() => new WorkerActor(Self))
                        .WithRouter(new RoundRobinPool(_maxNumberofConcurretWorker)),
                    WorkerPool);
                Context.Watch(_workerActors);
            }

            if (Context.Child(IngestionMonitor).Equals(ActorRefs.Nobody))
            {
                _logger.LogInformation("Creating Ingestion Actor node.");
                var props = Props.Create(() => new IngestionMonitorActor());
                _ingestionMonitorActor = Context.ActorOf(props, IngestionMonitor);
                Context.Watch(_ingestionMonitorActor);
                _logger.LogInformation("Now watching ingestion monitor.");
            }

            base.PreStart();
        }
    }

    public class JobDone
    {
        public JobDone() { }
        public JobDone(int jobId) { this.JobId = jobId; }
        public JobDone(int jobId, int fileId) { this.JobId = jobId; FileId = fileId; }
        public int JobId { get; set; }
        public int FileId { get; set; }
    }
}
