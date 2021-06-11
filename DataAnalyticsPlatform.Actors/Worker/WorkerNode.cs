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
using System.ComponentModel;
using System.Text;
using System.Threading;
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
        public IngestionJob ingestionJob { get; set; }
        //private async void backgroundWorker1_DoWork(int jobId, int fileId)// object sender, System.ComponentModel.DoWorkEventArgs e)
        //{
        //    string connectionString = @"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";
        //    var buildrOption =  new DbContextOptionsBuilder<DAPDbContext>();

        //    var options = SqlServerDbContextOptionsExtensions.UseSqlServer<DAPDbContext>(buildrOption, connectionString).Options;
        //    var dbContext = new DAPDbContext(options);
        //    IRepository repo = new Repository(dbContext, null);
        //    await repo.UpdateJobStatus(jobId, 2, fileId);
        //    await repo.UpdateJobStart(jobId, fileId);
        //}
        public void UpdateJobProcess(object sender, IngestionJob job)
        {
            Console.WriteLine("Worker Node UpdateJobProcess");
            ingestionJob = job;
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
            Console.WriteLine("Worker Node UpdateJobComplete");
            string connectionString = _connectionString;//@"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";// "Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123"; //@"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options);
            _repo = new Repository(dbContext, null);
            var x = _repo.UpdateJobStatusSync(job.JobId, 3, job.FileId);
            var y = _repo.UpdateJobEnd(job.JobId, job.FileId);
            
        }
        public WorkerNode(int maxNumberofConcurretWorker)
        {
            _maxNumberofConcurretWorker = maxNumberofConcurretWorker;
            JobProcess += UpdateJobProcess;
            JobComplete += UpdateJobComplete;
            //  string connectionString = @"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";

            //  var options = SqlServerDbContextOptionsExtensions.UseSqlServer<DAPDbContext>(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            //  var dbContext = new DAPDbContext(options);
            //   _repo = new Repository(dbContext, null);

            //   _masterActor = masterActor;
            ReceiveBlock();
        }

        public async Task<bool> UpdateJob(int updateStatus, int jobId)
        {
            Func<bool> func = new Func<bool>(() =>
            {
                if (updateStatus == 2)
                {
                   var x = _repo.UpdateJobStatus(jobId, 2,3);
                   var y = _repo.UpdateJobStart(jobId, 3);
                }
                else// if (updateStatus == 3)
                {
                    var c =  _repo.UpdateJobStatus(jobId, 2, 3);
                    var d=_repo.UpdateJobStart(jobId, 3);
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
               // BackgroundWorker bg = new BackgroundWorker();
               // bg.DoWork += (obj, e) => backgroundWorker1_DoWork(x.JobId, 3);
               // bg.RunWorkerAsync();
                JobProcess?.Invoke(this, x);
               // JobProcess  -= UpdateJobProcess;
                // var h = UpdateJob(2, x.JobId);
                // _repo.UpdateJobStatusSync(x.JobId, 2, 3);
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
                    SendAttempt(x);
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
        }

        public async void SendAttempt(JobDone  x)
        {
            if (ingestionJob.JobId == x.JobId)
            {
                if (ingestionJob.ReaderConfiguration.TypeConfig != null)
                {
                    if (ingestionJob.ReaderConfiguration.TypeConfig.ModelInfoList != null)
                    {
                        string url = $"http://ec2basedservicealb-760561316.us-east-1.elb.amazonaws.com:6002/api/workflowattempts/projectrigger/{ingestionJob.ReaderConfiguration.ProjectId}";
                        var client = new RestClient(url);
                        foreach (var model in ingestionJob.ReaderConfiguration.TypeConfig.ModelInfoList)
                        {
                            IngestedData modeData = new IngestedData
                            {
                                UserId = ingestionJob.UserId,
                                ModelId = model.ModelId,
                                SchemaId = ingestionJob.ReaderConfiguration.TypeConfig.SchemaId,
                                ProjectId = ingestionJob.ReaderConfiguration.ProjectId,
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
