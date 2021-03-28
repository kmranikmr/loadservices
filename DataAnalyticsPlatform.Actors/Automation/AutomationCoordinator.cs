using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Writers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors.Automation
{
    public class AutomationCoordinator : ReceiveActor
    {
        public class FolderWatchDead
        {
            public ProjectAutomation AutomationModel { get; private set; }

            public FolderWatchDead(ProjectAutomation model)
            {
                AutomationModel = model;
            }
        }

        public const string DatabaseMonitorActorName = "DatabaseMonitorActor";

        public const string FolderWatcherActor = "FolderWatcherActor";

        private IActorRef _databaseMonitorActor;

        private Dictionary<int, IActorRef> _folderMonitorActors;

        private string _connectionString;

        private string postgresConnection;

        private readonly ILoggingAdapter _logger = Context.GetLogger();

        private Repository _repository;

        public event EventHandler<ProjectFile> AutoAdd;

        public MapperConfiguration mapperConf = null;
               
        public IMapper iMapper { get; set; }

        public IActorRef MasterActor { get; set; }
        public AutomationCoordinator(IActorRef masterRef, string dbConnectionString, string postgresString)
        {
            _connectionString = dbConnectionString;

            Console.WriteLine(dbConnectionString);
            MasterActor = masterRef;

            postgresConnection = postgresString;

            AutoAdd += AddtoProjectFile;

           

            _folderMonitorActors = new Dictionary<int, IActorRef>();

             mapperConf = new MapperConfiguration(cfg => {
                cfg.CreateMap<ProjectSchema, SchemaDTO>();
                 cfg.CreateMap<ProjectFileDTO, ProjectFile>();

             });

             iMapper = mapperConf.CreateMapper();

            
            SetReceivers();
        }

        public async void AddtoProjectFile(object sender, ProjectFile pf)
        //public async void AddtoProjectFile(object sender, ProjectFileDTO pf)
        {
            Console.WriteLine("AddtoProjectFile AddtoProjectFile");
            string connectionString = _connectionString;// "Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";// @"Server = dapdb.cqzm7ymwpoc8.us-east-1.rds.amazonaws.com; Database = dap_master; User Id = admin; Password = dapdata123";//// @"Server = localhost\\SQLEXPRESS; Database = dap_master; Trusted_Connection = True; ";
           
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), connectionString).Options;
            var dbContext = new DAPDbContext(options, connectionString);
         
            _repository = new Repository(dbContext, null);
     
            await _repository.AddProjectFiles(pf);// projectFile);
            
        }
        private void SetReceivers()
        {           
            Receive<DatabaseWatcherActor.AddAutomation>(x =>
            {
                IActorRef folderWatcherActor = Context.ActorOf(Props.Create(() => new FolderWatcherActor(x.AutomationModel, Self)), FolderWatcherActor+"_"+x.AutomationModel.ProjectSchemaId);

                Context.WatchWith(folderWatcherActor, new FolderWatchDead(x.AutomationModel));

                _folderMonitorActors.Add(x.AutomationModel.ProjectAutomationId, folderWatcherActor);

                folderWatcherActor.Tell(new FolderWatcherActor.WatchFolder(x.AutomationModel));
            });

            Receive<DatabaseWatcherActor.RemoveAutomation>(x =>
            {
                if (_folderMonitorActors.ContainsKey(x.AutomationModel.ProjectAutomationId))
                {
                    _folderMonitorActors[x.AutomationModel.ProjectAutomationId].Tell(new FolderWatcherActor.StopWatchFolder(x.AutomationModel));
                }
            });

            Receive<FolderWatcherActor.NewProjectFile>(x =>
            {
                ProjectFile pf = new ProjectFile();
                pf.FileName = x.FileName;
                pf.FilePath = x.FullFilePath;
                pf.ProjectId = x.AutomationModel.ProjectId;
                pf.SchemaId = x.AutomationModel.ProjectSchemaId;
                pf.ReaderId = x.AutomationModel.ReaderId;
                pf.SourceTypeId = 1;
                pf.UserId = x.AutomationModel.CreatedBy;
                WriteProjectFile(pf);

                Thread.Sleep(500);

                //is this a good time to trigger ingestion
                CallLoadModel(pf);


            });

            Receive<FolderWatchDead>(x =>
            {
                if (_folderMonitorActors.ContainsKey(x.AutomationModel.ProjectAutomationId))
                {
                    _folderMonitorActors.Remove(x.AutomationModel.ProjectAutomationId);
                }

                _logger.Info($"Automation Coordinator is no longer watch Automation ID {x.AutomationModel.ProjectAutomationId} @ Folder : {x.AutomationModel.FolderPath}");
            });
        }

        private void WriteProjectFile(ProjectFile pf)
        {
            //write using repo
            AutoAdd?.Invoke(this, pf);
        }


        private async void CallLoadModel(ProjectFile pf)
        {
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), _connectionString).Options;
            using (var dbContext = new DAPDbContext(options, _connectionString))
            {
                var repo = new Repository(dbContext, null);
                int jobId = repo.GetNewJobId();
                var projectSchemas = repo.GetSchemasAsync(pf.UserId, pf.ProjectId, true);
                List<TypeConfig> TypeConfigList = new List<TypeConfig>();
                int readerTypeId = 0;
               
                if (projectSchemas != null)
                {
                    //var retMap = iMapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchemas.Result);
                    foreach (var projectSchema in projectSchemas.Result)
                    {
                        var objTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(projectSchema.TypeConfig);
                        objTypeConfig.SchemaName = projectSchema.SchemaName;
                        objTypeConfig.SchemaId = projectSchema.SchemaId;
                        if (objTypeConfig != null)
                        {
                            TypeConfigList.Add(objTypeConfig);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(pf.FilePath))
                {

                    if (pf.FilePath.Contains(".csv"))
                        readerTypeId = 1;
                    else if (pf.FilePath.Contains(".json"))
                        readerTypeId = 2;
                    else if (pf.FilePath.Contains(".log"))
                        readerTypeId = 3;
                }
                await repo.AddJob(pf.UserId, pf.ProjectId, jobId, 0, new List<int> { pf.ProjectFileId});
                var reader = await repo.GetReaderAsync((int)pf.ReaderId);
                var Configuration = reader.ReaderConfiguration;
                var writer = await repo.GetWritersInProject(pf.UserId, pf.ProjectId);

                TypeConfig retSchema = null;
                if (readerTypeId == 1 || readerTypeId == 2)
                {
                    var fullPath = pf.FilePath;
                    await Execute(pf.UserId, fullPath, TypeConfigList, pf.ProjectFileId, jobId, Configuration, pf.ProjectId, _connectionString, postgresConnection);
                }
            }
        }

        public async Task<int> Execute(int userId, string FileName, List<TypeConfig> typeConfigList, int FileId = 1, int jobId = 1, string configuration = "", int projectId = -1, string connectionString = "", string postgresConnString = "", DataAccess.Models.Writer[] writers = null)
        {
            Func<int> Function = new Func<int>(() =>
            {
               

                if (FileName.EndsWith(".csv"))
                {

                    if (typeConfigList == null || (typeConfigList != null && typeConfigList.Count == 0)) return -1;

                    var conf = new ReaderConfiguration
                        (typeConfigList[0], FileName, DataAnalyticsPlatform.Shared.Types.SourceType.Csv, FileId);
                    if (projectId != -1)
                    {
                        conf.ProjectId = projectId;
                    }
                    if (!string.IsNullOrEmpty(configuration))
                    {
                        CsvReaderConfiguration Csvconf = JsonConvert.DeserializeObject<CsvReaderConfiguration>(configuration);
                        conf.ConfigurationDetails = Csvconf;
                    }
                    List<WriterConfiguration> confWriter = new List<WriterConfiguration>();

                    foreach (var writer in writers)
                    {
                        WriterConfiguration writerTest = null;
                        if (writer.WriterTypeId == 4)
                        {
                            writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.ElasticSearch, connectionString: "http://192.168.1.11:9200", ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                        }
                        else
                        {
                            writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.RDBMS, connectionString: postgresConnString, ModelMap: null);

                            //Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                        }
                        if (projectId != -1)
                        {

                            writerTest.ProjectId = projectId;
                        }
                        confWriter.Add(writerTest);

                    }

                    //@"Server =raja.db.elephantsql.com; User Id = aniwbjgk; Password = esypNF7dCv9kKReCSNvM48LsPoJX_IvG; Database = aniwbjgk; Port=5432;CommandTimeout=0
                    //var confWriter = new Writers.WriterConfiguration(Shared.Types.DestinationType.RDBMS, connectionString: postgresConnString, ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                   // if (projectId != -1)
                   // {
                    //    confWriter.ProjectId = projectId;
                    //}

                    //  var confWriter = new Writers.WriterConfiguration(Shared.Types.DestinationType.RDBMS, connectionString: @"Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0", ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0

                    var ingestionJob = new IngestionJob(jobId, conf, confWriter.ToArray());
                    ingestionJob.ControlTableConnectionString = connectionString;
                    ingestionJob.UserId = userId;
                    MasterActor.Tell(ingestionJob);
                }
                else if (FileName.EndsWith(".json") || FileName.Contains("twitter"))
                {
             
                    if (typeConfigList == null || (typeConfigList != null && typeConfigList.Count == 0)) return -1;
                    var conf = new ReaderConfiguration(typeConfigList[0], FileName, DataAnalyticsPlatform.Shared.Types.SourceType.Json, FileId);
                    if (FileName.Contains("twitter") && !string.IsNullOrEmpty(configuration))
                    {
                        TwitterConfiguration Csvconf = JsonConvert.DeserializeObject<TwitterConfiguration>(configuration);
                        conf.ConfigurationDetails = Csvconf;
                    }
                    if (projectId != -1)
                    {
                        conf.ProjectId = projectId;
                    }
                    //var confWriter = new Writers.WriterConfiguration(Shared.Types.DestinationType.csv, connectionString: @"e:\temp\", ModelMap: null);
                    // var confWriter = new Writers.WriterConfiguration(Shared.Types.DestinationType.RDBMS, connectionString: postgresConnString, ModelMap: null);//Search Path=public;
                    /// var confWriter = new Writers.WriterConfiguration(Shared.Types.DestinationType.Mongo, connectionString: @"mongodb://localhost:27017/?connectTimeoutMS=30000&maxIdleTimeMS=600000", ModelMap: null);//Search Path=public;
                    // if (projectId != -1)
                    // {
                    //     confWriter.ProjectId = projectId;
                    //  }
                    List<WriterConfiguration> confWriter = new List<WriterConfiguration>();

                    foreach (var writer in writers)
                    {
                        WriterConfiguration writerTest = null;
                        if (writer.WriterTypeId == 4)
                        {
                            writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.ElasticSearch, connectionString: "http://192.168.1.11:9200", ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                        }
                        else
                        {
                            writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.RDBMS, connectionString: postgresConnString, ModelMap: null);

                            //Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                        }
                        if (projectId != -1)
                        {

                            writerTest.ProjectId = projectId;
                        }
                        confWriter.Add(writerTest);

                    }
                    var ingestionJob = new IngestionJob(jobId, conf, confWriter.ToArray());
                    ingestionJob.ControlTableConnectionString = connectionString;
                    ingestionJob.UserId = userId;
                    MasterActor.Tell(ingestionJob);
                }
                return 1;
            });
            // Logger.LogInformation($"Requesting model of user '{userId}'");
            return await Task.Factory.StartNew<int>(Function);

        }
        protected override void PreStart()
        {
            if (Context.Child(DatabaseMonitorActorName).Equals(ActorRefs.Nobody))
            {
                _databaseMonitorActor = Context.ActorOf(Props.Create(() => new DatabaseWatcherActor(_connectionString, Self)), DatabaseMonitorActorName);

                Context.Watch(_databaseMonitorActor);

                _databaseMonitorActor.Tell(new DatabaseWatcherActor.GetAutomationModels());
            }

            base.PreStart();
        }
    }
}
