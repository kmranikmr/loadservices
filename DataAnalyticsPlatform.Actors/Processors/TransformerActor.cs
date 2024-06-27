/*
 * This file defines the TransformerActor class responsible for transforming records in the DataAnalyticsPlatform's Processors namespace.
 * 
 * TransformerActor:
 * - Implements Akka.NET's ReceiveActor to manage messages related to transforming data records.
 * - Handles messages such as TransformRecord and TransformedRecord.
 * - Initializes with a ReaderConfiguration to dynamically invoke methods on model types for transformation.
 * - Transforms a received record using reflection to invoke specific methods (MapIt and GetModels) defined on the model type.
 * - Returns the transformed record or a list of models associated with the record.
 * - Sends the transformed data back to the sender actor for further processing or storage.
 * 
 * Overall, this actor facilitates the transformation of data records based on a specified model type and configuration within the Data Analytics Platform.
 */


using Akka.Actor;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using System.Collections.Generic;
using System.Reflection;

namespace DataAnalyticsPlatform.Actors.Processors
{
    public class TransformerActor : ReceiveActor
    {
        #region Messages

        public class TransformRecord
        {
            public IRecord Record { get; private set; }

            public TransformRecord(IRecord rec)
            {
                Record = rec;
            }
        }

        public class TransformedRecord
        {
            public IRecord Record { get; private set; }

            public List<object> Models { get; set; }
            public object Objects { get; set; }
            public TransformedRecord(IRecord rec)
            {
                Record = rec;
            }
            public TransformedRecord(List<object> models)
            {
                Models = models;
            }
            public TransformedRecord(object objs)
            {
                Objects = objs;
            }
        }

        #endregion
        private ReaderConfiguration _readerConfiguration;
        private MethodInfo _methodMap;
        private MethodInfo _methodGetModels;
        public TransformerActor(ReaderConfiguration readerConfiguration)
        {
            _readerConfiguration = readerConfiguration;
            if (readerConfiguration.ModelType != null)
            {
                _methodMap = readerConfiguration.ModelType.GetMethod("MapIt");

                _methodGetModels = readerConfiguration.ModelType.GetMethod("GetModels");
            }
            SetReceiveBlocks();
        }

        private void SetReceiveBlocks()
        {
            Receive<TransformRecord>(x =>
            {
                if (x != null)
                {
                    _methodMap.Invoke(x.Record.Instance, null);
                    //transform here, may return list of models, for now send as is
                    var ret = _methodGetModels.Invoke(x.Record.Instance, new object[] { (int)x.Record.FileId });
                    //(List<BaseModel>)

                    if (ret == null || (ret != null && ((List<BaseModel>)ret).Count == 0))
                    {
                        Sender.Tell(new TransformedRecord(x.Record));
                    }
                    else
                    {
                        ((List<BaseModel>)ret).ForEach(y => y.RecordId1 = x.Record.RecordId);
                        Sender.Tell(new TransformedRecord(ret));
                    }
                }
            });

          
        }
    }
}
