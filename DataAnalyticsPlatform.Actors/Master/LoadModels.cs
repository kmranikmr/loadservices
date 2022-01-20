using Akka.Actor;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.Types;
using DataAnalyticsPlatform.Writers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataAnalyticsPlatform.Actors.Master.MasterActor;
using Npgsql;


namespace DataAnalyticsPlatform.Actors.Master
{
    public class LoadModels
    {
        private IActorRef LoadActor { get; set; }
        private IngestionJob _ingestionJob;
        private PreviewRegistry previewRegsitry;

        public LoadModels(LoadActorProvider provider)
        {
            // this.Logger = logger;
            this.previewRegsitry = provider.previewRegistry;
            this.LoadActor = provider.Get();
        }
        public bool CreateSchema(string connectionString, string schemaName)
        {
             using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                   try
                    {
                    connection.Open();

                    using (NpgsqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = string.Format("create schema if not exists " + schemaName);

                        command.ExecuteNonQuery();

                    }
                   }catch(Exception ex)
                   {
                     return false;
                   }
                  return true;
                }
            return false;

        }


        public async Task<int> Execute(int userId, string FileName, List<TypeConfig> typeConfigList, int FileId = 1, int jobId = 1, string configuration = "", int projectId = -1, string connectionString = "", string postgresConnString = "", DataAccess.Models.Writer[] writers = null, string elasticSearchString = "")
        {
            Func<int> Function = new Func<int>(() =>
            {
                SchemaModels smodels = previewRegsitry.GetFromRegistry(userId);
                
                if (FileName.EndsWith(".csv"))
                {

                    if (typeConfigList == null || (typeConfigList != null && typeConfigList.Count == 0)) return -1;

                    var conf = new ReaderConfiguration
                        (typeConfigList[0], FileName, SourceType.Csv, FileId);
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
                    //@"Server =raja.db.elephantsql.com; User Id = aniwbjgk; Password = esypNF7dCv9kKReCSNvM48LsPoJX_IvG; Database = aniwbjgk; Port=5432;CommandTimeout=0
                    foreach (var writer in writers)
                    {
                        WriterConfiguration writerTest = null;
                        if (writer.WriterTypeId == 4)
                        {
                            writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.ElasticSearch, connectionString: elasticSearchString, ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                        }
                        else
                        {
                            writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.RDBMS, connectionString: postgresConnString, ModelMap: null);
                            var _schemaName = conf.TypeConfig.SchemaName.Replace(" ", string.Empty);
                            _schemaName = conf.ProjectId != -1 ? _schemaName + "_" + conf.ProjectId +"_"+ userId: _schemaName;
                            CreateSchema(postgresConnString, _schemaName);
                             
                            //Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
                        }
                        if (projectId != -1)
                        {

                            writerTest.ProjectId = projectId;
                        }
                        confWriter.Add(writerTest);
                       
                    }

                    //  var confWriter = new Writers.WriterConfiguration(Shared.Types.DestinationType.RDBMS, connectionString: @"Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0", ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0

                    _ingestionJob = new IngestionJob(jobId, conf, confWriter.ToArray());
                    _ingestionJob.ControlTableConnectionString = connectionString;
                    _ingestionJob.UserId = userId;
                    LoadActor.Tell(_ingestionJob);
                }
                else if (FileName.EndsWith(".json") || FileName.Contains("twitter"))
                {
                    //-----commenedt below
                  //  List<Type> types = smodels.SModels[0].AllTypes;
                  ////  Type originalType = types.Where(x => x.FullName.Contains("OriginalRecord" + _ingestionJob.JobId)).FirstOrDefault();
                 //   object originalObject = Activator.CreateInstance(originalType);

                    ///---------------------------------------------------------

                    // var conf = new ReaderConfiguration
                    //     (originalObject.GetType(), null, FileName, SourceType.Json);

                    // var conf = new ReaderConfiguration(smodels.SModels[0].TypeConfiguration, FileName, SourceType.Json, FileId);
                    if (typeConfigList == null || (typeConfigList != null && typeConfigList.Count == 0)) return -1;
                    var conf = new ReaderConfiguration(typeConfigList[0], FileName, SourceType.Json, FileId);
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
                  //  if (projectId != -1)
                   // {
                        //    confWriter.ProjectId = projectId;
                        // }
                        List<WriterConfiguration> confWriter = new List<WriterConfiguration>();
                        //@"Server =raja.db.elephantsql.com; User Id = aniwbjgk; Password = esypNF7dCv9kKReCSNvM48LsPoJX_IvG; Database = aniwbjgk; Port=5432;CommandTimeout=0
                        foreach (var writer in writers)
                        {
                            WriterConfiguration writerTest = null;
                            if (writer.WriterTypeId == 4)
                            {
                                writerTest = new Writers.WriterConfiguration(Shared.Types.DestinationType.ElasticSearch, connectionString: elasticSearchString, ModelMap: null);//Search Path=public;/Server =localhost; User Id = dev; Password = nwdidb19; Database = nwdi_ts; Port=5433;CommandTimeout=0
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
                        _ingestionJob = new IngestionJob(jobId, conf, confWriter.ToArray());
                    _ingestionJob.ControlTableConnectionString = connectionString;
                    _ingestionJob.UserId = userId;
                    LoadActor.Tell(_ingestionJob);
                }
                return 1;
            });
            // Logger.LogInformation($"Requesting model of user '{userId}'");
            return await Task.Factory.StartNew<int>(Function);

        }
    }
}

