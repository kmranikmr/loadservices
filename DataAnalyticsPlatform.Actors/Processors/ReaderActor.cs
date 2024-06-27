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

        public class StartReading
        {

        }

        public class ReaderReady
        {

        }

        public class ReaderInitFailed
        {
            public string Message { get; private set; }
            public ReaderInitFailed(string msg)
            {
                Message = msg;
            }
        }

        public class GetRecord
        {

        }

        public class NoMoreRecord
        {

        }

        #endregion

        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);

        IReader _reader = null;

        IngestionJob _ingestionJob;

        public ReaderActor(IngestionJob j)
        {
            _ingestionJob = j;

            SetReceiveBlocks();
        }

        private void SetReceiveBlocks()
        {
            Receive<StartReading>(x =>
            {
                bool error = false;

                try
                {
                    _reader = Readers.Factory.GetReader(_ingestionJob.ReaderConfiguration);

                    _reader.OnError += _reader_OnError;

                    _reader.OnInfo += _reader_OnInfo;
                }
                catch (Exception ex)
                {
                    error = true;

                    _log.Error(ex, "Error while instantiating reader for : {0}", _ingestionJob.ReaderConfiguration.SourcePath);

                    Sender.Tell(new ReaderInitFailed(ex.Message));
                }

                if (error == false)
                {
                    Sender.Tell(new ReaderReady());
                }

            });

            Receive<GetRecord>(x =>
            {
                bool result = _reader.GetRecords(out IRecord record);

                if (result)
                {
                    if (_ingestionJob != null && _ingestionJob?.ReaderConfiguration != null && _ingestionJob.ReaderConfiguration.SourcePath != null)
                    {
                        if (record != null)
                        {
                            record.FileName = Path.GetFileName(_ingestionJob?.ReaderConfiguration?.SourcePath);
                        }
                    }
                    //send result to sender
                    Sender.Tell(record);
                }
                else
                {
                    Sender.Tell(new NoMoreRecord());
                }
            });
        }

        private void _reader_OnInfo(object sender, Shared.ExceptionUtils.InfoArgument e)
        {
            _log.Info(e.Information);
        }

        private void _reader_OnError(object sender, Shared.ExceptionUtils.ErrorArgument e)
        {
            _log.Error(e.ErrorMessage);
        }
    }
}
