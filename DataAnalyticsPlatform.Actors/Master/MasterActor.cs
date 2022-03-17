using Akka.Actor;
using Akka.Routing;
using DataAnalyticsPlatform.Actors.Automation;
using DataAnalyticsPlatform.Actors.Processors;
using DataAnalyticsPlatform.Actors.Worker;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.Types;
using DataAnalyticsPlatform.Writers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.Master
{
    public class IngestionJob
    {
        public int JobId { get; private set; }
       
        public int UserId { get; set; }

        public string ControlTableConnectionString { get; set; }

        public ReaderConfiguration ReaderConfiguration { get; private set; }

        public WriterConfiguration []WriterConfiguration { get; private set; }

        //Transform Configuration
        //public TransformConfiguration TransformConfiguration { get; private set; }

        public IngestionJob(int jobId, ReaderConfiguration rConf, WriterConfiguration []wConf)
        {
            JobId = jobId;
            ReaderConfiguration = rConf;
            WriterConfiguration = wConf;
        }
    }

    public class JobRegistry
    {
        public IngestionJob job;
        public PreviewRegistry previewPreg;
        public JobRegistry( IngestionJob job, PreviewRegistry previewRegistry)
        {
            this.job = job;
            previewPreg = previewRegistry;
        }
    }

    public class MasterActor : ReceiveActor
    {


        private const string WorkerRouterName = "workerrouter";

        private const string AutomationCoordinator = "AutomationCoordinatorActor";

        private const string IngestionMonitor = "IngestionMonitorActor";

        private const int MaxNumberOfWorkerPerNode = 5;

        private Queue<IngestionJob> _jobs;

        private IActorRef _workerNodes;

        private IActorRef _ingestionMonitorActor;

        private System.Notifier _notifier;

        //const based on workernode * workeractor
        private const int MaxNummberOfParallerJob = 25;

        private int _numberOfRunningJob = 0;
        private IActorRef MasterRouterActor;
        private IActorRef _automationCoordinator;
        public PreviewRegistry _previewRegistry;
        public string ConnectionString{get; set;}
        public string PostgresConnection { get; set; }
        public string ElasticConnection { get; set; }
        public string MongoConnection { get; set; }
        public MasterActor(PreviewRegistry registry = null, string connectionString = "", string postgresConnection = "", string elasticconnstring = "", string mongoconnstring = "")//: this()//, IActorRef actorRef = null
        {
            _jobs = new Queue<IngestionJob>();
            ConnectionString = connectionString;
            PostgresConnection = postgresConnection;
            ElasticConnection = elasticconnstring;
            MongoConnection = mongoconnstring;

            ReceiveBlock();
          //  MasterRouterActor = actorRef;
        }

        public MasterActor()
        {
            ReceiveBlock();
        }
        public void ReceiveBlock()
        {
            Receive<IngestionJob>(x =>
            {
                _jobs.Enqueue(x);

                SendJobToWorkerNode();
            });


            Receive<JobRegistry>(x =>
            {
                _jobs.Enqueue(x.job);
                _previewRegistry = x.previewPreg;
                SendJobToWorkerNode();
            });

            Receive<JobDone>(x =>
            {
                Console.WriteLine(" Master Actor Job Done");
                _notifier?.Notify(x.JobId.ToString());
            });

            Receive<CreateSchemaPostgres>(x =>
            {
                _ingestionMonitorActor.Tell(x);
            });
        }

        public MasterActor(System.Notifier notifier, PreviewRegistry registry = null) : this(registry)
        {
            _notifier = notifier;
            ReceiveBlock();
        }

        private void SendJobToWorkerNode()
        {
            if (_jobs.Count > 0)
            {
                if (_numberOfRunningJob < MaxNummberOfParallerJob)
                {
                    var job = _jobs.Dequeue();
                    Console.WriteLine(" Model at dequeue " + job.ReaderConfiguration.TypeConfig.ModelInfoList[0].ModelId.ToString() + " " +job.ReaderConfiguration.TypeConfig.ModelInfoList[0].ModelName );  
                    //_workerActors.Tell(job);
                    if (MasterRouterActor != null )
                    {
                        MasterRouterActor.Tell(job);
                    }
                    else
                    {
                        _workerNodes.Tell(job);
                    }
                }
            }            
        }

        protected override void PreStart()
        {
            if (Context.Child(WorkerRouterName).Equals(ActorRefs.Nobody))
            {
                //  Use router config
                //_workerActors =
                //    Context.ActorOf(
                //        Props.Create(() => new WorkerActor(Self))
                //            .WithRouter(FromConfig.Instance), WorkerRouterName);

                //_workerActors =
                //    Context.ActorOf(
                //        Props.Create(() => new WorkerActor(Self)), WorkerRouterName);

                _workerNodes = Context.Child(WorkerRouterName).Equals(ActorRefs.Nobody)
                  ? Context.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), WorkerRouterName)
                  : Context.Child(WorkerRouterName);
                Context.WatchWith(_workerNodes, new WorkersRouterIsDead());

                _automationCoordinator = Context.ActorOf(
                      Props.Create(() => new AutomationCoordinator(Self, ConnectionString, PostgresConnection, ElasticConnection, MongoConnection)), AutomationCoordinator);

                
            }

           

            base.PreStart();
        }
    }

    internal class WorkersRouterIsDead
    {
        public WorkersRouterIsDead()
        {
        }
    }
}
