/*
 * Csvwriter.cs
 * 
 * This file implements a CSV writer component that exports data records to CSV files.
 * It provides functionality to write records individually or in batches, with configurable 
 * batch sizes and mapping capabilities.
 
 */

using CsvHelper;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DataAnalyticsPlatform.Writers
{
    /// <summary>
    /// Writer implementation for CSV file output. Inherits from BaseWriter and provides
    /// functionality to export data records to CSV files with configurable options.
    /// </summary>
    public class Csvwriter : BaseWriter
    {
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter = null;
        private WriterConfiguration _config;
        private List<object> _recordBuffer;
        private int _batchSize = 500; // Default batch size
        private int _recordCount = 0;

        /// <summary>
        /// Gets or sets the model name for the CSV file.
        /// </summary>
        public string ModelName { get; set; }
        /// <summary>
        /// Initializes a new instance of the Csvwriter class with the specified configuration.
        /// </summary>
        /// <param name="conf">Configuration for the CSV writer</param>
        public Csvwriter(WriterConfiguration conf) : base(conf.ConnectionString, conf.DestinationType)
        {
            LogInfo($"Initializing CSV writer with destination: {conf.ConnectionString}");
            _recordBuffer = new List<object>();
            _config = conf;
            
            // Set batch size from configuration if available
            if (conf.BatchSize.HasValue && conf.BatchSize.Value > 0)
            {
                _batchSize = conf.BatchSize.Value;
                LogInfo($"Using configured batch size: {_batchSize}");
            }
            else
            {
                LogInfo($"Using default batch size: {_batchSize}");
            }
        }
        
        /// <summary>
        /// Writes a single record to the CSV file.
        /// </summary>
        /// <param name="record">The record to write</param>
        public override void Write(IRecord record)
        {
            // This method is not implemented for CSV writer
            LogInfo("Write(IRecord) called but not implemented for CSV writer");
        }
        /// <summary>
        /// Returns information about the data size.
        /// </summary>
        /// <returns>A dictionary containing data size information</returns>
        public override Dictionary<string, long?> DataSize()
        {
            LogInfo("DataSize method called but not implemented for CSV writer");
            return null;
        }
        
        /// <summary>
        /// Creates tables for the specified models (not applicable for CSV writer).
        /// </summary>
        /// <param name="model">The list of models</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>False as this operation is not supported</returns>
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            LogInfo("CreateTables(List<BaseModel>) called but not implemented for CSV writer");
            return false;
        }
        
        /// <summary>
        /// Creates tables for the specified models (not applicable for CSV writer).
        /// </summary>
        /// <param name="model">The list of model objects</param>
        /// <param name="db">The database name</param>
        /// <param name="schema">The schema name</param>
        /// <param name="table">The table name</param>
        /// <returns>True as placeholder</returns>
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            LogInfo("CreateTables(List<object>) called but not applicable for CSV writer");
            return true;
        }
        /// <summary>
        /// Gets a dictionary of property names and their string values from an object.
        /// </summary>
        /// <param name="obj">The object to extract properties from</param>
        /// <returns>A dictionary of property names and values</returns>
        private Dictionary<string, string> GetProperties(object obj)
        {
            var props = new Dictionary<string, string>();
            if (obj == null)
            {
                LogInfo("GetProperties called with null object");
                return props;
            }

            var type = obj.GetType();
            LogInfo($"Extracting properties from object of type {type.Name}");

            foreach (var prop in type.GetProperties())
            {
                var val = prop.GetValue(obj, new object[] { });
                var valStr = val == null ? "" : val.ToString();
                props.Add(prop.Name, valStr);
            }

            LogInfo($"Extracted {props.Count} properties from object");
            return props;
        }
        
        /// <summary>
        /// Writes the buffered records to the CSV file and clears the buffer.
        /// </summary>
        public void Dump()
        {
            if (_recordBuffer.Count == 0)
            {
                LogInfo("Dump called but no records to write");
                return;
            }
            
            // Determine model name from the first record
            string modelName = ((BaseModel)_recordBuffer[0]).ModelName;
            string filePath = _config.ConnectionString + modelName + ".csv";
            
            LogInfo($"Writing {_recordBuffer.Count} records to {filePath}");
            
            try
            {
                using (var streamWriter = new StreamWriter(filePath, true))
                {
                    using (var csvWriter = new CsvWriter(streamWriter))
                    {
                        // Apply custom mapping if provided
                        if (_config.ModelMap != null)
                        {
                            LogInfo("Applying custom model mapping");
                            csvWriter.Configuration.RegisterClassMap(_config.ModelMap);
                        }
                        
                        // Write each record
                        foreach (object rec in _recordBuffer)
                        {
                            csvWriter.WriteRecord(rec);
                            csvWriter.NextRecord();
                            _recordCount++;
                        }
                        
                        // Ensure all records are written
                        csvWriter.FlushAsync();
                    }
                }
                
                LogInfo($"Successfully wrote {_recordBuffer.Count} records to CSV file. Total records: {_recordCount}");
            }
            catch (System.Exception ex)
            {
                LogError($"Error writing to CSV file {filePath}: {ex.Message}");
            }
        }
        /// <summary>
        /// Writes a list of objects to the CSV file.
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
            if (_recordBuffer.Count >= _batchSize)
            {
                LogInfo($"Buffer reached batch size threshold ({_batchSize}), dumping to file");
                Dump();
                _recordBuffer.Clear();
            }
        }
        
        /// <summary>
        /// Writes a single object or enumerable collection to the CSV file.
        /// </summary>
        /// <param name="record">The object to write</param>
        public override void Write(object record)
        {
            if (record == null)
            {
                LogInfo("Write(object) called with null record");
                return;
            }
            
            // Handle both individual objects and collections
            if (record is IEnumerable && !(record is string))
            {
                LogInfo("Processing enumerable record");
                var list = ((IEnumerable<object>)record);
                _recordBuffer.AddRange(list);
            }
            else
            {
                LogInfo("Processing single record");
                _recordBuffer.Add(record);
            }

            // Check if buffer size exceeds batch size threshold
            if (_recordBuffer.Count >= _batchSize)
            {
                LogInfo($"Buffer reached batch size threshold ({_batchSize}), dumping to file");
                Dump();
                _recordBuffer.Clear();
            }
        }
        /// <summary>
        /// Writes a list of BaseModel objects to the CSV file.
        /// </summary>
        /// <param name="records">The list of BaseModel objects to write</param>
        public override void Write(List<BaseModel> records)
        {
            LogInfo("Write(List<BaseModel>) called but not implemented for CSV writer");
        }
        
        /// <summary>
        /// Releases resources used by the CSV writer and writes any remaining buffered records.
        /// </summary>
        public override void Dispose()
        {
            LogInfo("Disposing CSV writer");
            
            // Write any remaining records in the buffer
            if (_recordBuffer.Count > 0)
            {
                LogInfo($"Writing {_recordBuffer.Count} remaining records during disposal");
                Dump();
            }
            
            // Clear the buffer
            _recordBuffer.Clear();
            
            // Close any open streams
            _streamWriter?.Dispose();
            _csvWriter?.Dispose();
            
            LogInfo($"CSV writer disposed. Total records written: {_recordCount}");
        }
    }
}
