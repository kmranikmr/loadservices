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
    /// <summary>
    /// Actor responsible for transforming records using reflection and sending results to the sender.
    /// </summary>
    public class TransformerActor : ReceiveActor
    {
        #region Messages

        /// <summary>
        /// Message to request transformation of a record.
        /// </summary>
        public class TransformRecord
        {
            /// <summary>
            /// The record to be transformed.
            /// </summary>
            public IRecord Record { get; private set; }

            public TransformRecord(IRecord rec)
            {
                Record = rec;
            }
        }

        /// <summary>
        /// Message containing the result of a transformation.
        /// </summary>
        public class TransformedRecord
        {
            /// <summary>
            /// The transformed record.
            /// </summary>
            public IRecord Record { get; private set; }

            /// <summary>
            /// List of models produced by transformation.
            /// </summary>
            public List<object> Models { get; set; }

            /// <summary>
            /// Additional objects produced by transformation.
            /// </summary>
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

        /// <summary>
        /// Configuration for the reader/model.
        /// </summary>
        private ReaderConfiguration _readerConfiguration;

        /// <summary>
        /// Reflection info for the MapIt method.
        /// </summary>
        private MethodInfo _methodMap;

        /// <summary>
        /// Reflection info for the GetModels method.
        /// </summary>
        private MethodInfo _methodGetModels;

        /// <summary>
        /// Initializes the TransformerActor with the specified reader configuration.
        /// </summary>
        /// <param name="readerConfiguration">Configuration containing model type and settings.</param>
        public TransformerActor(ReaderConfiguration readerConfiguration)
        {
            _readerConfiguration = readerConfiguration;
            if (readerConfiguration.ModelType != null)
            {
                // Get MapIt method info from model type
                _methodMap = readerConfiguration.ModelType.GetMethod("MapIt");

                // Get GetModels method info from model type
                _methodGetModels = readerConfiguration.ModelType.GetMethod("GetModels");
            }
            SetReceiveBlocks();
        }

        /// <summary>
        /// Sets up message handlers for the actor.
        /// </summary>
        private void SetReceiveBlocks()
        {
            // Handle TransformRecord messages
            Receive<TransformRecord>(x =>
            {
                if (x != null)
                {
                    // Invoke MapIt method on the record's instance
                    _methodMap.Invoke(x.Record.Instance, null);

                    // Invoke GetModels method, passing FileId
                    var ret = _methodGetModels.Invoke(x.Record.Instance, new object[] { (int)x.Record.FileId });

                    // If no models returned, send the original record
                    if (ret == null || (ret != null && ((List<BaseModel>)ret).Count == 0))
                    {
                        Sender.Tell(new TransformedRecord(x.Record));
                    }
                    else
                    {
                        // Set RecordId1 for each model and send the list
                        ((List<BaseModel>)ret).ForEach(y => y.RecordId1 = x.Record.RecordId);
                        Sender.Tell(new TransformedRecord(ret));
                    }
                }
            });
        }
    }
}
