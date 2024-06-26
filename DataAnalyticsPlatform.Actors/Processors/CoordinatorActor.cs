using Akka.Actor;
using Akka.Event;
using DataAccess.Models;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Actors.Processors
{
    public class TransformerEnd
    {

    }
    public class CoordinatorActor : ReceiveActor
    {
        #region Messages

        public class ReaderEnd
        {

        }

        public class DoJob
        {

        }

        public class WriterEnd
        {

        }


        #endregion


        public const string ReaderActor = "ReaderActor";

        public const string WriterManager = "WriterManager";

        public const string TransformerActor = "TransformerActor";

        private ILoggingAdapter _logger = Context.GetLogger();

        IActorRef _reader;

        IActorRef _transformationActor;

        IActorRef _writer;

        IngestionJob _ingestionJob = null;

        IRepository _repo = null;

        IActorRef _masterActor;
        public CoordinatorActor(IngestionJob j, IActorRef masterActor)
        {
            _ingestionJob = j;

            _masterActor = masterActor;

            SetReceiveBlocks();
        }

        private int _recordCount = 0;

        private int _transformedRecordCount = 0;


        private void SetReceiveBlocks()
        {
            Receive<DoJob>(x =>
            {

                _reader.Tell(new ReaderActor.StartReading());
            });
            Receive<CreateSchemaPostgres>(x =>
            {
                _masterActor.Tell(x);
            });
            Receive<ReaderActor.ReaderReady>(x =>
            {
                //Console.WriteLine("Start GetRecord");
                _reader.Tell(new ReaderActor.GetRecord());
                //TODO    who will ask for next record? may be writer?
            });

            Receive<ReaderActor.ReaderInitFailed>(x =>
            {
                //may be shutdown coordinator here
                //kill coordinator
                Self.Tell(PoisonPill.Instance);
            });

            Receive<IRecord>(x =>
            {
                if (x != null)
                {
                    _recordCount++;
                    // Console.WriteLine("Start TransformRecord");
                    _transformationActor.Tell(new TransformerActor.TransformRecord(x));
                }
                else
                {
                    _reader.Tell(new ReaderActor.GetRecord());
                }
            });

            Receive<TransformerActor.TransformedRecord>(x =>
            {


                if (x.Record != null)
                {
                    // Console.WriteLine("Start WriteRecord");
                    _writer.Tell(new WriterActor.WriteRecord(x.Record));
                }
                else
                {
                    //   Console.WriteLine("Start WriteRecord");
                    //foreach (object bm in x.Models)
                    _writer.Tell(new WriterActor.WriteRecord(x.Objects));

                }
                _reader.Tell(new ReaderActor.GetRecord());

                _transformedRecordCount++;
            });

            Receive<ReaderActor.NoMoreRecord>(x =>
            {
                Console.WriteLine("Coordintaor NoMoreRecord");
                _reader.Tell(PoisonPill.Instance);
            });

            Receive<ReaderEnd>(x =>
            {
                Console.WriteLine("Coordintaor ReaderEnd");
                _transformationActor.Tell(PoisonPill.Instance);
            });

            Receive<TransformerEnd>(x =>
            {
                Console.WriteLine("Coordintaor TransformerEnd");
                _writer.Tell(new NoMoreTransformRecord());
            });

            Receive<WriterEnd>(x =>
            {
                Console.WriteLine("Coordintaor WriterEnd");
                //   _repo.UpdateJobStatus(_ingestionJob.JobId, 3, _ingestionJob.ReaderConfiguration.SourcePathId);
                //  _repo.UpdateJobEnd(_ingestionJob.JobId, _ingestionJob.ReaderConfiguration.SourcePathId);
                //kill coordinator
                Self.Tell(PoisonPill.Instance);
            });
            Receive<List<WriterActor.ModelSizeData>>(x =>
            {
                _masterActor.Tell(x);
            });
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(localOnlyDecider: x =>
            {
                //switch (x)
                //{
                //    //placeholders to flag the job error based on exception
                //    case SomeException re:
                //        {

                //            return Directive.Stop;
                //        }
                //    case SomeException de:
                //        {

                //            return Directive.Stop;
                //        }
                //    case SomeException are:
                //        {


                //            return Directive.Stop;
                //        }
                //}

                _logger.Error(x, x.Message);

                //stop the actor
                return Directive.Stop;

            }, loggingEnabled: true, maxNrOfRetries: 0, withinTimeMilliseconds: 0
            );
        }

        public override void AroundPostStop()
        {

            base.AroundPostStop();
        }
        protected override void PreStart()
        {
            if (Context.Child(ReaderActor).Equals(ActorRefs.Nobody))
            {
                _reader = Context.ActorOf(Props.Create(() => new ReaderActor(_ingestionJob)), ReaderActor);

                Context.WatchWith(_reader, new ReaderEnd());
            }

            if (Context.Child(TransformerActor).Equals(ActorRefs.Nobody))
            {
                _transformationActor = Context.ActorOf(Props.Create(() => new TransformerActor(_ingestionJob.ReaderConfiguration)), TransformerActor);

                Context.WatchWith(_transformationActor, new TransformerEnd());
            }

            if (Context.Child(WriterManager).Equals(ActorRefs.Nobody))
            {
                _writer = Context.ActorOf(Props.Create(() => new WriterManager(_ingestionJob, Self)), WriterManager);

                Context.WatchWith(_writer, new WriterEnd());
            }

            base.PreStart();
        }
    }
}
