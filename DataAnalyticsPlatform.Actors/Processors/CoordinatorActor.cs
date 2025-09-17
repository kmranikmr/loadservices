using Akka.Actor;
using Akka.Event;
using DataAccess.Models;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Actors.Processors
{
    // Message indicating the transformer actor has finished processing
    public class TransformerEnd { }

    /// <summary>
    /// Coordinates the ingestion job by managing reader, transformer, and writer actors.
    /// Handles job lifecycle, message passing, and error supervision.
    /// </summary>
    public class CoordinatorActor : ReceiveActor
    {
        #region Messages

        public class ReaderEnd { }
        public class DoJob { }
        public class WriterEnd { }

        #endregion

        public const string ReaderActor = "ReaderActor";
        public const string WriterManager = "WriterManager";
        public const string TransformerActor = "TransformerActor";

        private readonly ILoggingAdapter _logger = Context.GetLogger();

        private IActorRef _reader;
        private IActorRef _transformationActor;
        private IActorRef _writer;

        private readonly IngestionJob _ingestionJob;
        private readonly IRepository _repo;
        private readonly IActorRef _masterActor;

        private int _recordCount = 0;
        private int _transformedRecordCount = 0;

        public CoordinatorActor(IngestionJob j, IActorRef masterActor)
        {
            _ingestionJob = j;
            _masterActor = masterActor;
            SetReceiveBlocks();
        }

        /// <summary>
        /// Sets up message handlers for actor communication.
        /// </summary>
        private void SetReceiveBlocks()
        {
            Receive<DoJob>(_ =>
            {
                _logger.Info("Received DoJob. Starting reader actor.");
                _reader.Tell(new ReaderActor.StartReading());
            });

            Receive<CreateSchemaPostgres>(msg =>
            {
                _logger.Info("Forwarding CreateSchemaPostgres to master actor.");
                _masterActor.Tell(msg);
            });

            Receive<ReaderActor.ReaderReady>(_ =>
            {
                _logger.Info("Reader is ready. Requesting first record.");
                _reader.Tell(new ReaderActor.GetRecord());
            });

            Receive<ReaderActor.ReaderInitFailed>(_ =>
            {
                _logger.Error("Reader initialization failed. Shutting down coordinator.");
                Self.Tell(PoisonPill.Instance);
            });

            Receive<IRecord>(record =>
            {
                if (record != null)
                {
                    _recordCount++;
                    _logger.Info($"Received record #{_recordCount}. Sending to transformer.");
                    _transformationActor.Tell(new TransformerActor.TransformRecord(record));
                }
                else
                {
                    _logger.Warning("Received null record. Requesting next record.");
                    _reader.Tell(new ReaderActor.GetRecord());
                }
            });

            Receive<TransformerActor.TransformedRecord>(msg =>
            {
                if (msg.Record != null)
                {
                    _logger.Info("Received transformed record. Sending to writer.");
                    _writer.Tell(new WriterActor.WriteRecord(msg.Record));
                }
                else
                {
                    _logger.Info("Received transformed objects. Sending to writer.");
                    _writer.Tell(new WriterActor.WriteRecord(msg.Objects));
                }
                _reader.Tell(new ReaderActor.GetRecord());
                _transformedRecordCount++;
            });

            Receive<ReaderActor.NoMoreRecord>(_ =>
            {
                _logger.Info("No more records from reader. Stopping reader actor.");
                _reader.Tell(PoisonPill.Instance);
            });

            Receive<ReaderEnd>(_ =>
            {
                _logger.Info("Reader actor ended. Stopping transformer actor.");
                _transformationActor.Tell(PoisonPill.Instance);
            });

            Receive<TransformerEnd>(_ =>
            {
                _logger.Info("Transformer actor ended. Notifying writer of no more records.");
                _writer.Tell(new NoMoreTransformRecord());
            });

            Receive<WriterEnd>(_ =>
            {
                _logger.Info("Writer actor ended. Shutting down coordinator.");
                Self.Tell(PoisonPill.Instance);
            });

            Receive<List<WriterActor.ModelSizeData>>(msg =>
            {
                _logger.Info("Forwarding model size data to master actor.");
                _masterActor.Tell(msg);
            });
        }

        /// <summary>
        /// Supervises child actors. Stops actors on failure.
        /// </summary>
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(localOnlyDecider: ex =>
            {
                _logger.Error(ex, ex.Message);
                return Directive.Stop;
            }, loggingEnabled: true, maxNrOfRetries: 0, withinTimeMilliseconds: 0);
        }

        public override void AroundPostStop()
        {
            _logger.Info("CoordinatorActor stopped.");
            base.AroundPostStop();
        }

        /// <summary>
        /// Initializes child actors and sets up monitoring.
        /// </summary>
        protected override void PreStart()
        {
            if (Context.Child(ReaderActor).Equals(ActorRefs.Nobody))
            {
                _reader = Context.ActorOf(Props.Create(() => new ReaderActor(_ingestionJob)), ReaderActor);
                Context.WatchWith(_reader, new ReaderEnd());
                _logger.Info("Reader actor created and watched.");
            }

            if (Context.Child(TransformerActor).Equals(ActorRefs.Nobody))
            {
                _transformationActor = Context.ActorOf(Props.Create(() => new TransformerActor(_ingestionJob.ReaderConfiguration)), TransformerActor);
                Context.WatchWith(_transformationActor, new TransformerEnd());
                _logger.Info("Transformer actor created and watched.");
            }

            if (Context.Child(WriterManager).Equals(ActorRefs.Nobody))
            {
                _writer = Context.ActorOf(Props.Create(() => new WriterManager(_ingestionJob, Self)), WriterManager);
                Context.WatchWith(_writer, new WriterEnd());
                _logger.Info("Writer actor created and watched.");
            }

            base.PreStart();
        }
    }
}
