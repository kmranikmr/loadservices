/*
 * MongoWriter.cs
 * 
 * This file implements a MongoDB writer component that exports data records to 
 * MongoDB collections using the MongoDB.Driver library. It provides functionality 
 * to write records individually or in batches, with configurable options.
 * 
 * Author: Data Analytics Platform Team
 * Last Modified: September 17, 2025
 */

using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataAnalyticsPlatform.Writers
{
    /// <summary>
    /// Writer implementation for MongoDB output. Inherits from BaseWriter and provides
    /// functionality to export data records to MongoDB collections.
    /// </summary>
    public class MongoWriter : BaseWriter
    {
        private MongoClient _client = null;
        private IMongoDatabase _database = null;
        private Dictionary<string, IMongoCollection<object>> _collections;
        private Dictionary<string, List<object>> _recordBuffers;
        private const int BATCH_SIZE = 100;
        
        /// <summary>
        /// The database name to write to
        /// </summary>
        private readonly string _databaseName = "dap";

        /// <summary>
        /// Initializes a new instance of the MongoWriter class with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The MongoDB connection string</param>
        public MongoWriter(string connectionString) : base(connectionString, Shared.Types.DestinationType.Mongo)
        {
            LogInfo($"Initializing MongoWriter with connection string (URI format obfuscated for security)");
            
            try
            {
                // Use the connection string from configuration, rather than hardcoding
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                _client = new MongoClient(settings);
                
                _database = _client.GetDatabase(_databaseName);
                LogInfo($"Successfully connected to MongoDB database '{_databaseName}'");
                
                _collections = new Dictionary<string, IMongoCollection<object>>();
                _recordBuffers = new Dictionary<string, List<object>>();
            }
            catch (Exception ex)
            {
                LogError($"Error initializing MongoDB connection: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Creates tables for the specified model objects.
        /// </summary>
        /// <param name="model">The list of model objects</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>True as MongoDB will create collections automatically</returns>
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            LogInfo($"CreateTables called for {db}.{schema}.{table} - MongoDB creates collections automatically");
            return true;
        }
        
        /// <summary>
        /// Creates tables for the specified BaseModel objects (not implemented).
        /// </summary>
        /// <param name="model">The list of BaseModel objects</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>False as this specific method is not implemented</returns>
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            LogInfo("CreateTables(List<BaseModel>) called but not implemented for MongoDB");
            return false;
        }

        /// <summary>
        /// Creates a MongoDB collection if it doesn't already exist.
        /// </summary>
        /// <param name="collectionName">The name of the collection to create</param>
        /// <param name="properties">Optional properties for the collection (not currently used)</param>
        public void CreateTable(string collectionName, Dictionary<string, string> properties = null)
        {
            LogInfo($"Creating MongoDB collection: '{collectionName}'");
            
            if (!_collections.ContainsKey(collectionName))
            {
                try
                {
                    _collections.Add(collectionName, _database.GetCollection<object>(collectionName));
                    _recordBuffers.Add(collectionName, new List<object>());
                    LogInfo($"Successfully created MongoDB collection: '{collectionName}'");
                }
                catch (Exception ex)
                {
                    LogError($"Error creating MongoDB collection '{collectionName}': {ex.Message}");
                }
            }
            else
            {
                LogInfo($"Collection '{collectionName}' already exists, skipping creation");
            }
        }
        
        /// <summary>
        /// Returns information about the data size.
        /// </summary>
        /// <returns>A dictionary containing data size information</returns>
        public override Dictionary<string, long?> DataSize()
        {
            LogInfo("DataSize method called");
            Dictionary<string, long?> stats = new Dictionary<string, long?>();
            
            long totalRecords = _recordBuffers.Sum(item => (item.Value?.Count).GetValueOrDefault(0));
            stats.Add($"{_databaseName}_buffer_count", totalRecords);
            
            LogInfo($"Current MongoDB buffer has {totalRecords} records across {_recordBuffers.Count} collections");
            return stats;
        }
        
        /// <summary>
        /// Writes a record to MongoDB.
        /// </summary>
        /// <param name="record">The record to write</param>
        public override void Write(IRecord record)
        {
            if (record == null)
            {
                LogError("Write(IRecord) called with null record");
                return;
            }
            
            try
            {
                string collectionName = SchemaName + "." + record.ModelName;
                LogInfo($"Writing IRecord to collection '{collectionName}'");
                
                // Initialize buffer for this collection if needed
                if (!_recordBuffers.ContainsKey(collectionName))
                {
                    _recordBuffers.Add(collectionName, new List<object>());
                    LogInfo($"Created new buffer for collection '{collectionName}'");
                }
                
                // Add record to the buffer
                _recordBuffers[collectionName].Add(record);
                LogInfo($"Added IRecord to buffer for '{collectionName}', total: {_recordBuffers[collectionName].Count}");
                
                // Write to MongoDB if buffer exceeds batch size
                if (_recordBuffers[collectionName].Count > BATCH_SIZE)
                {
                    LogInfo($"Buffer size ({_recordBuffers[collectionName].Count}) exceeds batch size ({BATCH_SIZE}), writing to MongoDB");
                    Dump(collectionName);
                    _recordBuffers[collectionName].Clear();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error writing IRecord: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes an object (or collection of objects) to MongoDB.
        /// </summary>
        /// <param name="record">The object to write</param>
        public override void Write(object record)
        {
            if (record == null)
            {
                LogError("Write(object) called with null record");
                return;
            }
            
            try
            {
                // Handle both individual objects and collections
                if (record is IEnumerable && !(record is string))
                {
                    var list = ((IEnumerable)record);
                    if (!list.Cast<object>().Any())
                    {
                        LogInfo("Write(object) called with empty enumerable");
                        return;
                    }
                    
                    var firstItem = list.Cast<object>().First();
                    if (firstItem is BaseModel)
                    {
                        string collectionName = SchemaName + "." + ((BaseModel)firstItem).ModelName;
                        LogInfo($"Processing enumerable with {list.Cast<object>().Count()} records for collection '{collectionName}'");
                        
                        // Initialize buffer for this collection if needed
                        if (!_recordBuffers.ContainsKey(collectionName))
                        {
                            _recordBuffers.Add(collectionName, new List<object>());
                            LogInfo($"Created new buffer for collection '{collectionName}'");
                        }
                        
                        // Add records to the buffer
                        _recordBuffers[collectionName].AddRange(list.Cast<object>());
                        LogInfo($"Added {list.Cast<object>().Count()} records to buffer for '{collectionName}', total: {_recordBuffers[collectionName].Count}");
                        
                        // Write to MongoDB if buffer exceeds batch size
                        if (_recordBuffers[collectionName].Count > BATCH_SIZE)
                        {
                            LogInfo($"Buffer size ({_recordBuffers[collectionName].Count}) exceeds batch size ({BATCH_SIZE}), writing to MongoDB");
                            Dump(collectionName);
                            _recordBuffers[collectionName].Clear();
                        }
                    }
                    else
                    {
                        LogError($"Cannot process enumerable with items of type {firstItem.GetType().Name} - BaseModel expected");
                    }
                }
                else if (record is BaseModel)
                {
                    // Process single record
                    string collectionName = SchemaName + "." + ((BaseModel)record).ModelName;
                    LogInfo($"Processing single record for collection '{collectionName}'");
                    
                    // Initialize buffer for this collection if needed
                    if (!_recordBuffers.ContainsKey(collectionName))
                    {
                        _recordBuffers.Add(collectionName, new List<object>());
                        LogInfo($"Created new buffer for collection '{collectionName}'");
                    }
                    
                    // Add record to the buffer
                    _recordBuffers[collectionName].Add(record);
                    LogInfo($"Added record to buffer for '{collectionName}', total: {_recordBuffers[collectionName].Count}");
                    
                    // Write to MongoDB if buffer exceeds batch size
                    if (_recordBuffers[collectionName].Count > BATCH_SIZE)
                    {
                        LogInfo($"Buffer size ({_recordBuffers[collectionName].Count}) exceeds batch size ({BATCH_SIZE}), writing to MongoDB");
                        Dump(collectionName);
                        _recordBuffers[collectionName].Clear();
                    }
                }
                else
                {
                    // Handle generic object - use type name as collection name
                    string collectionName = record.GetType().Name;
                    LogInfo($"Processing object of type '{collectionName}'");
                    
                    // Initialize buffer for this collection if needed
                    if (!_recordBuffers.ContainsKey(collectionName))
                    {
                        _recordBuffers.Add(collectionName, new List<object>());
                        LogInfo($"Created new buffer for collection '{collectionName}'");
                    }
                    
                    // Add record to the buffer
                    _recordBuffers[collectionName].Add(record);
                    LogInfo($"Added record to buffer for '{collectionName}', total: {_recordBuffers[collectionName].Count}");
                    
                    // Write to MongoDB if buffer exceeds batch size
                    if (_recordBuffers[collectionName].Count > BATCH_SIZE)
                    {
                        LogInfo($"Buffer size ({_recordBuffers[collectionName].Count}) exceeds batch size ({BATCH_SIZE}), writing to MongoDB");
                        Dump(collectionName);
                        _recordBuffers[collectionName].Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error writing record: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Writes a list of BaseModel objects to MongoDB.
        /// </summary>
        /// <param name="records">The list of BaseModel objects to write</param>
        public override void Write(List<BaseModel> records)
        {
            LogInfo($"Write(List<BaseModel>) called with {records?.Count ?? 0} records");
            if (records != null && records.Count > 0)
            {
                Write((object)records);
            }
        }

        /// <summary>
        /// Writes the buffered records for a specific collection to MongoDB.
        /// </summary>
        /// <param name="collectionName">The name of the collection to write to</param>
        public void Dump(string collectionName)
        {
            if (!_recordBuffers.ContainsKey(collectionName) || _recordBuffers[collectionName].Count == 0)
            {
                LogInfo($"Dump called for '{collectionName}' but no records to write");
                return;
            }
            
            LogInfo($"Writing {_recordBuffers[collectionName].Count} records to MongoDB collection '{collectionName}'");
            
            try
            {
                // Get or create the collection
                var collection = _database.GetCollection<object>(collectionName);
                
                // Insert the records
                collection.InsertMany(_recordBuffers[collectionName]);
                LogInfo($"Successfully wrote {_recordBuffers[collectionName].Count} records to MongoDB collection '{collectionName}'");
            }
            catch (Exception ex)
            {
                LogError($"Error writing to MongoDB collection '{collectionName}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// Releases resources used by the MongoDB writer and writes any remaining buffered records.
        /// </summary>
        public override void Dispose()
        {
            LogInfo("Disposing MongoWriter");
            
            // Write any remaining records in the buffers
            foreach (var kvp in _recordBuffers)
            {
                if (kvp.Value.Count > 0)
                {
                    LogInfo($"Writing {kvp.Value.Count} remaining records for '{kvp.Key}' during disposal");
                    Dump(kvp.Key);
                }
            }
            
            // Clear the buffers
            _recordBuffers.Clear();
            _collections.Clear();
            
            LogInfo("MongoWriter disposed");
        }
    }
}
