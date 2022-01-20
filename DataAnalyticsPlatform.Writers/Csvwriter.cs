using CsvHelper;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DataAnalyticsPlatform.Writers
{
    public class Csvwriter : BaseWriter
    {
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter = null;
        public string ModelName { get; set; }
        int count = 0;
        private WriterConfiguration _conf;
        public List<object> _mylist;
        public Csvwriter(WriterConfiguration conf) : base(conf.ConnectionString, conf.DestinationType)
        {
            _mylist = new List<object>();
            // _streamWriter = new StreamWriter(conf.ConnectionString + "Model" +);

            //_csvWriter = new CsvWriter(_streamWriter);
            _conf = conf;
            //if (conf.ModelMap != null)
            //{
            //    _csvWriter.Configuration.RegisterClassMap(conf.ModelMap);
            //    _csvWriter.Configuration.IncludePrivateMembers = true ;
            //}
        }
        public override void Write(IRecord record)
        {

           
        }
        public override Dictionary<string, long?> DataSize()
        {
            return null;
        }
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            return false;
        }
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            return true;
        }
        private Dictionary<string, string> GetProperties(object obj)
        {
            var props = new Dictionary<string, string>();
            if (obj == null)
                return props;

            var type = obj.GetType();
            
            foreach (var prop in type.GetProperties())
            {
                var val = prop.GetValue(obj, new object[] { });
                var valStr = val == null ? "" : val.ToString();
                props.Add(prop.Name, valStr);
            }

            return props;
        }
        public void Dump()
        {
            using (var streamwriter = new StreamWriter(_conf.ConnectionString + ((BaseModel)_mylist[0]).ModelName + ".csv", true))
            {
                using (var csvWriter = new CsvWriter(streamwriter))
                {
                    if (_conf.ModelMap != null)
                    {
                        csvWriter.Configuration.RegisterClassMap(_conf.ModelMap);
                    }
                    foreach (object rec in _mylist)
                    {
                        csvWriter.WriteRecord(rec);
                        csvWriter.NextRecord();
                    }
                    csvWriter.FlushAsync();
                }
            }
        }
        public override void Write(List<object> record)
        {
            _mylist.AddRange(record);

            if (_mylist.Count >= 500)
            {
                Dump();
                _mylist.Clear();
            }
        }
        public override void Write(object record)
        {
            if (record is IEnumerable)
            {
                var list = ((IEnumerable<object>)record);

                _mylist.AddRange(list);
            }
            else
            {
                _mylist.Add(record);
            }

            if (_mylist.Count >= 500)
            {
                Dump();
                _mylist.Clear();
            }
            
            //Type t = record.GetType();
            //Type t1 = _csvWriter.GetTypeForRecord(record);
            //object mapperObject = Activator.CreateInstance(record.GetType());
            // Console.WriteLine(mapperObject);
            // Console.WriteLine(record);
            // mapperObject = record;
            // var prop = GetProperties(record);
            //foreach (KeyValuePair<string, List<BaseModel>> kvp in ((Dictionary<string, List<BaseModel>>)records))
            //{
            //    _csvWriter.WriteRecord(kvp);
            //    _csvWriter.NextRecord();
            //    Console.WriteLine(++count);
            //}
            //_csvWriter.Flush();
            //_streamWriter.Close();
            
           
        }
        public override void Write(List<BaseModel> record)
        {
            
        }
        public override void Dispose()
        {
            if (_mylist.Count > 0)
                Dump();
            //_csvWriter.Flush();
            //_streamWriter.Close();

        }
    }
}
