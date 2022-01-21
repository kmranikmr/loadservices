using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Writers;
using System;
using System.Collections.Generic;
using System.Text;
using static DataAnalyticsPlatform.Actors.Processors.WriterManager;

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

        // public Dictionary<string, List<BaseModel>> _modelList;
       // public List<object> _modelList;
        public WriterActor(IngestionJob j)
        {
            _ingestionJob = j;
            _schemaName = j.ReaderConfiguration.TypeConfig.SchemaName.Replace(" ",string.Empty);
             
            _schemaName = j.ReaderConfiguration.ProjectId != -1 ? _schemaName + "_" + j.ReaderConfiguration.ProjectId+"_"+j.UserId : _schemaName;

            foreach (var writerConf in _ingestionJob.WriterConfiguration)
            {
                if (writerConf.DestinationType != Shared.Types.DestinationType.ElasticSearch)
                {
                    writerConf.SchemaName = _schemaName;
                    _writer = Writers.Factory.GetWriter(writerConf);
                    _writer.SchemaName = _schemaName != string.Empty ? _schemaName : "public";
                  
                }
                if (writerConf.DestinationType == Shared.Types.DestinationType.ElasticSearch)
                {
                    _elasticWriter = Writers.Factory.GetWriter(writerConf); //new ElasticWriter("http://192.168.1.11:9200");
                    _elasticWriter.SchemaName = _schemaName;
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
                        _writer.Write(x.Record);
                        if (_elasticWriter != null)
                            _elasticWriter.Write(x.Record);
                    }
                    else if (x.Model != null)
                    {
                        _writer.Write(x.Model);
                        if (_elasticWriter != null)
                            _elasticWriter.Write(x.Model);
                    }
                    else if (x.Objects != null)
                    {
                        _writer.Write(x.Objects);
                        if (_elasticWriter != null)
                            _elasticWriter.Write(x.Objects);
                    }
                }catch (Exception ex)
                {
                    throw new WriterException("Writer Error", ex, 0, Sender);
                }
            });
            //Receive<BaseModel>(x =>
            //{
            //    if (_modelList.Count >= 500)
            //    {
            //       _writer.Write(_modelList);
            //    }
            //    else
            //    {
            //        _modelList.Add(x);
            //        //if ( !_modelList.TryGetValue( x.ModelName, out List<BaseModel> baseList))
            //        //{
            //        //    _modelList.Add(x.ModelName, new List<BaseModel>() { x});
            //        //}
            //        //else
            //        //{
            //        //    _modelList[x.ModelName].Add(x);
            //        //}                   
            //    }
            //});
            Receive<ConsistentHashableEnvelope>(x=>
                {
                    object x1 = x.HashKey;
            });
            Receive<NoMoreTransformRecord>(x=>
                {
                    Console.WriteLine("NoMoreTransformRecord at WriterActor");
                    Context.Stop(Self);
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
