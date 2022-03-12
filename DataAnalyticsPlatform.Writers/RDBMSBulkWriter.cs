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
    public class RDBMSBulkWriter : BaseWriter
    {
        BulkPostgresRepository<BaseModel> repository;
        public List<BaseModel> _mylist;
        public string Schema { get; set; }
        public RDBMSBulkWriter(string connectionString, string schema = "") : base(connectionString, Shared.Types.DestinationType.RDBMS)
        {
            Console.WriteLine("Start RDBMSBulkWriter" + connectionString);
            Schema = schema;
            Console.WriteLine("schema " + schema);
            repository = new BulkPostgresRepository<BaseModel>(connectionString, Schema);
            
            _mylist = new List<BaseModel>();
        }

        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            repository.CreateSchema();
            return true;
        }
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            return false;
        }
        public override void Dispose()
        {
            if (_mylist.Count > 0)
                Dump();
            Console.WriteLine("Disposed");
            _mylist.Clear();
            // throw new NotImplementedException();
            //if (_mylist.Count > 0)
            //     Dump();
            // _mylist.Clear();

            //if (Helper.FileNametoId.Count > 0)
            //{

            //    foreach (var kvp in Helper.FileNametoId)
            //    {
            //        _mylist.Add(new FileNames(kvp.Value, kvp.Key, DateTime.Now.ToString()));
            //    }
            //   // repository.CreateTables(_mylist, Schema);
            //    Dump();
            //}
            // repository.Dispose();
        }

        public override Dictionary<string, long?> DataSize()
        {
            return repository.DataSize();
        }
        public override void Write(object record)
        {
            if (record is IEnumerable)
            {
                var list = ((IEnumerable<BaseModel>)record);
                Console.WriteLine("RDBMS bulk IEnumerable");
                _mylist.AddRange(list);
            }
            else
            {
                Console.WriteLine("RDBMS bulk not IEnumerable" + record.GetType());
                _mylist.Add((BaseModel)record);
            }
            //_mylist.Add(record);

            // repository.CreateTables(_mylist, "public", true);

            if (_mylist.Count >= 500)
            {
                Dump();
                _mylist.Clear();
            }
        }

        public void Dump()
        {
            try
            {
                repository.Write(_mylist);
                Console.Write("*");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void Write(IRecord record)
        {
            Console.WriteLine("RDBMS bulk record no imp" + record.GetType());
        }

        public override void Write(List<object> record)
        {
            Console.WriteLine("RDBMS bulk List<object> record" + record.GetType());
        }
        public override void Write(List<BaseModel> record)
        {
            Console.WriteLine("RDBMS Bulkl writer List<BaseModel> ");

          repository.Write(record);

            //throw new NotImplementedException();
        }
    }
}
