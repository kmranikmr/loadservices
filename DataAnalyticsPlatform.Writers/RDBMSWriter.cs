using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using BaseModel = DataAnalyticsPlatform.Shared.DataAccess.BaseModel;

namespace DataAnalyticsPlatform.Writers
{
    public class RDBMSWriter : BaseWriter
    {
        IRepository<object> repository;
        public List<object> _mylist;
        public string Schema { get; set; }
        public RDBMSWriter(string connectionString, string schema = "") : base(connectionString, Shared.Types.DestinationType.RDBMS)
        {
            Console.WriteLine("Start RDBMSWriter" + connectionString);
            Schema = schema;
            Console.WriteLine("schema " + schema);
            repository = new PgRepository<object>(connectionString, Schema);
            _mylist = new List<object>();
        }

        public override void Dispose()
        {
            // throw new NotImplementedException();
            if (_mylist.Count > 0)
                Dump();
            _mylist.Clear();
           
            if (Helper.FileNametoId.Count > 0)
            {
               
                foreach (var kvp in Helper. FileNametoId)
                {
                    _mylist.Add( new FileNames(kvp.Value, kvp.Key, DateTime.Now.ToString()));
                }
                repository.CreateTables(_mylist, Schema);
                Dump();
            }
            repository.Dispose();
        }

        public override Dictionary<string, long?> DataSize()
        {
            return null;
        }
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            return false;
        }
        public override  bool CreateTables(List<object> model, string db, string schema, string table)
        {
            Schema = schema;
            var ret = repository.CreateSchema(schema);
            repository.CreateTables(model, schema);
            return true;
        }
        public override void Write(IRecord record)
        {
            // Console.WriteLine("Start RDBMSWriter  basic");
            if (record is IEnumerable)
            {
                var list = ((IEnumerable<object>)record);

                _mylist.AddRange(list);
            }
            else
            {
                _mylist.Add(record.Instance);
            }
            //_mylist.Add(record);

           // repository.CreateTables(_mylist, "public", true);

            if (_mylist.Count >= 500)
            {
                Dump();
                _mylist.Clear();
            }
            //throw new NotImplementedException();
        }
        public void Dump()
        {
            try
            {
                if (_mylist.OfType<BaseModel>().Any())
                {
                    var groupList = _mylist.GroupBy(x => ((BaseModel)x).ModelName);
                    foreach (var perGroup in groupList)
                    {
                        foreach (object obj in perGroup)
                        {
                            repository.Insert(obj);
                        }
                    }
                }
                else
                {
                    foreach (object obj in _mylist)
                    {
                        repository.Insert(obj);
                    }
                }
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public override void Write(object record)
        {
           // Console.WriteLine("Start RDBMSWriter 2");
            if (record is IEnumerable)
            {
                var list = ((IEnumerable<object>)record);

                _mylist.AddRange(list);
            }
            else
            {
                _mylist.Add(record);
            }
            //_mylist.Add(record);
            
           // repository.CreateTables(_mylist, "public", true);
            
            if (_mylist.Count >= 500)
            {
                Dump();
                _mylist.Clear();
            }

            //throw new NotImplementedException();
        }
        public override void Write(List<BaseModel> record)
        {
          
        }
        public override void Write(List<object> record)
        {
            _mylist.AddRange(record);

            repository.CreateTables(_mylist, Schema, true);

            if (_mylist.Count >= 500)
            {
                Dump();
                _mylist.Clear();
            }

            //throw new NotImplementedException();
        }
    }
}
