/*
 * This file defines the ReaderActor class responsible for reading records in the DataAnalyticsPlatform's Processors namespace.
 * 
 * ReaderActor:
 * - Implements Akka.NET's ReceiveActor to manage messages related to reading data.
 * - Handles messages such as StartReading, ReaderReady, ReaderInitFailed, GetRecord, and NoMoreRecord.
 * - Initializes a specific reader based on the provided IngestionJob's ReaderConfiguration.
 * - Emits events for errors and informational messages during the reading process.
 * - Retrieves records from the reader and sends them to the sender or signals when no more records are available.
 * - Logs errors and informational messages using Akka's logging facilities.
 * 
 * Overall, this actor facilitates the reading of data from a specified source path, handling initialization, record retrieval, and error reporting within the Data Analytics Platform.
 */

using Akka.Actor;
using Akka.Event;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.IO;

namespace DataAnalyticsPlatform.Actors.Processors
{
    public class ReaderActor : ReceiveActor
    {
        #region Messages

        // Message to start reading process
        public class StartReading { }

        // Message indicating reader is ready
        public class ReaderReady { }

        // Message indicating reader initialization failed
        public class ReaderInitFailed
        {
            public string Message { get; private set; }
            public ReaderInitFailed(string msg)
            {
                Message = msg;
            }
        }

        // Message to request a record
        public class GetRecord { }

        // Message indicating no more records are available
        public class NoMoreRecord { }

        #endregion

        // Akka.NET logging adapter
        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);

        // Reader instance
        IReader _reader = null;

        // Ingestion job configuration
        IngestionJob _ingestionJob;

        // Constructor receives ingestion job configuration
        public ReaderActor(IngestionJob j)
        {
            _ingestionJob = j;
            SetReceiveBlocks();
        }

        // Sets up message handlers for the actor
        private void SetReceiveBlocks()
        {
            // Handler for StartReading message
            Receive<StartReading>(x =>
            {
                bool error = false;

                try
                {
                    // Instantiate reader using factory and job configuration
                    _reader = Readers.Factory.GetReader(_ingestionJob.ReaderConfiguration);

                    // Subscribe to reader events
                    _reader.OnError += _reader_OnError;
                    _reader.OnInfo += _reader_OnInfo;
                }
                catch (Exception ex)
                {
                    error = true;
                    // Log error and notify sender of failure
                    _log.Error(ex, "Error while instantiating reader for : {0}", _ingestionJob.ReaderConfiguration.SourcePath);
                    Sender.Tell(new ReaderInitFailed(ex.Message));
                }

                // Notify sender that reader is ready if no error occurred
                if (error == false)
                {
                    Sender.Tell(new ReaderReady());
                }
            });

            // Handler for GetRecord message
            Receive<GetRecord>(x =>
            {
                // Attempt to get next record from reader
                bool result = _reader.GetRecords(out IRecord record);

                if (result)
                {
                    // Set FileName property if possible
                    if (_ingestionJob != null && _ingestionJob?.ReaderConfiguration != null && _ingestionJob.ReaderConfiguration.SourcePath != null)
                    {
                        if (record != null)
                        {
                            record.FileName = Path.GetFileName(_ingestionJob?.ReaderConfiguration?.SourcePath);
                        }
                    }
                    // Send record to sender
                    Sender.Tell(record);
                }
                else
                {
                    // Notify sender that no more records are available
                    Sender.Tell(new NoMoreRecord());
                }
            });
        }

        // Handler for informational events from reader
        private void _reader_OnInfo(object sender, Shared.ExceptionUtils.InfoArgument e)
        {
            _log.Info(e.Information);
        }

        // Handler for error events from reader
        private void _reader_OnError(object sender, Shared.ExceptionUtils.ErrorArgument e)
        {
            _log.Error(e.ErrorMessage);
        }
    }
}
