/* WriterManager:
 * - Implements Akka.NET's ReceiveActor to manage messages related to writing data records and managing the WriterActor pool.
 * - Initializes with an IngestionJob to configure and instantiate writers based on WriterConfiguration.
 * - Manages a pool of WriterActor instances (_writerPool) to distribute write tasks using consistent hashing.
 * - Handles messages such as WriteRecord, NoMoreTransformRecord, WriterManagerDead, and List<ModelSizeData>.
 * - Manages table creation and ensures tables are created only once per model type using a dictionary (TablesCreated).
 * - Forwards write requests to the appropriate writer pool based on the type of data received (record, model list, or objects).
 * - Handles shutdown and cleanup by stopping the _writerPool and notifying the coordinator actor about model size data.
 * 
 * Overall, this actor coordinates and optimizes the writing of data records and models to their respective destinations within the Data Analytics Platform.
 */

using Akka.Actor;
using Akka.Routing;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static DataAnalyticsPlatform.Actors.Processors.WriterActor;

namespace DataAnalyticsPlatform.Actors.Processors
{
    public class NoMoreTransformRecord { }

    public class CreateSchemaPostgres
    {
        public CreateSchemaPostgres(string schema, string connectionString)
        {
            SchemaName = schema;
            ConnectionString = connectionString;
        }
        public string SchemaName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class WriterManager : ReceiveActor
    {
        public class WriterManagerDead { }

        private readonly string WriterActor = "WriterActor";
        private readonly IngestionJob _ingestionJob;
        public IActorRef _writerPool = ActorRefs.Nobody;
        private int _recordCount = 0;
        private IWriter _writer;
        private readonly IActorRef _coordinatorActor;
        private readonly Dictionary<string, bool> TablesCreated;
        public string _schemaName { get; set; }
        private readonly ILogger<WriterManager> _logger;

        public WriterManager(IngestionJob job, IActorRef coordinatorActor)
        {
            _ingestionJob = job;
            _coordinatorActor = coordinatorActor;
            _logger = DataAnalyticsPlatform.Shared.Logging.LoggerFactory.CreateLogger<WriterManager>();

            // Prepare schema name
            _schemaName = job.ReaderConfiguration.TypeConfig.SchemaName.Replace(" ", string.Empty);
            _schemaName = job.ReaderConfiguration.ProjectId != -1
                ? $"{_schemaName}_{job.ReaderConfiguration.ProjectId}_{job.UserId}"
                : _schemaName;

            _logger.LogInformation("WriterManager initialized with schema: {SchemaName}", _schemaName);

            if (_ingestionJob.WriterConfiguration == null)
            {
                _logger.LogWarning("WriterManager received null WriterConfiguration");
            }

            // Initialize writer for non-ElasticSearch destinations
            foreach (var writerConf in _ingestionJob.WriterConfiguration)
            {
                if (writerConf.DestinationType != Shared.Types.DestinationType.ElasticSearch)
                {
                    writerConf.SchemaName = _schemaName;
                    _writer = Writers.Factory.GetWriter(writerConf);
                    _writer.SchemaName = !string.IsNullOrEmpty(_schemaName) ? _schemaName : "public";
                    _logger.LogInformation("WriterManager writer schema set to: {WriterSchema}", _writer.SchemaName);
                }
            }

            TablesCreated = new Dictionary<string, bool>();
            SetReceiveBlocks();
        }

        /// <summary>
        /// Sets up message handlers for the actor.
        /// </summary>
        private void SetReceiveBlocks()
        {
            // Handles WriteRecord messages
            Receive<WriteRecord>(msg =>
            {
                _recordCount++;

                if (msg.Model != null)
                {
                    _logger.LogDebug("Model list received for writing.");
                    string key = ((BaseModel)msg.Model[0]).ModelName.Remove(0, 5);
                    int keyInt = Convert.ToInt32(key);
                    var hashMsg = new ConsistentHashableEnvelope(msg, keyInt * 1000);
                    _writerPool.Tell(hashMsg);
                }
                else if (msg.Objects != null)
                {
                    var lists = (IEnumerable<BaseModel>)msg.Objects;
                    var grouped = lists.GroupBy(y => y.ModelName);

                    foreach (var list in grouped)
                    {
                        string key = list.ElementAt(0).ModelName;
                        if (!TablesCreated.ContainsKey(key))
                        {
                            // Table creation logic can be added here if needed
                            TablesCreated.Add(key, true);
                        }
                        _writerPool.Tell(new WriteRecord(list));
                    }
                }
                else if (msg.Record != null)
                {
                    _logger.LogDebug("Single record received for writing.");
                    string key = _ingestionJob.ReaderConfiguration.TypeConfig.SchemaName;
                    if (!TablesCreated.ContainsKey(key))
                    {
                        var obj = new List<object> { msg.Record.Instance };
                        // Table creation logic can be added here if needed
                        TablesCreated.Add(key, true);
                    }
                    _writerPool.Tell(msg);
                }
            });

            // Handles NoMoreTransformRecord messages
            Receive<NoMoreTransformRecord>(_ =>
            {
                _logger.LogInformation("NoMoreTransformRecord received.");
                if (_writerPool == ActorRefs.Nobody)
                {
                    _logger.LogWarning("WriterManager _writerPool is null.");
                }
                _writerPool.Tell(_);
            });

            // Handles WriterManagerDead messages
            Receive<WriterManagerDead>(_ =>
            {
                _logger.LogInformation("WriterManagerDead received. Stopping self.");
                Self.Tell(PoisonPill.Instance);
            });

            // Forwards model size data to coordinator
            Receive<List<ModelSizeData>>(data =>
            {
                _coordinatorActor.Tell(data);
            });
        }

        /// <summary>
        /// Initializes the writer pool actor on start.
        /// </summary>
        protected override void PreStart()
        {
            if (Context.Child(WriterActor).Equals(ActorRefs.Nobody))
            {
                _writerPool = Context.ActorOf(
                    Props.Create(() => new WriterActor(_ingestionJob)),
                    "writerpool"
                );
                Context.WatchWith(_writerPool, new WriterManagerDead());
                _logger.LogInformation("Writer pool actor created and watched.");
            }
        }
    }

    /// <summary>
    /// Message for consistent hashing based on model name.
    /// </summary>
    internal class ModelMsg : IConsistentHashable
    {
        public string ModelName { get; }
        public BaseModel Model { get; }

        public ModelMsg(string modelName, BaseModel model)
        {
            ModelName = modelName;
            Model = model;
        }

        public object ConsistentHashKey => ModelName;
    }
}
