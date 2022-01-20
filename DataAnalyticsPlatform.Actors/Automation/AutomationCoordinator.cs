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

        public PreviewRegistry previewRegistry;

        public IActorRef MasterActor { get; set; }
        public AutomationCoordinator(IActorRef masterRef, string dbConnectionString, string postgresString)
        {
            _connectionString = dbConnectionString;

            Console.WriteLine(dbConnectionString);
            MasterActor = masterRef;

            postgresConnection = postgresString;
            previewRegistry = new PreviewRegistry();
            
            AutoAdd += AddtoProjectFile;

           

            _folderMonitorActors = new Dictionary<int, IActorRef>();

             mapperConf = new MapperConfiguration(cfg => {
                cfg.CreateMap<ProjectSchema,SchemaDTO>();
                 cfg.CreateMap<ProjectFile,ProjectFileDTO>();

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

                Thread.Sleep(5000);

                //is this a good time to trigger ingestion
                CallLoadModel(pf);
                Thread.Sleep(2000);

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

         public async Task<TypeConfig> CheckSchemaAndUpdate(int ProjectId, int reader_type_id, string file_name , int file_id, List<TypeConfig> TypeConfigList, string configuration)
        {
            string className = string.Empty;
            Console.WriteLine("CheckSchemaAndUpdate 1");
            if (reader_type_id == 1)
            {
                CsvReaderConfiguration Csvconf = null;
                if (!string.IsNullOrEmpty(configuration))
                {
                    Csvconf = JsonConvert.DeserializeObject<CsvReaderConfiguration>(configuration);
                }
                else
                {
                    Console.WriteLine("CheckSchemaAndUpdate null config ");
                }
                if ( Csvconf != null )
                {
                  Console.WriteLine("CheckSchemaAndUpdate " + file_name + " " + ((CsvReaderConfiguration)Csvconf).delimiter);
               }
                var fieldInfoList = new CsvModelGenerator().GetAllFields(file_name, ref className, 
                                ((CsvReaderConfiguration)Csvconf).delimiter, "", "", (CsvReaderConfiguration)Csvconf);
                if (fieldInfoList == null || fieldInfoList.Count == 0) 
                 {
                       Console.WriteLine("CheckSchemaAndUpdate Null");
                       return null;
                  }
                 Console.WriteLine(" length " +  fieldInfoList.Count.ToString());
                var matchedTypeConfig = TypeConfigList.Find(x => previewRegistry.CompareBaseFields(x.BaseClassFields, fieldInfoList) == PreviewRegistry.EnumSchemaDiffType.SameBase);
                if ( matchedTypeConfig != null )
                {
                     Console.WriteLine("matchedTypeConfig is good");
                    await _repository.SetSchemaId(file_id, matchedTypeConfig.SchemaId);
                    return matchedTypeConfig;
                }
                else {Console.WriteLine("matchedTypeConfig no good");}
            }
            else if ( reader_type_id == 2 && !file_name.Contains("twitter"))
            {
               
                string ClassString = "";
                var fieldInfoList = new JsonModelGenerator().GetAllFields(file_name, configuration, ref className, ref ClassString, "test");
                if (fieldInfoList == null || fieldInfoList.Count == 0) return null;
                var matchedTypeConfig = TypeConfigList.Find(x => previewRegistry.CompareBaseFields(x.BaseClassFields, fieldInfoList) == PreviewRegistry.EnumSchemaDiffType.SameBase);
                if (matchedTypeConfig != null)
                {
                    await _repository.SetSchemaId(file_id, matchedTypeConfig.SchemaId);
                    return matchedTypeConfig;
                }
            }
            return null;

        }
        private async void CallLoadModel(ProjectFile pf)
        {
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<DAPDbContext>(), _connectionString).Options;
            Console.WriteLine("CallMOdel ConnectionString  " + _connectionString);
            using (var dbContext = new DAPDbContext(options, _connectionString))
            {
                var repo = new Repository(dbContext, null);
                int jobId = repo.GetNewJobId();
                var projectSchemas = await repo.GetSchemasAsync(pf.UserId, pf.ProjectId, true);
                List<TypeConfig> TypeConfigList = new List<TypeConfig>();
                int readerTypeId = 0;

                string fullPath = Path.Combine(pf.FilePath, pf.FileName);                
                
                if (projectSchemas != null)
                {
                    Console.WriteLine("ProjectSchemas Obtained ");
                     var Conf = new MapperConfiguration(cfg => {
                cfg.CreateMap<SchemaDTO, ProjectSchema>();
                

             });
            
             var thisMapper = Conf.CreateMapper();
                    //var retMap = thisMapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchemas);
                    //var retMap = iMapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchemas.Result);
                    foreach (var projectSchema in projectSchemas)
                    {
                        var objTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(projectSchema.TypeConfig);
                        var schemaModelArray = await _repository.GetModelsAsync(pf.UserId, projectSchema.SchemaId);
                    foreach ( var configObj in objTypeConfig.ModelInfoList )
                    {
                        for(int j = 0; j < schemaModelArray.Length; j++)
                        {
                            if ( schemaModelArray[j] != null )
                            {
                                if ( configObj.ModelName == schemaModelArray[j].ModelName)
                                {
                                    Console.WriteLine(" Matched model " + configObj.ModelName + " " + schemaModelArray[j].ModelId ); 
                                    configObj.ModelId = schemaModelArray[j].ModelId;
                                }
                            }
                        }
                    }
                        
		        objTypeConfig.SchemaName = projectSchema.SchemaName;
                        objTypeConfig.SchemaId = projectSchema.SchemaId;
                        if (objTypeConfig != null)
                        {
                            TypeConfigList.Add(objTypeConfig);
                            Console.WriteLine("TypeCOnfigList added");
                        }
                    }
                   
                }
                if (!string.IsNullOrEmpty(fullPath))
                {

                    if (fullPath.Contains(".csv"))
                        readerTypeId = 1;
                    else if (fullPath.Contains(".json"))
                        readerTypeId = 2;
                    else if (fullPath.Contains(".log"))
                        readerTypeId = 3;
                }
                Console.WriteLine("readerType "+readerTypeId + " file path "+pf.FilePath + "full Path "+ fullPath + " " +  pf.ProjectFileId);
                await repo.AddJob(pf.UserId, pf.ProjectId, jobId, 0, new List<int> { pf.ProjectFileId});
                var reader = await repo.GetReaderAsync((int)pf.ReaderId);
                var Configuration = reader.ReaderConfiguration;
                var writer = await repo.GetWritersInProject(pf.UserId, pf.ProjectId);

                TypeConfig retSchema = null;
                if ( Configuration == null )
                {
                    Console.WriteLine("Configuration is NUll");
                }
                Console.WriteLine("Check the schema");
              
                if (readerTypeId == 1 || readerTypeId == 2)
                {
                    
                    Console.WriteLine("About to execute");
                    retSchema = await CheckSchemaAndUpdate(pf.ProjectId, readerTypeId, fullPath, pf.ProjectFileId, TypeConfigList, Configuration);
                    if (retSchema == null)
                    {
                        Console.WriteLine("no schema annot execute");
                    }
                    else
                    {
                       
                    
                          await Execute(pf.UserId, fullPath, new List<TypeConfig> { retSchema }, pf.ProjectFileId, jobId, Configuration, pf.ProjectId, _connectionString, postgresConnection, writer); 
                    }
                }
            }
        }

        public async Task<int> Execute(int userId, string FileName, List<TypeConfig> typeConfigList, int FileId = 1, int jobId = 1, string configuration = "", int projectId = -1, string connectionString = "", string postgresConnString = "", DataAccess.Models.Writer[] writers = null)
        {
            Func<int> Function = new Func<int>(() =>
            {
               

                if (FileName.EndsWith(".csv"))
                {
                    Console.WriteLine("Execue in Automation" + FileName);
                    if (typeConfigList == null || (typeConfigList != null && typeConfigList.Count == 0)) return -1;

                    var conf = new ReaderConfiguration
                        (typeConfigList[0], FileName, DataAnalyticsPlatform.Shared.Types.SourceType.Csv, FileId);
                    if (projectId != -1)
                    {
                        if ( conf == null )
			{
                             Console.WriteLine("conf is null");
			}
                        conf.ProjectId = projectId;
                    }
                    if (!string.IsNullOrEmpty(configuration))
                    {
                        CsvReaderConfiguration Csvconf = JsonConvert.DeserializeObject<CsvReaderConfiguration>(configuration);
                        conf.ConfigurationDetails = Csvconf;
                    }
                    List<WriterConfiguration> confWriter = new List<WriterConfiguration>();
                    if ( writers == null )
                    {
                          Console.WriteLine("Writers are null");
                    }
                    foreach (var writer in writers)
                    {
                        WriterConfiguration writerTest = null;
                        if (writer.WriterTypeId == 4)
                        {
                            writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.ElasticSearch, connectionString: "http://192.168.1.11:9200", ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                        }
                        else
                        {
                            Console.WriteLine("writer not 4 ");
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
                    Console.WriteLine("Call Master with ingestion ");
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
