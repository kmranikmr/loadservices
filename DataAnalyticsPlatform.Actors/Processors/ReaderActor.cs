using Akka.Actor;
using Akka.Event;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
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
                    record.FileName = Path.GetFileName(_ingestionJob.ReaderConfiguration.SourcePath);
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
