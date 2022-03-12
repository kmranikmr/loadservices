using Akka.Actor;
using Akka.Routing;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static DataAnalyticsPlatform.Actors.Processors.WriterActor;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.DataAccess;

namespace DataAnalyticsPlatform.Actors.Processors
{
    public class NoMoreTransformRecord
    {

    }

    public class CreateSchemaPostgres
    {
        public CreateSchemaPostgres( string schema, string connectionString)
        {
            SchemaName = schema;
            ConnectionString = connectionString;
        }
        public string SchemaName { get; set; }
        public string ConnectionString { get; set; }
    }
    public class WriterManager : ReceiveActor
    {
        public class WriterManagerDead
        {

        }

       

        private string WriterActor = "WriterActor";

        IngestionJob _ingestionJob;
        public IActorRef _writerPool = null;
        private int _recordCount = 0;
        private IWriter _writer;
        private IActorRef _coordinatorActor;
        Dictionary<string, bool> TablesCreated;
        public string _schemaName { get; set; }
        public WriterManager(IngestionJob j, IActorRef cordinatorActor)
        {
            _ingestionJob = j;
            _coordinatorActor = cordinatorActor;
            _schemaName = j.ReaderConfiguration.TypeConfig.SchemaName.Replace(" ", string.Empty);
            _schemaName = j.ReaderConfiguration.ProjectId != -1 ? _schemaName + "_" + j.ReaderConfiguration.ProjectId +"_"+ j.UserId: _schemaName;
            Console.WriteLine("WriterManager _schemaName" + _schemaName);
            if (_ingestionJob.WriterConfiguration == null )
            {
                Console.WriteLine("WriterManager Null WriterConfiguration");
            }
            foreach (var writerConf in _ingestionJob.WriterConfiguration)
            {
                if (writerConf.DestinationType != Shared.Types.DestinationType.ElasticSearch)
                {
                    writerConf.SchemaName = _schemaName;
                    _writer = Writers.Factory.GetWriter(writerConf);
                    _writer.SchemaName = _schemaName != string.Empty ? _schemaName : "public";
                    Console.WriteLine("WriterManager  _writer.SchemaName" + _writer.SchemaName);
                    //_writer.CreateTables((List<BaseModel>)null, "", "", "");
                    // _coordinatorActor.Tell(new CreateSchemaPostgres(_writer.SchemaName, writerConf.ConnectionString));
                }
            }
            TablesCreated = new Dictionary<string, bool>();
            SetReceiveBlocks();
        }

        private void SetReceiveBlocks()
        {
            Receive<WriteRecord>(x =>
            {
                _recordCount++;
              //  Console.WriteLine("WriterManager  WriteRecord");
                if (x.Model != null)
               {
                    Console.WriteLine("model List received");
                    //  var ModelMsg = new ModelMsg(((BaseModel)x.Model).ModelName, (BaseModel)x.Model);
                    string key = ((BaseModel)x.Model[0]).ModelName;
                    key = key.Remove(0,5);
                    int key_int = Convert.ToInt32(key);
                    var hashMsg = new ConsistentHashableEnvelope(x, key_int*1000);
                    _writerPool.Tell(hashMsg);
               }
               else if (x.Objects != null)
                {
                    //Console.WriteLine("WriterManager  Objects");
                    //var Lists = (IEnumerable<object>)x.Objects;
                    var Lists = (IEnumerable<BaseModel>)x.Objects;

                    var Grouped = Lists.GroupBy(y => ((BaseModel)y).ModelName);
                    if (Grouped != null )
                   // Console.WriteLine("WriterManager  Objects Grouped Count" + Grouped.Count());
                    // foreach (IEnumerable<object> list in Grouped)
                    foreach (IEnumerable<BaseModel> list in Grouped)
                    {
                       
                        string key = ((BaseModel)list.ElementAt(0)).ModelName;
                        if (!TablesCreated.TryGetValue(key, out bool truth))
                        {
  
                           // _writer.CreateTables(list.ToList(), "", _writer.SchemaName , key);
                            TablesCreated.Add(key, true);
                        }

                        _writerPool.Tell(new WriteRecord(list));// hashMsg);
                    }
                    
                }
                else if(x.Record != null)
                {
                    Console.WriteLine("WriterManager  Record");
                    string key = _ingestionJob.ReaderConfiguration.TypeConfig.SchemaName;
                    Console.WriteLine("WriterManager  key" + key);
                    if (!TablesCreated.TryGetValue(key, out bool truth))
                    {
                        var obj = new List<object>();
                        obj.Add(x.Record.Instance);
                        // _ingestionJob.ReaderConfiguration.TypeConfig.SchemaName
                       // _writer.CreateTables(obj, "", _writer.SchemaName, key);
                        TablesCreated.Add(key, true);
                    }
                        //var hashMsg = new ConsistentHashableEnvelope((object)x, 1000);
                    _writerPool.Tell(x);
                }

            });

            Receive<NoMoreTransformRecord>(x =>
            {
                Console.WriteLine("NoMoreTransformRecord");
                _writerPool.Tell(x);
            });
            //WriterManagerDead
            Receive<WriterManagerDead>( x=>
            {
                Console.WriteLine("WriterManagerDead");
                Self.Tell(PoisonPill.Instance);
            });
            Receive<List<ModelSizeData>>(x => { _coordinatorActor.Tell(x); });
        }

        //protected override SupervisorStrategy SupervisorStrategy()
        //{
        //    return new OneForOneStrategy(localOnlyDecider: x =>
        //    {
        //        switch (x)
        //        {
        //            case WriterException we:
        //                //update table using dbUtils.              

        //                return Directive.Escalate;

        //        }

        //      //  _logger.Error(x, x.Message);

        //        return Akka.Actor.SupervisorStrategy.DefaultStrategy.Decider.Decide(x);

        //    }, loggingEnabled: true, maxNrOfRetries: 0, withinTimeMilliseconds: 0
        //    );
        //}
        protected override void PreStart()
        {
            if (Context.Child(WriterActor).Equals(ActorRefs.Nobody))
            {
                // _writerPool =Context.ActorOf(Props.Create(() => new WriterActor(_ingestionJob)).WithRouter(new ConsistentHashingPool(10)), "writerpool");

                _writerPool=  Context.ActorOf(Props.Create(() => new WriterActor(_ingestionJob))
                    /*.
                    WithSupervisorStrategy(new OneForOneStrategy(0, 0, ex =>
                    {
                        switch (ex)
                        {
                            case WriterException we:
                              return Directive.Resume;
                        } 
                        return Directive.Resume;
                    })) */
                    
                    ,"writerpool") ;
                Context.WatchWith(_writerPool, new WriterManagerDead());
            }
        }
    }

    internal class ModelMsg : IConsistentHashable
    {
        public string ModelName;
        public BaseModel Model;


        public ModelMsg(string p1, BaseModel p2)
        {
            this.ModelName = p1;
            this.Model = p2;
          
        }
        public object ConsistentHashKey
        {
            get
            {
                return this.ModelName;
            }
        }
    }
}
