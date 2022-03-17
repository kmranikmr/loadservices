﻿using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Interfaces;
//using DataAnalyticsPlatform.Shared.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using DataAnalyticsPlatform.Shared.DataAccess;
using System.Collections;
using System.Linq;

namespace DataAnalyticsPlatform.Writers
{
    public class MongoWriter : BaseWriter
    {

        MongoClient _client = null;

        IMongoDatabase _database = null;

        //IMongoCollection<Entity> _collection;
        Dictionary<string, IMongoCollection<object>> _collection;

        public Dictionary<string, List<object>> _mylistDict;

        public MongoWriter(string connectionString) : base(connectionString, Shared.Types.DestinationType.Mongo)
        {
            _client = new MongoClient(new MongoUrl(connectionString));

            _database = _client.GetDatabase("dap");//_database.GetCollection<Entity>("table");

            _collection = new Dictionary<string, IMongoCollection<object>>();
            _mylistDict = new Dictionary<string, List<object>>();
        }
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            return true;
        }
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            return false;
        }

        public override Dictionary<string, long?> DataSize()
        {
            Dictionary<string, long?> temp = new Dictionary<string, long?>();
            temp.Add("dap", 1000);
            return temp;
        }
        public override void Write(IRecord record)
        {

            //_collection.InsertOne((BaseModel)record.Instance);
        }
        public override void Write(object record)
        {
            if (record is IEnumerable)
            {
                var list = ((IEnumerable<BaseModel>)record);
              
               
                //_list.GetEnumerator().MoveNext();
                string tableName = SchemaName + ((BaseModel)list.ElementAt(0)).ModelName;
                if  (!_mylistDict.ContainsKey(tableName))
                {
                    _mylistDict.Add(tableName, new List<object>());
                }
                _mylistDict[tableName].AddRange(list);
                
                if (_mylistDict[tableName].Count > 100)
                {
                    Dump(tableName);
                    _mylistDict[tableName].Clear();
                }
            }
            else
            {
                string tableName = SchemaName + ((BaseModel)record).ModelName;
                if (!_mylistDict.ContainsKey(tableName))
                {
                    _mylistDict.Add(tableName, new List<object>());
                }
                _mylistDict[tableName].Add((BaseModel)record);
                if (_mylistDict[tableName].Count > 100)
                {
                    Dump(tableName);
                    _mylistDict[tableName].Clear();
                }
            }

            //if (_mylist.Count >= 100)
            //{
            //    // _mylist.ForEach(x => { ((BaseModel)x).Props = null; ((BaseModel)x).Values = null; });
            //    Dump();
            //    _mylist.Clear();
            //}
            // _collection.InsertOne(record);
        }
        public override void Write(List<BaseModel> record)
        {
           
        }
        public override void Write(List<object> record)
        {
            Write(record);
            //if (!_mylistDict.ContainsKey(tableName))
            //{
            //}
            //_mylist.AddRange(record);

            //if (_mylist.Count >= 10)
            //{
            //    Dump();
            //    _mylist.Clear();
            //}
            ///_collection.InsertManyAsync(record);
        }

        public void Dump(string tableName)
        {
            Console.WriteLine("Dump");
            _database.GetCollection<object>(tableName).InsertManyAsync(_mylistDict[tableName]);
            Console.WriteLine("DumpDone");
            //_collection.InsertManyAsync(_mylist);
        }
        public override void Dispose()
        {
            
            foreach (var kvp in _mylistDict)
            {
                Console.WriteLine("Dispose" + kvp.Key);
                if (kvp.Value.Count > 0)
                {
                    Dump(kvp.Key);
                }
            }
        }
    }
}
