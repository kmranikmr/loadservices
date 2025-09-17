/*
 * CsvReader.cs
 * 
 * This file defines the CsvReader class responsible for reading CSV files and converting 
 * records to structured data objects within the Data Analytics Platform.
 * 
 * Author: Data Analytics Platform Team
 * Last Modified: September 17, 2025
 */

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataAnalyticsPlatform.Readers
{
    /// <summary>
    /// Custom converter for handling integer values where hyphens or empty strings should be converted to zero.
    /// </summary>
    public class HyphenToZeroConverter : Int32Converter
    {
        /// <summary>
        /// Converts a string to an integer, treating hyphens and other non-numeric values as zero.
        /// </summary>
        /// <param name="text">The string to convert</param>
        /// <param name="row">The current row being read</param>
        /// <param name="memberMapData">Mapping metadata for the current field</param>
        /// <returns>An integer value; zero for invalid numeric formats</returns>
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text.Trim() == "-" || 
                (text.Any(x => Char.IsLetter(x))) || 
                text.Contains("-") || 
                string.IsNullOrEmpty(text) || 
                string.IsNullOrEmpty(text.Trim()))
            {
                return 0;
            }

            return base.ConvertFromString(text.Trim(), row, memberMapData);
        }
    }

    /// <summary>
    /// Custom converter for handling double values where hyphens or empty strings should be converted to zero.
    /// </summary>
    public class HyphenToZeroConverterDouble : DoubleConverter
    {
        /// <summary>
        /// Converts a string to a double, treating hyphens and other non-numeric values as zero.
        /// </summary>
        /// <param name="text">The string to convert</param>
        /// <param name="row">The current row being read</param>
        /// <param name="memberMapData">Mapping metadata for the current field</param>
        /// <returns>A double value; zero for invalid numeric formats</returns>
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            string trimmed = text.Trim();
            
            // Handle special cases that should return zero
            if (trimmed == "-" || 
                (trimmed.Any(x => Char.IsLetter(x))) || 
                (trimmed.Length <= 1 && trimmed.Contains("-")) || 
                string.IsNullOrEmpty(text) || 
                string.IsNullOrEmpty(trimmed))
            {
                return 0.0;
            }

            // Handle numbers with hyphens in the middle (take only the portion before the hyphen)
            int pos = trimmed.IndexOf("-");
            if (pos != -1 && pos > 1)
            {
                double.TryParse(
                    trimmed.Substring(0, pos - 1), 
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | 
                    NumberStyles.Number | NumberStyles.Integer | NumberStyles.Float, 
                    CultureInfo.CurrentCulture, 
                    out double val
                );
                return val;
            }

            return base.ConvertFromString(text.Trim(), row, memberMapData);
        }
    }
    /// <summary>
    /// Reader for CSV files that implements the BaseReader interface for the Data Analytics Platform.
    /// Handles parsing, configuration, and extraction of data records from CSV files.
    /// </summary>
    public class CsvReader : BaseReader
    {
        private StreamReader _streamReader = null;
        private CsvHelper.CsvReader _csvDataReader = null;
        private long RecordId { get; set; }
        private CsvReaderConfiguration _csvReaderConfiguration;
        private int _fileId { get; set; }
        private int _recordCount = 0;

        /// <summary>
        /// Initializes a new instance of the CsvReader class with the specified configuration.
        /// </summary>
        /// <param name="conf">Configuration for the reader including file path and model mapping</param>
        public CsvReader(ReaderConfiguration conf) : base(conf)
        {
            LogInfo($"Initializing CsvReader with source path: {conf.SourcePath}");
            
            try
            {
                _streamReader = new StreamReader(conf.SourcePath);
                _fileId = Helper.GetFileId(conf.SourcePath);
                _csvDataReader = new CsvHelper.CsvReader(_streamReader);

                // Get CSV-specific configuration if available
                if (conf.ConfigurationDetails != null)
                {
                    LogInfo($"CsvReader using configuration: {conf.ConfigurationDetails.readerName}");
                    _csvReaderConfiguration = (CsvReaderConfiguration)conf.ConfigurationDetails;
                }

                // Skip header lines if specified
                int skipLines = _csvReaderConfiguration != null ? _csvReaderConfiguration.skipLines : 0;
                if (skipLines > 0)
                {
                    LogInfo($"Skipping {skipLines} lines at beginning of file");
                    for (var i = 0; i < skipLines; i++)
                    {
                        _csvDataReader.Read();
                    }
                }

                // Configure CSV parser if a model map is provided
                if (conf.ModelMap != null)
                {
                    LogInfo("Configuring CSV parser with model mapping");
                    ConfigureCsvParser();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error initializing CsvReader: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Configures the CSV parser based on the current configuration.
        /// </summary>
        private void ConfigureCsvParser()
        {
            // Basic configuration
            _csvDataReader.Configuration.IgnoreBlankLines = true;
            
            // Remove whitespace from headers for matching
            _csvDataReader.Configuration.PrepareHeaderForMatch = (header, index) => { 
                return Regex.Replace(header, @"\s", string.Empty); 
            };
            
            // Disable validation callbacks
            _csvDataReader.Configuration.HeaderValidated = null;
            _csvDataReader.Configuration.MissingFieldFound = null;
            _csvDataReader.Configuration.BadDataFound = null;
            
            // Configure quote handling
            if (_csvReaderConfiguration != null)
            {
                if (!string.IsNullOrEmpty(_csvReaderConfiguration.quotes))
                {
                    _csvDataReader.Configuration.IgnoreQuotes = false;
                }
                else if (_csvReaderConfiguration.quotes?.Trim() == "-")
                {
                    _csvDataReader.Configuration.IgnoreQuotes = true;
                }

                // Configure delimiter (special handling for tabs)
                if (_csvReaderConfiguration.delimiter?.Contains("tab") == true)
                {
                    _csvDataReader.Configuration.Delimiter = "\t";
                    LogInfo("Using tab as delimiter");
                }
                else
                {
                    _csvDataReader.Configuration.Delimiter = _csvReaderConfiguration.delimiter;
                    LogInfo($"Using '{_csvReaderConfiguration.delimiter}' as delimiter");
                }
            }
            
            // Configure trimming and type converters
            _csvDataReader.Configuration.TrimOptions = TrimOptions.Trim;
            _csvDataReader.Configuration.TypeConverterCache.AddConverter(typeof(int), new HyphenToZeroConverter());
            _csvDataReader.Configuration.TypeConverterCache.AddConverter(typeof(double), new HyphenToZeroConverterDouble());
            
            // Register class map
            _csvDataReader.Configuration.RegisterClassMap(GetConfiguration().ModelMap);
        }

        /// <summary>
        /// Gets the next record from the CSV file.
        /// </summary>
        /// <param name="record">The record object that will contain the data</param>
        /// <returns>True if a record was successfully read, false otherwise</returns>
        public override bool GetRecords(out IRecord record)
        {
            bool result = false;
            
            try
            {
                if (_csvDataReader.Read())
                {
                    // Get record using the configured model type
                    var rec = _csvDataReader.GetRecord(GetConfiguration().ModelType);
                    
                    // Create a record wrapper
                    record = new SingleRecord(rec);
                    record.FileId = _fileId;
                    record.RecordId = ++_recordCount;
                    
                    LogInfo($"Read record #{_recordCount}");
                    result = true;
                }
                else
                {
                    LogInfo($"End of file reached after reading {_recordCount} records");
                    record = null;
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error reading record: {ex.Message}");
                record = null;
                return true; // Continue reading despite error
            }
            
            return result;
        }
        /// <summary>
        /// Gets the next record from the CSV file using a specified type.
        /// </summary>
        /// <param name="record">The record object that will contain the data</param>
        /// <param name="t">The type to use for parsing</param>
        /// <returns>True if a record was successfully read, false otherwise</returns>
        public override bool GetRecords(out IRecord record, Type t)
        {
            bool result = false;
            
            try
            {
                if (_csvDataReader.Read())
                {
                    // Get record using the configured model type
                    var rec = _csvDataReader.GetRecord(GetConfiguration().ModelType);
                    
                    // Create a record wrapper
                    record = new SingleRecord(rec);
                    record.FileId = _fileId;
                    record.RecordId = ++_recordCount;
                    
                    LogInfo($"Read record #{_recordCount} as {t.Name}");
                    result = true;
                }
                else
                {
                    LogInfo($"End of file reached after reading {_recordCount} records");
                    record = null;
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error reading record as {t.Name}: {ex.Message}");
                record = null;
                return false;
            }
            
            return result;
        }

        /// <summary>
        /// Provides a preview of data as a DataTable.
        /// </summary>
        /// <param name="size">The number of rows to preview</param>
        /// <returns>A DataTable containing preview data</returns>
        public override DataTable Preview(int size)
        {
            LogInfo($"Generating preview with {size} rows");
            DataTable dt = new DataTable();
            
            try
            {
                // TODO: Implement preview functionality by reading the first 'size' rows
                // and converting them to a DataTable format
            }
            catch (Exception ex)
            {
                LogError($"Error generating preview: {ex.Message}");
            }
            
            return dt;
        }

        /// <summary>
        /// Disposes of resources used by the reader.
        /// </summary>
        private void Dispose()
        {
            LogInfo("Disposing CSV reader resources");
            
            if (_csvDataReader != null)
            {
                _csvDataReader.Dispose();
                _csvDataReader = null;
            }
            
            if (_streamReader != null)
            {
                _streamReader.Dispose();
                _streamReader = null;
            }
        }
    }
}
