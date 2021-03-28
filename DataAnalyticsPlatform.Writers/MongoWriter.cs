using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Interfaces;
//using DataAnalyticsPlatform.Shared.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

using DataAnalyticsPlatform.Shared.DataModels;
//using DataAnalyticsPlatform.Common;
using Entity = DataAnalyticsPlatform.Shared.DataModels.Entity;
using DataAnalyticsPlatform.SharedUtils;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared.DataAccess;

namespace DataAnalyticsPlatform.Writers
{
    public class MongoWriter : BaseWriter
    {

        MongoClient _client = null;

        IMongoDatabase _database = null;

        //IMongoCollection<Entity> _collection;
        IMongoCollection<object> _collection;
        public MongoWriter(string connectionString) : base(connectionString, Shared.Types.DestinationType.Mongo)
        {
            _client = new MongoClient(new MongoUrl(connectionString));

            _database = _client.GetDatabase("growApp");

            _collection = _database.GetCollection<object>("table");//_database.GetCollection<Entity>("table");

        }
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            return true;
        }
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            return false;
        }
        public override void Write(IRecord record)
        {

            _collection.InsertOne((BaseModel)record.Instance);
        }
        public override void Write(object record)
        {

            _collection.InsertOne(record);
        }
        public override void Write(List<BaseModel> record)
        {
           
        }
        public override void Write(List<object> record)
        {

            _collection.InsertManyAsync(record);
        }
        public override void Dispose()
        {
        }
    }
}
