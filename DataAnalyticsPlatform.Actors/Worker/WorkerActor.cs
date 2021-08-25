using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.Processors;
using Akka.Event;
using DataAnalyticsPlatform.Common;
using System.Linq;
using DataAnalyticsPlatform.Shared.Models;

namespace DataAnalyticsPlatform.Actors.Worker
{
    public class WorkerActor: ReceiveActor
    {
        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);

        public class CoordinatorIsDead
        {
            public int JobId { get; private set; }
            public int FileId { get; set; }

            public CoordinatorIsDead(int jobId)
            {
                JobId = jobId;
            }

            public CoordinatorIsDead(int jobId, int fileId)
            {
                JobId = jobId;
                FileId = fileId;
            }
        }

        private HashSet<int> _runningJobs;
        IActorRef _masterActor;
        public WorkerActor(IActorRef masterActor)
        {
            _masterActor = masterActor;
            _runningJobs = new HashSet<int>();
            SetReceiveBlock();
        }

        public WorkerActor()
        {
            _runningJobs = new HashSet<int>();
            SetReceiveBlock();
        }

        private void SetReceiveBlock()
        {
            Receive<CreateSchemaPostgres>(x =>
            {
                _masterActor.Tell(x);
            });

            Receive<IngestionJob>(x =>
            {
                List<Type> types = null;
                //check if nill type ...then generate code and get types
                if ( x.ReaderConfiguration.ModelType == null)
                {
                    //
                    TransformationCodeGenerator codegen = new TransformationCodeGenerator();
                    if (x.ReaderConfiguration.SourcePath.EndsWith(".csv"))
                    {
                        types = codegen.Code(x.ReaderConfiguration.TypeConfig, x.JobId,  Path.GetFileName(x.ReaderConfiguration.SourcePath));
                    }
                    else if (x.ReaderConfiguration.SourcePath.EndsWith(".json"))
                    {
                        types = codegen.CodeJSON(x.ReaderConfiguration.TypeConfig, x.JobId);
                    }
                    else if (x.ReaderConfiguration.SourcePath.Contains("twitter"))
                    {
                        types = codegen.CodeJSON(x.ReaderConfiguration.TypeConfig, x.JobId);
                        ((TwitterConfiguration)x.ReaderConfiguration.ConfigurationDetails).MaxSearchEntriesToReturn = 100;

                       ((TwitterConfiguration)x.ReaderConfiguration.ConfigurationDetails).MaxTotalResults = 5000;
                    }

                    if (types != null && types.Count >= 1)//+x.JobId
                    {
                        var MapType = types.Where(k => k.FullName.Contains("Mappers")).FirstOrDefault();
                        var OriginalType = types.Where(k => k.FullName.Contains("OriginalRecord")).FirstOrDefault();
                        object mapperObject = null;
                        if (MapType != null)
                        {
                            mapperObject = Activator.CreateInstance(MapType);
                            x.ReaderConfiguration.ModelMap = mapperObject.GetType();
                        }
                        object originalObject = Activator.CreateInstance(OriginalType);
                        x.ReaderConfiguration.ModelType = originalObject.GetType();
                        x.ReaderConfiguration.Types = types;
                        
                    }
                }
                IActorRef coordinatorActor = Context.ActorOf(Props.Create(() => new CoordinatorActor(x, _masterActor)));

                _log.Debug("JobId: {0} Sent to Coordinator.", x.JobId);
                _runningJobs.Add(x.JobId);

                Context.WatchWith(coordinatorActor, new CoordinatorIsDead(x.JobId, x.ReaderConfiguration.SourcePathId));

                coordinatorActor.Tell(new CoordinatorActor.DoJob());
            });

            Receive<CoordinatorIsDead>(x =>
            {
                //Coordinator is dead
                Console.WriteLine("WorkerActor CoordinatorIsDead");
                _runningJobs.Remove(x.JobId);
                _masterActor.Tell(new JobDone(x.JobId , x.FileId));
                _log.Debug("JobId: {0} done.", x.JobId);
            }); 
        }

        protected override void PreStart()
        {
            _log.Info("Worker Instantiated...");
            base.PreStart();
        }
    }
}
