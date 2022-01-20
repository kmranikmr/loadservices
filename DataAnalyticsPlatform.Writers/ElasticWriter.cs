using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;

namespace DataAnalyticsPlatform.Writers
{
    public class ElasticWriter : BaseWriter
    {
        protected readonly IElasticClient Client;
        public List<object> _mylist;

        public ElasticWriter(string connectionString) : base(connectionString, Shared.Types.DestinationType.ElasticSearch)
        {
            _mylist = new List<object>();
            Client = CreateClient(connectionString);
        }
        public IElasticClient CreateClient(string connectionString)
        {
            var node = new Uri(connectionString);
            var connectionPool = new SingleNodeConnectionPool(node);
            var connectionSettings = new ConnectionSettings(connectionPool,
           sourceSerializer: (builtin, setting) => new JsonNetSerializer(
            builtin, setting, () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }
          )).DisableDirectStreaming().DefaultMappingFor<BaseModel> (m => m
        .Ignore(p => p.Props).Ignore(p=>p.Values)
    );
;
            return new ElasticClient(connectionSettings);
        }
        public override bool CreateTables(List<object> model, string db, string schema, string table)
        {
            return false;
        }
        public override bool CreateTables(List<BaseModel> model, string db, string schema, string table)
        {
            return false;
        }
        public override void Write(List<BaseModel> record)
        {
           
        }
        public override Dictionary<string, long?> DataSize()
        {
            return null;
        }
        public override void Write(IRecord record)
        {
            //LogInfo(Helper.GetJson(record.Instance));
        }
        public override void Write(List<object> record)
        {
            _mylist.AddRange(record);

            if (_mylist.Count >= 10)
            {
                Dump();
                _mylist.Clear();
            }
            //LogInfo(Helper.GetJson(record));
        }
        public override void Write(object record)
        {
            if (record is IEnumerable)
            {
                var list = ((IEnumerable<BaseModel>)record);
                
                _mylist.AddRange(list);
            }
            else
            {
                _mylist.Add((BaseModel)record);
            }
            
            if (_mylist.Count >= 100)
            {
               // _mylist.ForEach(x => { ((BaseModel)x).Props = null; ((BaseModel)x).Values = null; });
                Dump();
                _mylist.Clear();
            }
            //LogInfo(Helper.GetJson(record));
        }
        public void Dump()
        {
            string indexx = SchemaName +"." +((BaseModel)_mylist[0]).ModelName;
           
            var waitHandle = new CountdownEvent(1);
            var bulkall = Client.BulkAll(_mylist, b => b
            .Index(indexx.ToLower())
            .BackOffRetries(2)
            .BackOffTime("30s")
            .RefreshOnCompleted(true)
            .MaxDegreeOfParallelism(4)
            .Size(100)
            );

            bulkall.Subscribe(new BulkAllObserver(
                onNext: (b) => { Console.Write("."); },
                onError: (e) => { throw e; },
                onCompleted: () => waitHandle.Signal()
                ));//have to change this error to actor supervisory strategy

            waitHandle.Wait();
        }

        public void Dump1()
        {
            string indexx = ((BaseModel)_mylist[0]).ModelName;
            List<string> errors = new List<string>();
            int seenPages = 0;
            int requests = 0;
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ConcurrentBag<BulkResponse> bulkResponses = new ConcurrentBag<BulkResponse>();
            ConcurrentBag<BulkAllResponse> bulkAllResponses = new ConcurrentBag<BulkAllResponse>();
            ConcurrentBag<object> deadLetterQueue = new ConcurrentBag<object>();
            var observableBulk = Client.BulkAll(_mylist, f => f
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    .BulkResponseCallback(r =>
                    {
                        bulkResponses.Add(r);
                        Interlocked.Increment(ref requests);
                    })
                    .ContinueAfterDroppedDocuments()
                    .DroppedDocumentCallback((r, o) =>
                    {
                        errors.Add(r.Error.Reason);
                        deadLetterQueue.Add(o);
                    })
                    .BackOffTime(TimeSpan.FromSeconds(5))
                    .BackOffRetries(2)
                    .Size(1000)
                    .RefreshOnCompleted()
                    .Index(indexx)
                    .BufferToBulk((r, buffer) => r.IndexMany(buffer))
                , tokenSource.Token);

            try
            {
                observableBulk.Wait(TimeSpan.FromMinutes(15), b =>
                {
                    bulkAllResponses.Add(b);
                    Interlocked.Increment(ref seenPages);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Exxx => " + e.Message);
            }
            foreach (var err in errors)
            {
                Console.WriteLine("Error : " + err);
            }
        }
        public override void Dispose()
        {
            if (_mylist.Count > 0)
                Dump();
        }
    }
}
