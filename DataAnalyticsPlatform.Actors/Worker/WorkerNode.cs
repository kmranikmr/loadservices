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

        int _maxNumberofConcurretWorker;

        IActorRef _workerActors;
        private IActorRef _ingestionMonitorActor;
        IActorRef _masterActor;
        private Repository _repo;
        public event EventHandler<IngestionJob> JobProcess;
        public event EventHandler<JobDone> JobComplete;
        public delegate void MyEventHandler(object sender, int e);
        public string _connectionString { get; set; }
        //public IngestionJob ingestionJob { get; set; }
        public List<IngestionJob> ingestionJob { get; set; }
       
        public void UpdateJobProcess(object sender, IngestionJob job)
        {
            Console.WriteLine("Worker Node UpdateJobProcess");
            //ingestionJob = job;
            ingestionJob.Add(job);
            Console.WriteLine(" job at update process " + job.ReaderConfiguration.TypeConfig.ModelInfoList[0].ModelId.ToString() + " " + job.ReaderConfiguration.TypeConfig.ModelInfoList[0].ModelName);
            string connectionString = job.ControlTableConnectionString;//"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";// @"Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123";//// @"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";
            _connectionString = connectionString;
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options, connectionString);
            _repo = new Repository(dbContext, null);
            var x = _repo.UpdateJobStatusSync(job.JobId, 2, job.ReaderConfiguration.SourcePathId);
            var y = _repo.UpdateJobStart(job.JobId, job.ReaderConfiguration.SourcePathId);
        }

        public async void UpdateJobComplete(object sender, JobDone job)
        {
            Console.WriteLine("Worker Node UpdateJobComplete " + job.FileId);
            string connectionString = _connectionString;//@"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";// "Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123"; //@"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options);
            _repo = new Repository(dbContext, null);

            var x = _repo.UpdateJobStatusSync(job.JobId, 3, job.FileId);
            var y = _repo.UpdateJobEnd(job.JobId, job.FileId);

        }
        public WorkerNode(int maxNumberofConcurretWorker)
        {
            ingestionJob = new List<IngestionJob>();
            _maxNumberofConcurretWorker = maxNumberofConcurretWorker;
            JobProcess += UpdateJobProcess;
            JobComplete += UpdateJobComplete;
           
            ReceiveBlock();
        }

        public async Task<bool> UpdateJob(int updateStatus, int jobId)
        {
            Func<bool> func = new Func<bool>(() =>
            {
                if (updateStatus == 2)
                {
                    var x = _repo.UpdateJobStatus(jobId, 2, 3);
                    var y = _repo.UpdateJobStart(jobId, 3);
                }
                else// if (updateStatus == 3)
                {
                    var c = _repo.UpdateJobStatus(jobId, 2, 3);
                    var d = _repo.UpdateJobStart(jobId, 3);
                }
                return true;
            });
            return await Task.Factory.StartNew(func);
        }
        public void ReceiveBlock()
        {

            Receive<IngestionJob>(x =>
            {
                _masterActor = Sender;
               
                JobProcess?.Invoke(this, x);
               
                _workerActors.Tell(x);

            });
            Receive<JobDone>(x =>
            {
                Console.WriteLine(" Worker Node Job Done");
                int st = 2;
                if (JobComplete != null)
                {
                    Console.WriteLine(" Worker Node Job Done ");
                }
                JobComplete?.Invoke(this, x);
                Console.WriteLine(" Worker Node before Master ");
                // var ret = UpdateJob(st).ConfigureAwait(true);
                _masterActor.Tell(x);

                if (ingestionJob != null)//lets take th eingetsio njob and send the info to datsaervice for projecttrigger
                {
                  
                    Console.WriteLine("IngestionJob Check");
                    IngestionJob tbd = null;
                    foreach (var j in ingestionJob)
                    {
                        Console.WriteLine("checking jobs");
                        if (j.ReaderConfiguration != null)
                        {
                            Console.WriteLine(((JobDone)x).FileId.ToString() + " " + j.ReaderConfiguration.SourcePathId.ToString());
                            if (((JobDone)x).FileId == j.ReaderConfiguration.SourcePathId)
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
                Console.WriteLine(" Worker Node before Master Done");
            });

            Receive<CreateSchemaPostgres>(x =>
            {
                if (_ingestionMonitorActor != null)
                {
                    _ingestionMonitorActor.Tell(x);
                }
            });
            Receive<List<WriterActor.ModelSizeData>>(x =>
            {
                Console.WriteLine("ModelSizeData");
                foreach (var s in x)
                {
                    foreach (var j in ingestionJob)
                    {
                        if (j.ReaderConfiguration.TypeConfig != null)
                        {
                            if (j.ReaderConfiguration.TypeConfig.ModelInfoList != null)
                            {
                                foreach (var model in j.ReaderConfiguration.TypeConfig.ModelInfoList)
                                {
                                    if (model.ModelName == s.ModelName)
                                    {
                                        Console.WriteLine("ModelSizeData " + model.ModelName);
                                        if (ingestionJob != null)
                                        {
                                            Console.WriteLine("ModelSizeData Updating");
                                            string connectionString = j.ControlTableConnectionString;//"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";// @"Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123";//// @"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";
                                            _connectionString = connectionString;
                                            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
                                            var dbContext = new DAPDbContext(options, connectionString);
                                            _repo = new Repository(dbContext, null);
                                            Console.WriteLine("ModelSizeData Updating Call");
                                            _repo.UpdateModelSize(j.UserId, model.ModelId, (int)s.Size);
                                            Console.WriteLine("ModelSizeData Updating done " + s.Size + " " + model.ModelId + " " + s.ModelName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        public async void SendAttempt(JobDone x, IngestionJob ij)
        {
            if (ij.JobId == x.JobId)
            {
                if (ij.ReaderConfiguration.TypeConfig != null)
                {
                    if (ij.ReaderConfiguration.TypeConfig.ModelInfoList != null)
                    {
                        string url = $"http://ec2basedservicealb-760561316.us-east-1.elb.amazonaws.com:6002/api/workflowattempts/projectrigger/{ij.ReaderConfiguration.ProjectId}";
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

                            Console.WriteLine($"ProjectTrigger|SchemaId {modeData.SchemaId} {url}");
                            Console.WriteLine($"ProjectTrigger|ProjectId {modeData.ProjectId} ");
                            var requestRest = new RestRequest(Method.POST);
                            requestRest.AddHeader("Accept", "application/json");
                            //requestRest.AddJsonBody(workflow);

                            Console.WriteLine($"ProjectTrigger|model {model.ModelId} {modeData.jobId}");
                            var json = JsonConvert.SerializeObject(modeData);
                            Console.WriteLine($"json {json}");

                            requestRest.AddParameter("application/json", json, ParameterType.RequestBody);
                            IRestResponse response = await client.ExecuteAsync(requestRest);
                            Console.WriteLine($"ProjectTrigger|done calling - {response.Content}");
                        }
                    }
                }
            }
        }

        protected override void PreStart()
        {
            if (Context.Child(WorkerPool).Equals(ActorRefs.Nobody))
            {
                _workerActors = Context.ActorOf(Props.Create(() => new WorkerActor(Self)).WithRouter(new RoundRobinPool(_maxNumberofConcurretWorker)), WorkerPool);
                Context.Watch(_workerActors);
            }
            if (Context.Child(IngestionMonitor).Equals(ActorRefs.Nobody))
            {

                Console.WriteLine("Creating Ingestion Actor node.");

                var props = Props.Create(() => new IngestionMonitorActor());

                _ingestionMonitorActor = Context.ActorOf(props, IngestionMonitor);

                Context.Watch(_ingestionMonitorActor);

                Console.WriteLine("Now watching ingestion monitor.");
            }
            base.PreStart();
        }
    }

    public class JobDone
    {
        public JobDone()
        {

        }
        public JobDone(int jobId)
        {
            this.JobId = jobId;
        }
        public JobDone(int jobId, int fileId)
        {
            this.JobId = jobId;
            FileId = fileId;
        }
        public int JobId { get; set; }
        public int FileId { get; set; }
    }
}
