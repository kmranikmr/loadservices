/*
 * RDBMSBulkWriter.cs
 * 
 * This file implements a bulk writer component for relational database management systems (RDBMS).
 * It provides functionality for efficient bulk writing of data records to database tables
 * using the BulkPostgresRepository for optimized bulk operations.
 * 
 * Author: Data Analytics Platform Team
 * Last Modified: September 17, 2025
 */

using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using BaseModel = DataAnalyticsPlatform.Shared.DataAccess.BaseModel;

namespace DataAnalyticsPlatform.Writers
{
    /// <summary>
    /// Writer implementation for RDBMS output with bulk operations. Inherits from BaseWriter and provides
    /// functionality to export data records to relational database tables using bulk operations.
    /// </summary>
    public class RDBMSBulkWriter : BaseWriter
    {
        /// <summary>
        /// The repository for bulk database operations
        /// </summary>
        private BulkPostgresRepository<BaseModel> _repository;
        
        /// <summary>
        /// Buffer of records to be written
        /// </summary>
        private List<BaseModel> _recordBuffer;
        
        /// <summary>
        /// Maximum number of records to buffer before writing
        /// </summary>
        private const int BATCH_SIZE = 500;
        
        /// <summary>
        /// Gets or sets the database schema name
        /// </summary>
        public string Schema { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the RDBMSBulkWriter class with the specified connection string and optional schema.
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="schema">The optional database schema name</param>
        public RDBMSBulkWriter(string connectionString, string schema = "") : base(connectionString, Shared.Types.DestinationType.RDBMS)
        {
            LogInfo($"Initializing RDBMSBulkWriter with schema '{(string.IsNullOrEmpty(schema) ? "default" : schema)}'");
            
            Schema = schema;
            _repository = new BulkPostgresRepository<BaseModel>(connectionString, Schema);
            _recordBuffer = new List<BaseModel>();
            
            LogInfo("RDBMSBulkWriter initialized successfully");
        }

        /// <summary>
        /// Creates database tables for the specified BaseModel objects.
        /// </summary>
        /// <param name="model">The list of BaseModel objects</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>True if tables were created successfully</returns>
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            LogInfo($"Creating schema for database '{db}', schema '{schema}', table '{table}'");
            _repository.CreateSchema();
            LogInfo("Schema created successfully");
            return true;
        }
        
        /// <summary>
        /// Creates tables for the specified objects (not implemented).
        /// </summary>
        /// <param name="model">The list of objects</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>False as this method is not implemented</returns>
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            LogInfo("CreateTables(List<object>) called but not implemented for RDBMSBulkWriter");
            return false;
        }
        
        /// <summary>
        /// Releases resources used by the RDBMS bulk writer and writes any remaining buffered records.
        /// </summary>
        public override void Dispose()
        {
            LogInfo("Disposing RDBMSBulkWriter");
            
            if (_recordBuffer.Count > 0)
            {
                LogInfo($"Writing {_recordBuffer.Count} remaining records during disposal");
                Dump();
            }
           
            _recordBuffer.Clear();
            LogInfo("RDBMSBulkWriter disposed");
        }

        /// <summary>
        /// Returns information about the data size.
        /// </summary>
        /// <returns>A dictionary containing data size information</returns>
        public override Dictionary<string, long?> DataSize()
        {
            LogInfo("DataSize method called");
            return _repository.DataSize();
        }
        
        /// <summary>
        /// Writes an object (or collection of objects) to the RDBMS.
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
                if (record is IEnumerable && !(record is string))
                {
                    var list = ((IEnumerable<BaseModel>)record);
                    LogInfo($"Processing enumerable with {list.Cast<BaseModel>().Count()} records");
                    _recordBuffer.AddRange(list);
                }
                else if (record is BaseModel)
                {
                    LogInfo("Processing single BaseModel record");
                    _recordBuffer.Add((BaseModel)record);
                }
                else
                {
                    LogError($"Unsupported record type: {record.GetType().Name}");
                    return;
                }
                
                if (_recordBuffer.Count >= BATCH_SIZE)
                {
                    LogInfo($"Buffer size ({_recordBuffer.Count}) exceeds batch size ({BATCH_SIZE}), writing to database");
                    Dump();
                    _recordBuffer.Clear();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error writing record: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Writes the buffered records to the database.
        /// </summary>
        public void Dump()
        {
            if (_recordBuffer.Count == 0)
            {
                LogInfo("Dump called but no records to write");
                return;
            }
            
            LogInfo($"Writing {_recordBuffer.Count} records to database");
            
            try
            {
                _repository.Write(_recordBuffer);
                LogInfo($"Successfully wrote {_recordBuffer.Count} records to database");
            }
            catch (Exception ex)
            {
                LogError($"Error writing records to database: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Writes a record to RDBMS (not fully implemented).
        /// </summary>
        /// <param name="record">The record to write</param>
        public override void Write(IRecord record)
        {
            LogInfo($"Write(IRecord) called with record type {record?.GetType().Name ?? "null"} but not fully implemented");
        }

        /// <summary>
        /// Writes a list of objects to RDBMS (not fully implemented).
        /// </summary>
        /// <param name="records">The list of objects to write</param>
        public override void Write(List<object> records)
        {
            LogInfo($"Write(List<object>) called with {records?.Count ?? 0} records but not fully implemented");
        }
        
        /// <summary>
        /// Writes a list of BaseModel objects to RDBMS.
        /// </summary>
        /// <param name="records">The list of BaseModel objects to write</param>
        public override void Write(List<BaseModel> records)
        {
            if (records == null || records.Count == 0)
            {
                LogInfo("Write(List<BaseModel>) called with empty or null records");
                return;
            }
            
            LogInfo($"Writing {records.Count} BaseModel records directly to repository");
            
            try
            {
                _repository.Write(records);
                LogInfo($"Successfully wrote {records.Count} BaseModel records");
            }
            catch (Exception ex)
            {
                LogError($"Error writing BaseModel records: {ex.Message}");
                throw;
            }
        }
    }
}
