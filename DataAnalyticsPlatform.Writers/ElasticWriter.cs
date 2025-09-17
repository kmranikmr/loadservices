/*
 * ElasticWriter.cs
 * 
 * This file implements an Elasticsearch writer component that exports data records to 
 * Elasticsearch using the Nest library. It provides functionality to write records 
 * individually or in batches, with configurable options and error handling.
 * 

 */

using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DataAnalyticsPlatform.Writers
{
    /// <summary>
    /// Writer implementation for Elasticsearch output. Inherits from BaseWriter and provides
    /// functionality to export data records to Elasticsearch indices with configurable options.
    /// </summary>
    public class ElasticWriter : BaseWriter
    {
        /// <summary>
        /// Elasticsearch client instance for performing operations
        /// </summary>
        protected readonly IElasticClient Client;
        
        /// <summary>
        /// Buffer for records to be written in batch
        /// </summary>
        private List<object> _recordBuffer;
        
        /// <summary>
        /// Default batch size for regular writes
        /// </summary>
        private const int DEFAULT_BATCH_SIZE = 100;
        
        /// <summary>
        /// Default batch size for bulk operations
        /// </summary>
        private const int DEFAULT_BULK_SIZE = 100;

        /// <summary>
        /// Initializes a new instance of the ElasticWriter class with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The Elasticsearch connection string</param>
        public ElasticWriter(string connectionString) : base(connectionString, Shared.Types.DestinationType.ElasticSearch)
        {
            LogInfo($"Initializing ElasticWriter with connection: {connectionString}");
            _recordBuffer = new List<object>();
            Client = CreateClient(connectionString);
            
            if (Client != null)
            {
                LogInfo("Successfully created Elasticsearch client");
            }
            else
            {
                LogError("Failed to create Elasticsearch client");
            }
        }

        /// <summary>
        /// Creates and configures an Elasticsearch client with custom serialization settings.
        /// </summary>
        /// <param name="connectionString">The Elasticsearch connection string</param>
        /// <returns>A configured IElasticClient instance</returns>
        public IElasticClient CreateClient(string connectionString)
        {
            try
            {
                LogInfo("Creating Elasticsearch client");
                
                // Create connection pool with single node
                var node = new Uri(connectionString);
                var connectionPool = new SingleNodeConnectionPool(node);
                
                // Configure connection settings with custom JSON serialization
                var connectionSettings = new ConnectionSettings(
                    connectionPool,
                    sourceSerializer: (builtin, setting) => new JsonNetSerializer(
                        builtin, 
                        setting, 
                        () => new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }
                    )
                )
                .DisableDirectStreaming() // Keep response bytes for debugging
                .DefaultMappingFor<BaseModel>(m => m
                    .Ignore(p => p.Props)
                    .Ignore(p => p.Values)
                );
                
                return new ElasticClient(connectionSettings);
            }
            catch (Exception ex)
            {
                LogError($"Error creating Elasticsearch client: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Creates tables for the specified model objects (not implemented for Elasticsearch).
        /// </summary>
        /// <param name="model">The list of model objects</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>False as this operation is not supported</returns>
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            LogInfo("CreateTables(List<object>) called but not implemented for Elasticsearch");
            return false;
        }

        /// <summary>
        /// Creates tables for the specified BaseModel objects (not implemented for Elasticsearch).
        /// </summary>
        /// <param name="model">The list of BaseModel objects</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>False as this operation is not supported</returns>
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            LogInfo("CreateTables(List<BaseModel>) called but not implemented for Elasticsearch");
            return false;
        }

        /// <summary>
        /// Writes a list of BaseModel objects to Elasticsearch (not implemented).
        /// </summary>
        /// <param name="records">The list of BaseModel objects to write</param>
        public override void Write(List<BaseModel> records)
        {
            LogInfo("Write(List<BaseModel>) called but not implemented for Elasticsearch");
        }

        /// <summary>
        /// Returns information about the data size.
        /// </summary>
        /// <returns>A dictionary containing data size information</returns>
        public override Dictionary<string, long?> DataSize()
        {
            LogInfo("DataSize method called but not implemented for Elasticsearch");
            return null;
        }
        /// <summary>
        /// Writes a record to Elasticsearch.
        /// </summary>
        /// <param name="record">The record to write</param>
        public override void Write(IRecord record)
        {
            LogInfo("Write(IRecord) called but not implemented in detail for Elasticsearch");
        }

        /// <summary>
        /// Writes a list of objects to Elasticsearch.
        /// </summary>
        /// <param name="records">The list of objects to write</param>
        public override void Write(List<object> records)
        {
            if (records == null || records.Count == 0)
            {
                LogInfo("Write(List<object>) called with empty or null list");
                return;
            }
            
            LogInfo($"Adding {records.Count} records to buffer");
            _recordBuffer.AddRange(records);

            // Check if buffer size exceeds batch size threshold
            if (_recordBuffer.Count >= 10)
            {
                LogInfo($"Buffer reached batch size threshold (10), performing bulk write");
                Dump();
                _recordBuffer.Clear();
            }
        }

        /// <summary>
        /// Writes a single object or enumerable collection to Elasticsearch.
        /// </summary>
        /// <param name="record">The object to write</param>
        public override void Write(object record)
        {
            if (record == null)
            {
                LogInfo("Write(object) called with null record");
                return;
            }
            
            try
            {
                // Handle both individual objects and collections
                if (record is IEnumerable && !(record is string))
                {
                    LogInfo("Processing enumerable record");
                    var list = ((IEnumerable<BaseModel>)record);
                    _recordBuffer.AddRange(list);
                }
                else
                {
                    LogInfo("Processing single record");
                    _recordBuffer.Add((BaseModel)record);
                }

                // Check if buffer size exceeds batch size threshold
                if (_recordBuffer.Count >= DEFAULT_BATCH_SIZE)
                {
                    LogInfo($"Buffer reached batch size threshold ({DEFAULT_BATCH_SIZE}), performing bulk write");
                    Dump();
                    _recordBuffer.Clear();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error writing record: {ex.Message}");
            }
        }
        /// <summary>
        /// Writes the buffered records to Elasticsearch using bulk operations.
        /// </summary>
        public void Dump()
        {
            if (_recordBuffer.Count == 0)
            {
                LogInfo("Dump called but no records to write");
                return;
            }
            
            try
            {
                // Determine index name from the first record
                string indexName = SchemaName + "." + ((BaseModel)_recordBuffer[0]).ModelName;
                indexName = indexName.ToLower(); // Elasticsearch indices must be lowercase
                
                LogInfo($"Writing {_recordBuffer.Count} records to Elasticsearch index '{indexName}'");
                
                // Configure and execute bulk write operation
                var waitHandle = new CountdownEvent(1);
                var bulkAll = Client.BulkAll(_recordBuffer, b => b
                    .Index(indexName)
                    .BackOffRetries(2)
                    .BackOffTime("30s")
                    .RefreshOnCompleted(true)
                    .MaxDegreeOfParallelism(4)
                    .Size(DEFAULT_BULK_SIZE)
                );

                // Subscribe to bulk operation events
                bulkAll.Subscribe(new BulkAllObserver(
                    onNext: (response) => {
                        LogInfo($"Bulk write progress: {response.Page}/{response.Pages} pages, {response.Items} items");
                    },
                    onError: (error) => {
                        LogError($"Error during bulk write: {error.Message}");
                        throw error; // Rethrow to handle at higher level
                    },
                    onCompleted: () => {
                        LogInfo($"Bulk write completed successfully");
                        waitHandle.Signal();
                    }
                ));

                // Wait for the operation to complete
                waitHandle.Wait();
                LogInfo($"Successfully wrote {_recordBuffer.Count} records to Elasticsearch");
            }
            catch (Exception ex)
            {
                LogError($"Error writing to Elasticsearch: {ex.Message}");
            }
        }

        /// <summary>
        /// Alternative implementation of bulk write with more detailed error handling and tracking.
        /// </summary>
        public void Dump1()
        {
            if (_recordBuffer.Count == 0)
            {
                LogInfo("Dump1 called but no records to write");
                return;
            }
            
            try
            {
                string indexName = ((BaseModel)_recordBuffer[0]).ModelName;
                LogInfo($"Writing {_recordBuffer.Count} records to Elasticsearch index '{indexName}' using advanced bulk write");
                
                // Collections for tracking results and errors
                List<string> errors = new List<string>();
                int seenPages = 0;
                int requests = 0;
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                ConcurrentBag<BulkResponse> bulkResponses = new ConcurrentBag<BulkResponse>();
                ConcurrentBag<BulkAllResponse> bulkAllResponses = new ConcurrentBag<BulkAllResponse>();
                ConcurrentBag<object> deadLetterQueue = new ConcurrentBag<object>();
                
                // Configure and execute bulk write with advanced options
                var observableBulk = Client.BulkAll(_recordBuffer, f => f
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    .BulkResponseCallback(response => {
                        bulkResponses.Add(response);
                        Interlocked.Increment(ref requests);
                        LogInfo($"Bulk response received, request #{requests}");
                    })
                    .ContinueAfterDroppedDocuments()
                    .DroppedDocumentCallback((response, document) => {
                        errors.Add(response.Error.Reason);
                        deadLetterQueue.Add(document);
                        LogError($"Document dropped: {response.Error.Reason}");
                    })
                    .BackOffTime(TimeSpan.FromSeconds(5))
                    .BackOffRetries(2)
                    .Size(1000)
                    .RefreshOnCompleted()
                    .Index(indexName)
                    .BufferToBulk((descriptor, buffer) => descriptor.IndexMany(buffer))
                , tokenSource.Token);

                // Wait for the operation to complete with timeout
                try
                {
                    LogInfo("Waiting for bulk operation to complete");
                    observableBulk.Wait(TimeSpan.FromMinutes(15), response => {
                        bulkAllResponses.Add(response);
                        Interlocked.Increment(ref seenPages);
                        LogInfo($"Bulk page completed: {seenPages}");
                    });
                    
                    LogInfo($"Bulk operation completed. Processed {seenPages} pages, {requests} requests");
                }
                catch (Exception ex)
                {
                    LogError($"Error waiting for bulk operation: {ex.Message}");
                }
                
                // Log any errors
                if (errors.Count > 0)
                {
                    LogError($"Bulk operation completed with {errors.Count} errors");
                    foreach (var error in errors)
                    {
                        LogError($"Elasticsearch error: {error}");
                    }
                }
                
                if (deadLetterQueue.Count > 0)
                {
                    LogError($"{deadLetterQueue.Count} documents were not indexed");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in Dump1 method: {ex.Message}");
            }
        }

        /// <summary>
        /// Releases resources used by the Elasticsearch writer and writes any remaining buffered records.
        /// </summary>
        public override void Dispose()
        {
            LogInfo("Disposing ElasticWriter");
            
            // Write any remaining records in the buffer
            if (_recordBuffer.Count > 0)
            {
                LogInfo($"Writing {_recordBuffer.Count} remaining records during disposal");
                Dump();
            }
            
            // Clear the buffer
            _recordBuffer.Clear();
            
            LogInfo("ElasticWriter disposed");
        }
    }
}
