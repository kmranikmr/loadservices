using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DataAnalyticsPlatform.Shared;
//using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataAnalyticsPlatform.Readers
{
    public class HyphenToZeroConverter : Int32Converter
    {

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            //Console.WriteLine(memberMapData.Index + " " + text);
            if (text.Trim() == "-" || (text.Any(x => Char.IsLetter(x))) || text.Contains("-") || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text.Trim()))
            {
                return 0;
            }

            return base.ConvertFromString(text.Trim(), row, memberMapData);
        }
    }

   
    public class HyphenToZeroConverterDouble : DoubleConverter
    {

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            //Console.WriteLine(memberMapData.Index + " " + text);
            string trimmed = text.Trim();
            if (trimmed == "-" ||(trimmed.Any(x => Char.IsLetter(x))) ||(trimmed.Length <= 1 && trimmed.Contains("-") ) || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(trimmed))
            {
                return 0.0;
            }

            int pos = trimmed.IndexOf("-");
            if ( pos != -1 && pos > 1)
            {
                double.TryParse(trimmed.Substring(0, pos - 1), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | 
                    NumberStyles.Number | NumberStyles.Integer | NumberStyles.Float, CultureInfo.CurrentCulture, out double val);
                return val;
            }

            return base.ConvertFromString(text.Trim(), row, memberMapData);
        }
    }
    public class CsvReader : BaseReader
    {
        private StreamReader _streamReader = null;

        private CsvHelper.CsvReader _csvDataReader = null;

        private long RecordId { get; set; }

        private CsvReaderConfiguration csvReaderConfiguration;

        private int FileId { get; set;}

        public CsvReader(ReaderConfiguration conf) : base (conf)
        {
            Console.WriteLine("CsvReader Conf" + conf.SourcePath);
            _streamReader = new StreamReader(conf.SourcePath);

            FileId = Helper.GetFileId(conf.SourcePath);

            _csvDataReader = new CsvHelper.CsvReader(_streamReader);

            if ( conf.ConfigurationDetails != null )
            {
                Console.WriteLine("CsvReader ConfigurationDetails" + conf.ConfigurationDetails.readerName);
                csvReaderConfiguration  = (CsvReaderConfiguration)conf.ConfigurationDetails;
            }
            int skipLines = csvReaderConfiguration != null ? csvReaderConfiguration.skipLines : 0;
            for (var i = 0; skipLines > 0 && i < skipLines; i++)
            {
                _csvDataReader.Read();
            }

            if (conf.ModelMap != null)
            {
                Console.WriteLine("CsvReader ModelMap is Available");


                _csvDataReader.Configuration.IgnoreBlankLines = true;
                _csvDataReader.Configuration.PrepareHeaderForMatch = (header, index) => { var headervar = Regex.Replace(header, @"\s", string.Empty); return headervar; };
                _csvDataReader.Configuration.HeaderValidated = null;
                _csvDataReader.Configuration.MissingFieldFound = null;
                if (!string.IsNullOrEmpty(csvReaderConfiguration.quotes))
                {
                    _csvDataReader.Configuration.IgnoreQuotes = false;
                }
                else
                {
                    _csvDataReader.Configuration.IgnoreQuotes = true;
                }
                if (csvReaderConfiguration.delimiter.Contains("tab"))
                {
                    _csvDataReader.Configuration.Delimiter = "\t";//hack for tab
                }
                else
                {
                    _csvDataReader.Configuration.Delimiter = csvReaderConfiguration.delimiter;
                }
                _csvDataReader.Configuration.TrimOptions = TrimOptions.Trim;
                // List<string> headerRow = csv.Context.HeaderRecord.ToList(); ;
                _csvDataReader.Configuration.TypeConverterCache.AddConverter(typeof(int), new HyphenToZeroConverter());
                _csvDataReader.Configuration.TypeConverterCache.AddConverter(typeof(double), new HyphenToZeroConverterDouble());
                //DataReader.Configuration.TypeConverterCache.AddConverter(typeof(double), 

              _csvDataReader.Configuration.RegisterClassMap(conf.ModelMap);
                
            }

        }

        private int _recordCount = 0;

        public override bool GetRecords(out IRecord record)
        {
            bool result = false;
            try
            {

                if (_csvDataReader.Read())
                {
                   
                    // var t1 = GetConfiguration().ModelType.GetType().UnderlyingSystemType;
                    var rec = _csvDataReader.GetRecord(GetConfiguration().ModelType);//OriginalRecord>();//(

                    record = new SingleRecord(rec);
                    record.FileId = FileId;
                    result = true;
                    _recordCount++;
                    record.RecordId = _recordCount;
                }
                else
                {
                    Console.WriteLine("Error CsvReader GetRecords Count " + _recordCount);
                    record = null;
                    Dispose();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error CsvReader GetRecords " + ex.Message);
                record = null;
                //GetRecords(out record);
                //Dispose();
                return true;
            }
            return result;
        }
        public override bool GetRecords(out IRecord record, Type t) 
        {
            bool result = false;

            if (_csvDataReader.Read())
            {
                // var t1 = GetConfiguration().ModelType.GetType().UnderlyingSystemType;
                var rec = _csvDataReader.GetRecord(GetConfiguration().ModelType);//OriginalRecord>();//(

                record = new SingleRecord(rec);
                record.FileId = FileId;
                result = true;
            }
            else
            {
                record = null;
                Dispose();
            }

            return result;
            
        }

        public override DataTable Preview(int size)
        {
            DataTable dt = new DataTable();

            


            return dt;
        }

        private void Dispose()
        {
            _csvDataReader.Dispose();
            _streamReader.Dispose();
        }
    }
}
