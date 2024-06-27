﻿/* WriterActor:
 * - Implements Akka.NET's ReceiveActor to manage messages related to writing data records.
 * - Handles messages such as WriteRecord, WriteList, ConsistentHashableEnvelope, and NoMoreTransformRecord.
 * - Initializes with an IngestionJob to configure and instantiate writers based on WriterConfiguration.
 * - Manages a primary writer (_writer) and an optional elastic writer (_elasticWriter) based on destination types.
 * - Writes records or models to the configured destinations (_writer and _elasticWriter).
 * - Responds to NoMoreTransformRecord messages to clean up resources and stop itself.
 * - Implements error handling and logging for writer operations.
 * - Provides a mechanism to report data size information after stopping.
 * - Disposes of resources (_writer and _elasticWriter) during shutdown.
 * 
 * Overall, this actor plays a critical role in the data ingestion and transformation process within the Data Analytics Platform, ensuring data is efficiently written to configured destinations.
 */

using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Actors.Processors
{
    public class WriterActor : ReceiveActor
    {
        #region Messages
        public class ModelSizeData
        {
            public string ModelName { get; set; }
            public long? Size { get; set; }
        }
        public class WriteRecord
        {
            public IRecord Record { get; private set; }
            public List<object> Model { get; private set; }
            public object Objects { get; set; }
            public int key { set; get; }
            public WriteRecord(IRecord rec)
            {
                Record = rec;
            }
            public WriteRecord(List<object> rec)
            {
                Model = rec;
            }
            public WriteRecord(object objs)
            {
                Objects = objs;
            }
            public WriteRecord(object objs, int key)
            {
                Objects = objs;
                this.key = key;
            }
        }
        public class WriteList
        {

            public WriteList(List<object> list)
            {

            }
        }
        #endregion

        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);

        private IWriter _writer = null;
        private IWriter _elasticWriter = null;
        IngestionJob _ingestionJob;
        private IActorRef _writerManager;
        public string _schemaName { get; set; }

        public WriterActor(IngestionJob j)
        {
            _ingestionJob = j;
            _schemaName = j.ReaderConfiguration.TypeConfig.SchemaName.Replace(" ", string.Empty);

            _schemaName = j.ReaderConfiguration.ProjectId != -1 ? _schemaName + "_" + j.ReaderConfiguration.ProjectId + "_" + j.UserId : _schemaName;
            Console.WriteLine("WriterActor  _schemaName" + _schemaName);
            if (_ingestionJob.WriterConfiguration == null)
            {
                Console.WriteLine("WriterActor  WriterConfiguration Null");
            }
            foreach (var writerConf in _ingestionJob.WriterConfiguration)
            {
                if (writerConf.DestinationType != Shared.Types.DestinationType.ElasticSearch)
                {
                    writerConf.SchemaName = _schemaName;
                    _writer = Writers.Factory.GetWriter(writerConf);

                    _writer.SchemaName = _schemaName != string.Empty ? _schemaName : "public";
                    Console.WriteLine("WriterActor  WriterConfiguration _writer.SchemaName" + _writer.SchemaName);

                }
                if (writerConf.DestinationType == Shared.Types.DestinationType.ElasticSearch)
                {
                    _elasticWriter = Writers.Factory.GetWriter(writerConf); //new ElasticWriter("http://192.168.1.11:9200");
                    _elasticWriter.SchemaName = _schemaName;
                    Console.WriteLine("WriterActor  WriterConfiguration    _elasticWriter.SchemaName " + _elasticWriter.SchemaName);
                }
                else
                {
                    if (writerConf.DestinationType == Shared.Types.DestinationType.Mongo)
                    {
                        _elasticWriter = Writers.Factory.GetWriter(writerConf); //new ElasticWriter("http://192.168.1.11:9200");
                        _elasticWriter.SchemaName = _schemaName;
                        Console.WriteLine("WriterActor  WriterConfiguration    _elasticWriter.SchemaName " + _elasticWriter.SchemaName);
                    }
                }
            }

            //   _modelList = new List<object>(); //new Dictionary<string, List<BaseModel>>();

            _writer.OnError += _writer_OnError;

            _writer.OnInfo += _writer_OnInfo;
            if (_elasticWriter != null)
            {
                _elasticWriter.OnError += _Ewriter_OnError;

                _elasticWriter.OnInfo += _Ewriter_OnInfo;
            }

            SetReceiveBlocks();
        }

        private void SetReceiveBlocks()
        {
            Receive<WriteRecord>(x =>
            {
                try
                {
                    _writerManager = Sender;
                    if (x.Record != null)
                    {
                        Console.WriteLine("WriterActor x.Record");

                        _writer.Write(x.Record);
                        if (_elasticWriter != null)
                            _elasticWriter.Write(x.Record);
                    }
                    else if (x.Model != null)
                    {
                        Console.WriteLine("WriterActor x.Model");
                        _writer.Write(x.Model);
                        if (_elasticWriter != null)
                            _elasticWriter.Write(x.Model);
                    }
                    else if (x.Objects != null)
                    {
                        //Console.WriteLine("WriterActor x.Objects");
                        if (_writer == null)
                        {
                            Console.WriteLine("WriterActor _writer null");
                        }
                        _writer.Write(x.Objects);
                        if (_elasticWriter != null)
                            _elasticWriter.Write(x.Objects);
                    }
                    else
                    {
                        Console.WriteLine("WriterActor diff type" + x.GetType());
                    }
                }
                catch (Exception ex)
                {
                    throw new WriterException("Writer Error", ex, 0, Sender);
                }
            });
           
            Receive<ConsistentHashableEnvelope>(x =>
                {
                    object x1 = x.HashKey;
                });
            Receive<NoMoreTransformRecord>(x =>
                {
                    Console.WriteLine("NoMoreTransformRecord at WriterActor");
                    Context.Stop(Self);
                });
            ReceiveAny(x =>
            {
                Console.WriteLine("Writeractor receiveany" + x.GetType());
            });
        }
        public override void AroundPostStop()
        {


            _writer.Dispose();
            var sizeMap = _writer.DataSize();
            Console.WriteLine(" AroundPostStop ");
            List<ModelSizeData> sizeData = new List<ModelSizeData>();
            foreach (var item in sizeMap)
            {
                Console.WriteLine(" AroundPostStop " + item.Key + " " + item.Value);
                ModelSizeData data = new ModelSizeData
                {
                    ModelName = item.Key,
                    Size = item.Value
                };
                sizeData.Add(data);
            }
            if (sizeMap == null || sizeMap.Count == 0)
            {
                Console.WriteLine(" AroundPostStop sizeMap is null or 0");
            }
            _writerManager.Tell(sizeData);

            if (_elasticWriter != null)
                _elasticWriter.Dispose();
        }
        private void _writer_OnInfo(object sender, Shared.ExceptionUtils.InfoArgument e)
        {
            _log.Info(e.Information);
        }

        private void _writer_OnError(object sender, Shared.ExceptionUtils.ErrorArgument e)
        {
            _log.Error(e.ErrorMessage);
        }

        private void _Ewriter_OnInfo(object sender, Shared.ExceptionUtils.InfoArgument e)
        {
            _log.Info(e.Information);
        }

        private void _Ewriter_OnError(object sender, Shared.ExceptionUtils.ErrorArgument e)
        {
            _log.Error(e.ErrorMessage);
        }


    }

    public class WriterException : Exception
    {
        public long FileId { get; private set; }

        public IActorRef Caller { get; private set; }

        public WriterException(string errorMessage, Exception ex, long fileId, IActorRef caller) : base(errorMessage, ex)
        {
            FileId = fileId;
            Caller = caller;
        }
    }
}
