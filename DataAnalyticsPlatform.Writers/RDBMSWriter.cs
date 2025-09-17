/*
 * RDBMSWriter.cs
 * 
 * This class implements a writer component for relational database management systems (RDBMS).
 * It provides functionality to write data records to database tables using a repository pattern.

 */

using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseModel = DataAnalyticsPlatform.Shared.DataAccess.BaseModel;

namespace DataAnalyticsPlatform.Writers
{
    public class RDBMSWriter : BaseWriter
    {
        private IRepository<object> _repository;
        private List<object> _recordBuffer;
        private const int BATCH_SIZE = 500;
        
        public string Schema { get; set; }
        
        public RDBMSWriter(string connectionString, string schema = "") : base(connectionString, Shared.Types.DestinationType.RDBMS)
        {
            LogInfo($"Initializing RDBMSWriter with schema '{(string.IsNullOrEmpty(schema) ? "default" : schema)}'");
            Schema = schema;
            _repository = new PgRepository<object>(connectionString, Schema);
            _recordBuffer = new List<object>();
        }

        public override void Dispose()
        {
            LogInfo("Disposing RDBMSWriter");
            
            if (_recordBuffer.Count > 0)
            {
                LogInfo($"Writing {_recordBuffer.Count} remaining records during disposal");
                Dump();
                _recordBuffer.Clear();
            }

            if (Helper.FileNametoId.Count > 0)
            {
                LogInfo($"Writing {Helper.FileNametoId.Count} file mappings during disposal");
                foreach (var kvp in Helper.FileNametoId)
                {
                    _recordBuffer.Add(new FileNames(kvp.Value, kvp.Key, DateTime.Now.ToString()));
                }
                _repository.CreateTables(_recordBuffer, Schema);
                Dump();
            }
            
            _repository.Dispose();
            LogInfo("RDBMSWriter disposed");
        }

        public override Dictionary<string, long?> DataSize()
        {
            return null;
        }
        
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            LogInfo("CreateTables(List<BaseModel>) called but not implemented");
            return false;
        }
        
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            LogInfo($"Creating tables in database '{db}', schema '{schema}', table '{table}'");
            Schema = schema;
            var ret = _repository.CreateSchema(schema);
            _repository.CreateTables(model, schema);
            LogInfo("Tables created successfully");
            return true;
        }
        public override void Write(IRecord record)
        {
            if (record == null)
            {
                LogError("Write(IRecord) called with null record");
                return;
            }
            
            try
            {
                if (record is IEnumerable)
                {
                    var list = ((IEnumerable<object>)record);
                    _recordBuffer.AddRange(list);
                    LogInfo($"Added enumerable to record buffer, current size: {_recordBuffer.Count}");
                }
                else
                {
                    _recordBuffer.Add(record.Instance);
                    LogInfo($"Added record to buffer, current size: {_recordBuffer.Count}");
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
                if (_recordBuffer.OfType<BaseModel>().Any())
                {
                    var groupList = _recordBuffer.GroupBy(x => ((BaseModel)x).ModelName);
                    foreach (var perGroup in groupList)
                    {
                        LogInfo($"Writing {perGroup.Count()} records for model '{perGroup.Key}'");
                        foreach (object obj in perGroup)
                        {
                            _repository.Insert(obj);
                        }
                    }
                }
                else
                {
                    LogInfo($"Writing {_recordBuffer.Count} generic records");
                    foreach (object obj in _recordBuffer)
                    {
                        _repository.Insert(obj);
                    }
                }
                LogInfo("Records written successfully");
            }
            catch (Exception ex)
            {
                LogError($"Error writing records to database: {ex.Message}");
                throw;
            }
        }
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
                    var list = ((IEnumerable<object>)record);
                    _recordBuffer.AddRange(list);
                    LogInfo($"Added enumerable to record buffer, current size: {_recordBuffer.Count}");
                }
                else
                {
                    _recordBuffer.Add(record);
                    LogInfo($"Added record to buffer, current size: {_recordBuffer.Count}");
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
        public override void Write(List<BaseModel> records)
        {
            LogInfo("Write(List<BaseModel>) called but not implemented");
        }
        
        public override void Write(List<object> records)
        {
            if (records == null || records.Count == 0)
            {
                LogInfo("Write(List<object>) called with empty or null records");
                return;
            }
            
            LogInfo($"Writing {records.Count} objects and creating tables if needed");
            
            try
            {
                _recordBuffer.AddRange(records);
                _repository.CreateTables(_recordBuffer, Schema, true);

                if (_recordBuffer.Count >= BATCH_SIZE)
                {
                    LogInfo($"Buffer size ({_recordBuffer.Count}) exceeds batch size ({BATCH_SIZE}), writing to database");
                    Dump();
                    _recordBuffer.Clear();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error writing List<object>: {ex.Message}");
                throw;
            }
        }
    }
}
