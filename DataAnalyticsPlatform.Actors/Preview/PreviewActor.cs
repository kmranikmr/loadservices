using Akka.Actor;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DataAnalyticsPlatform.Actors
{
    /// <summary>
    /// Actor responsible for handling preview model operations and registry management.
    /// </summary>
    public class PreviewActor : ReceiveActor
    {
        // NLog logger for logging events and debugging
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public PreviewRegistry previewRegistry { get; set; }

        public PreviewActor(PreviewRegistry previewRegistry)
        {
            this.previewRegistry = previewRegistry;
            logger.Info("PreviewActor initialized with PreviewRegistry.");
            SetReceiveHandles();
        }

        /// <summary>
        /// Sets up message handlers for preview actor messages.
        /// </summary>
        void SetReceiveHandles()
        {
            logger.Info("Setting up Receive handlers for PreviewActor.");

            Receive<messages.PreviewActor.GetModel>(m =>
            {
                logger.Info($"Received GetModel for userId: {m.userId}");
                if (this.previewRegistry.Models.TryGetValue(m.userId, out SchemaModels models))
                {
                    logger.Info($"Model found for userId: {m.userId}, sending to sender.");
                    Sender.Tell(models);
                }
                else
                {
                    logger.Warn($"No model found for userId: {m.userId}");
                }
            });

            ReceiveAsync<messages.PreviewActor.GenerateModel>(m =>
            {
                logger.Info($"Received GenerateModel for file: {m.file_name}, userId: {m.userId}");
                return GenerateModelAction(m).PipeTo(Sender);
            });

            ReceiveAsync<messages.PreviewActor.UpdateModel>(m =>
            {
                logger.Info($"Received UpdateModel for file: {m.FileName}, userId: {m.userId}");
                return UpdateModelAction(m).PipeTo(Sender);
            });
        }

        /// <summary>
        /// Generates a schema model based on the provided GenerateModel message.
        /// Logs key steps and errors.
        /// </summary>
        public async Task<SchemaModel> GenerateModelAction(messages.PreviewActor.GenerateModel gm)
        {
            logger.Info($"Starting GenerateModelAction for file: {gm.file_name}, userId: {gm.userId}");

            if (gm.file_name.EndsWith(".csv"))
            {
                Func<SchemaModel> Function = new Func<SchemaModel>(() =>
                {
                    string className = "";
                    logger.Info("Csv Class generator Start");
                    var gen = new CsvModelGenerator().ClassGenerator(gm.file_name, ref className, ((CsvReaderConfiguration)gm.readerConfiguration).delimiter, "", "", (CsvReaderConfiguration)gm.readerConfiguration);
                    logger.Info("Csv Class generator GetAllFields");
                    var fieldInfoList = new CsvModelGenerator().GetAllFields(gm.file_name, ref className, ((CsvReaderConfiguration)gm.readerConfiguration).delimiter, "", "", (CsvReaderConfiguration)gm.readerConfiguration);

                    logger.Info("Initializing TransformationCodeGenerator for CSV.");
                    TransformationCodeGenerator codegen = new TransformationCodeGenerator();

                    List<string> clases = new List<string> { className };
                    logger.Info("Csv Code Gen");
                    Type type = codegen.GenerateModelCode(gen, className);
                    logger.Info($"Generated type: {type?.FullName}");

                    SchemaModel model = new SchemaModel()
                    {
                        ListOfFieldInfo = fieldInfoList,
                        TypeConfiguration = new TypeConfig { BaseClassFields = fieldInfoList },
                        ClassDefinition = gen
                    };

                    model.AllTypes = new List<Type> { type };

                    logger.Info($"Adding model to registry for userId: {gm.userId}");
                    previewRegistry.AddToRegistry(model, gm.userId);
                    logger.Info("CSV model generation completed.");
                    return model;
                });
                return await Task.Factory.StartNew<SchemaModel>(Function);
            }
            else if (gm.file_name.EndsWith(".json"))
            {
                Func<SchemaModel> Function = new Func<SchemaModel>(() =>
                {
                    string className = "";
                    string ClassString = "";
                    logger.Info("PreviewActor: Starting JSON model creation");

                    var fieldInfoList = new JsonModelGenerator().GetAllFields(
                        gm.file_name,
                        gm.readerConfiguration.readerName,
                        ref className,
                        ref ClassString,
                        "test"
                    );

                    logger.Info("PreviewActor: Field info extracted, proceeding to code generation");

                    TransformationCodeGenerator codegen = new TransformationCodeGenerator();

                    List<string> clases = new List<string> { className };

                    Type[] types = codegen.GenerateModelJSONCode(ClassString, className);

                    logger.Info("PreviewActor: Model code generated");

                    var OriginalRecordType = types.Where(x => x.FullName.Contains("OriginalRecord" + gm.jobId)).FirstOrDefault();

                    logger.Info($"PreviewActor: OriginalRecord type resolved: {OriginalRecordType?.FullName}");

                    SchemaModel model = new SchemaModel()
                    {
                        ListOfFieldInfo = fieldInfoList,
                        TypeConfiguration = new TypeConfig { BaseClassFields = fieldInfoList },
                        ClassDefinition = ClassString
                    };

                    logger.Info("PreviewActor: SchemaModel created");

                    model.AllTypes = new List<Type> { OriginalRecordType };

                    logger.Info($"Adding JSON model to registry for userId: {gm.userId}");
                    previewRegistry.AddToRegistry(model, gm.userId);

                    logger.Info("JSON model generation completed.");
                    return model;
                });
                return await Task.Factory.StartNew<SchemaModel>(Function);
            }
            else if (gm.file_name.ToLower().Contains("twitter"))
            {
                logger.Info("Starting Twitter model generation.");
                var conf = new ReaderConfiguration();
                conf.ConfigurationDetails = gm.readerConfiguration;
                TwitterReader reader = new TwitterReader(conf);
                var twitConf = (TwitterConfiguration)conf.ConfigurationDetails;
                reader.twitConf.MaxSearchEntriesToReturn = 1;
                reader.twitConf.MaxTotalResults = 2;

                try
                {
                    IRecord record = null;
                    logger.Info("Fetching Twitter records.");
                    reader.GetRecords(out record);
                    TwitterObjectModelGenerator gen = new TwitterObjectModelGenerator();
                    var fieldInfoList = gen.GetAllFields(gm.file_name, record.Instance);
                    logger.Info("Twitter field info extracted.");
                    SchemaModel model = new SchemaModel()
                    {
                        ListOfFieldInfo = fieldInfoList,
                        TypeConfiguration = new TypeConfig { BaseClassFields = fieldInfoList },
                        ClassDefinition = ""
                    };
                    logger.Info("Twitter model generation completed.");
                    return model;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error during Twitter model generation.");
                }
            }
            logger.Warn("GenerateModelAction: Unsupported file type or error occurred.");
            return null;
        }

        /// <summary>
        /// Updates a schema model based on the provided UpdateModel message.
        /// Logs key steps and errors.
        /// </summary>
        public async Task<Tuple<string, Dictionary<string, List<BaseModel>>>> UpdateModelAction(messages.PreviewActor.UpdateModel um)
        {
            logger.Info($"Starting UpdateModelAction for file: {um.FileName}, userId: {um.userId}");

            Func<Tuple<string, Dictionary<string, List<BaseModel>>>> Function = new Func<Tuple<string, Dictionary<string, List<BaseModel>>>>(() =>
            {
                logger.Info("Initializing TransformationCodeGenerator for update.");
                TransformationCodeGenerator codegen = new TransformationCodeGenerator();
                SchemaModels models = previewRegistry.GetFromRegistry(um.userId);
                List<Type> types = null;
                bool skipUpdate = false;
                TypeConfig incomingTypeConfig = new TypeConfig(um.typeConfig);

                logger.Info("Checking if model update is required.");
                if (models == null || (models != null && previewRegistry.CompareTypeConfig(um.typeConfig, models.SModels[0].TypeConfiguration) == false))
                {
                    logger.Info("Model update required, generating new types.");
                    if (um.FileName.EndsWith(".csv"))
                    {
                        types = codegen.Code(um.typeConfig, 0, Path.GetFileName(um.FileName));
                        logger.Info("CSV types generated.");
                    }
                    else if (um.FileName.EndsWith(".json"))
                    {
                        types = codegen.CodeJSON(um.typeConfig);
                        logger.Info("JSON types generated.");
                    }
                    else if (um.FileName.Contains("twitter"))
                    {
                        types = codegen.CodeJSON(um.typeConfig);
                        logger.Info("Twitter types generated.");
                    }
                }
                else
                {
                    if (models.SModels != null)
                    {
                        skipUpdate = true;
                        types = models.SModels[0].AllTypes;
                        logger.Info("Skipping update, using existing types.");
                    }
                }

                if (um.FileName.EndsWith(".csv"))
                {
                    logger.Info("Processing CSV preview data.");
                    if (types != null && types.Count > 1)
                    {
                        var MapType = types.Where(x => x.FullName.Contains("Mappers")).FirstOrDefault();
                        object mapperObject = Activator.CreateInstance(MapType);
                        object originalObject = Activator.CreateInstance(types[0]);
                        if (!skipUpdate)
                        {
                            SchemaModel smodel = new SchemaModel() { TypeConfiguration = incomingTypeConfig, AllTypes = types };
                            previewRegistry.AddToRegistry(smodel, um.userId, true);
                            logger.Info("CSV model added to registry after update.");
                        }

                        var conf = new ReaderConfiguration(originalObject.GetType(), mapperObject.GetType(), um.FileName, SourceType.Csv);
                        conf.ConfigurationDetails = um.readerConfiguration;
                        CsvReader preCsvreader = new CsvReader(conf);
                        Dictionary<string, List<BaseModel>> previewData = new Dictionary<string, List<BaseModel>>();
                        for (int i = 0; i < 10; i++)
                        {
                            IRecord rec;
                            MethodInfo methodMap = types[0].GetMethod("MapIt");
                            MethodInfo methodGetModels = types[0].GetMethod("GetModels");
                            if (preCsvreader.GetRecords(out rec, originalObject.GetType()))
                            {
                                logger.Info($"Mapping CSV record {i + 1}");
                                methodMap.Invoke(rec.Instance, null);
                                List<BaseModel> ret = (List<BaseModel>)methodGetModels.Invoke(rec.Instance, new object[] { 0 });
                                if (ret != null)
                                {
                                    foreach (BaseModel bm in ret)
                                    {
                                        if (previewData.ContainsKey(bm.ModelName))
                                        {
                                            previewData[bm.ModelName].Add(bm);
                                        }
                                        else
                                        {
                                            previewData.Add(bm.ModelName, new List<BaseModel> { bm });
                                        }
                                    }
                                }
                            }
                        }
                        if (models == null)
                        {
                            models = previewRegistry.GetFromRegistry(um.userId);
                        }
                        var resp = Tuple.Create(models.SModels[0].SchemaId, previewData);
                        logger.Info("CSV preview data generation completed.");
                        return resp;
                    }
                }
                else if (um.FileName.EndsWith(".json") || um.FileName.Contains("twitter"))
                {
                    logger.Info("Processing JSON/Twitter preview data.");
                    if (types != null && types.Count > 1)
                    {
                        Type originalType = types.Where(x => x.FullName.Contains("OriginalRecord")).FirstOrDefault();
                        object originalObject = Activator.CreateInstance(originalType);
                        if (!skipUpdate)
                        {
                            SchemaModel smodel = new SchemaModel() { TypeConfiguration = incomingTypeConfig, AllTypes = types };
                            previewRegistry.AddToRegistry(smodel, um.userId, true);
                            logger.Info("JSON/Twitter model added to registry after update.");
                        }

                        var conf = new ReaderConfiguration(originalObject.GetType(), null, um.FileName, SourceType.Json);
                        Readers.BaseReader jsonReader = null;
                        if (um.FileName.EndsWith(".json"))
                        {
                            jsonReader = new Readers.JsonReader(conf);
                            logger.Info("Using JsonReader for preview.");
                        }
                        else
                        {
                            conf.ConfigurationDetails = um.readerConfiguration;
                            ((TwitterConfiguration)conf.ConfigurationDetails).MaxTotalResults = 16;
                            jsonReader = new Readers.TwitterReader(conf, types);
                            logger.Info("Using TwitterReader for preview.");
                        }
                        Dictionary<string, List<BaseModel>> previewData = new Dictionary<string, List<BaseModel>>();
                        for (int i = 0; i < 10; i++)
                        {
                            IRecord rec;
                            MethodInfo methodMap = originalType.GetMethod("MapIt");
                            MethodInfo methodGetModels = originalType.GetMethod("GetModels");
                            if (jsonReader.GetRecords(out rec))
                            {
                                logger.Info($"Mapping JSON/Twitter record {i + 1}");
                                methodMap.Invoke(rec.Instance, null);
                                List<BaseModel> ret = (List<BaseModel>)methodGetModels.Invoke(rec.Instance, new object[] { 0 });
                                if (ret != null)
                                {
                                    foreach (BaseModel bm in ret)
                                    {
                                        if (previewData.ContainsKey(bm.ModelName))
                                        {
                                            previewData[bm.ModelName].Add(bm);
                                        }
                                        else
                                        {
                                            previewData.Add(bm.ModelName, new List<BaseModel> { bm });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                logger.Info("No more records to process for preview.");
                                break;
                            }
                        }
                        if (models == null)
                        {
                            models = previewRegistry.GetFromRegistry(um.userId);
                        }
                        var resp = Tuple.Create(models.SModels[0].SchemaId, previewData);
                        logger.Info("JSON/Twitter preview data generation completed.");
                        return resp;
                    }
                }
                logger.Warn("UpdateModelAction: No preview data generated or unsupported file type.");
                return null;
            });
            return await Task.Factory.StartNew<Tuple<string, Dictionary<string, List<BaseModel>>>>(Function);
        }
    }
}
