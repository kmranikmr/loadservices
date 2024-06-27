/*
 * This file defines the WorkerActor class responsible for managing ingestion jobs in the DataAnalyticsPlatform's Worker namespace.
 * 
 * WorkerActor:
 * - Implements Akka.NET's ReceiveActor to handle messages related to ingestion jobs and coordination with the master actor.
 * - Initializes with an optional master actor reference (_masterActor) to communicate job statuses and results.
 * - Receives messages such as CreateSchemaPostgres and IngestionJob to process schema creation and job execution.
 * - Dynamically generates code for data transformation if the model type is not specified based on the file type or configuration details.
 * - Creates a CoordinatorActor for each IngestionJob to oversee the execution and coordination of data ingestion tasks.
 * - Manages job lifecycle by tracking running jobs (_runningJobs) and handling CoordinatorIsDead messages to clean up and notify the master actor.
 * - Logs initialization and job status updates using Akka.NET's logging facilities.
 * 
 * Overall, this actor plays a pivotal role in initiating, monitoring, and completing data ingestion jobs within the Data Analytics Platform, ensuring robust job execution and status reporting.
 */


using Akka.Actor;
using Akka.Event;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.Processors;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataAnalyticsPlatform.Actors.Worker
{
    public class WorkerActor : ReceiveActor
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
                if (x.ReaderConfiguration.ModelType == null)
                {
                    //
                    TransformationCodeGenerator codegen = new TransformationCodeGenerator();
                    if (x.ReaderConfiguration.SourcePath.EndsWith(".csv"))
                    {
                        Console.WriteLine("filename passed" + x.ReaderConfiguration.SourcePath);
                        types = codegen.Code(x.ReaderConfiguration.TypeConfig, x.JobId, Path.GetFileName(x.ReaderConfiguration.SourcePath));
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
                _masterActor.Tell(new JobDone(x.JobId, x.FileId));
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
