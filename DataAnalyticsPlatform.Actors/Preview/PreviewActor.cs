using Akka.Actor;
using DataAnalyticsPlatform.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataAnalyticsPlatform.Shared;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared.Types;
using DataAnalyticsPlatform.Shared.Interfaces;
using System.Reflection;
using DataAnalyticsPlatform.SharedUtils;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Common.Twitter;
using AutoMapper;
using DataAnalyticsPlatform.Shared.DataAccess;

//using DataAnalyticsPlatform.Common;
//using DataAnalyticsPlatform.Actors.messages;

namespace DataAnalyticsPlatform.Actors
{
    public class PreviewActor : ReceiveActor
    {
        public PreviewRegistry previewRegistry { get; set; }
        public PreviewActor(PreviewRegistry previewRegistry)
        {
            this.previewRegistry = previewRegistry;
            SetReceiveHandles();
        }
        void SetReceiveHandles()
        {
            Receive<messages.PreviewActor.GetModel>(m => 
            {
                if (this.previewRegistry.Models.TryGetValue(m.userId, out SchemaModels models))
                {
                    Sender.Tell(models);
                }
            });

            ReceiveAsync<messages.PreviewActor.GenerateModel>(m =>
                GenerateModelAction(m).PipeTo(Sender));

            ReceiveAsync<messages.PreviewActor.UpdateModel>(m =>
               UpdateModelAction(m).PipeTo(Sender));
        }
        public async Task<SchemaModel> GenerateModelAction(messages.PreviewActor.GenerateModel gm)
        {
            if ( gm.file_name.EndsWith(".csv"))
            {
                Func<SchemaModel> Function = new Func<SchemaModel>(() =>
                {
                    string className = "";
                    Console.WriteLine("Csv Class generator Start");
                    var gen = new CsvModelGenerator().ClassGenerator( gm.file_name, ref className, ((CsvReaderConfiguration)gm.readerConfiguration).delimiter, "","", (CsvReaderConfiguration)gm.readerConfiguration);
                    Console.WriteLine("Csv Class generator GEtaLLfields");
                    var fieldInfoList = new CsvModelGenerator().GetAllFields(gm.file_name, ref className, ((CsvReaderConfiguration)gm.readerConfiguration).delimiter, "","", (CsvReaderConfiguration)gm.readerConfiguration);
                    
                    TransformationCodeGenerator codegen = new TransformationCodeGenerator();
                   
                    List<string> clases = new List<string> { className };
                    Console.WriteLine("Csv Code Gen");
                    Type type = codegen.GenerateModelCode(gen, className); // CodeCompilerHelper.generate(gen, clases);
                    SchemaModel model = new SchemaModel() { ListOfFieldInfo = fieldInfoList, TypeConfiguration = new TypeConfig { BaseClassFields = fieldInfoList}, ClassDefinition = gen };
                    
                     model.AllTypes = new List<Type> { type };
                    
                    previewRegistry.AddToRegistry(model, gm.userId);
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
                    Console.WriteLine(" preview actor cretaing");

                    var fieldInfoList = new JsonModelGenerator().GetAllFields(gm.file_name, gm.readerConfiguration.readerName, ref className, ref ClassString, "test");
                    Console.WriteLine(" preview actor cretaing 2");
                    //var gen = new JsonModelGenerator().ClassGenerator(fieldInfoList, ref className);
                    Console.WriteLine(" preview actor cretaing 3");
                    TransformationCodeGenerator codegen = new TransformationCodeGenerator();
                    Console.WriteLine(" preview actor cretaing 4");
                    List<string> clases = new List<string> { className };
                    Type[] types = codegen.GenerateModelJSONCode(ClassString, className); // CodeCompilerHelper.generate(gen, clases);
                    Console.WriteLine(" preview actor cretaing 5");
                    var OriginalRecordType = types.Where(x => x.FullName.Contains("OriginalRecord"+gm.jobId)).FirstOrDefault();
                    Console.WriteLine(" preview actor cretaing 6");
                    SchemaModel model = new SchemaModel() { ListOfFieldInfo = fieldInfoList, TypeConfiguration = new TypeConfig { BaseClassFields = fieldInfoList }, ClassDefinition = ClassString };
                    Console.WriteLine(" preview actor cretaing 7");
                    model.AllTypes = new List<Type> { OriginalRecordType };

                    previewRegistry.AddToRegistry(model, gm.userId);
                    return model;
                });
                return await Task.Factory.StartNew<SchemaModel>(Function);
            }
            else if ( gm.file_name.ToLower().Contains("twitter") == true)
            {

                var conf = new ReaderConfiguration();
                conf.ConfigurationDetails = gm.readerConfiguration;
                TwitterReader reader = new TwitterReader(conf);
                var twitConf = (TwitterConfiguration)conf.ConfigurationDetails;
                reader.twitConf.MaxSearchEntriesToReturn = 1;
                reader.twitConf.MaxTotalResults = 2;
             //   OriginalRecord org = new OriginalRecord();
              //  var task = Task.Run(async () => await reader.GetTweets());
             //   if (task.Result.Any())
                {
                    try
                    {
                        IRecord record = null;
                        reader.GetRecords(out record);
                       // var orgObj = mapper.Map<LinqToTwitter.Status, OriginalRecord>(task.Result[0]);
                        TwitterObjectModelGenerator gen = new TwitterObjectModelGenerator();
                        var fieldInfoList = gen.GetAllFields(gm.file_name, record.Instance);//, task.Result[0]);
                        SchemaModel model = new SchemaModel() { ListOfFieldInfo = fieldInfoList, TypeConfiguration = new TypeConfig { BaseClassFields = fieldInfoList }, ClassDefinition = "" };
                        return model;
                    }
                    catch(Exception ex)
                    {
                        int gg = 0;
                    }
                   
                }

            }
            return null;
        }

        public async Task<Tuple<string, Dictionary<string,List<BaseModel>>>> UpdateModelAction(messages.PreviewActor.UpdateModel um)
        {
            //gene nw type
            Func<Tuple<string,Dictionary<string, List<BaseModel>>>> Function = new Func<Tuple<string,Dictionary<string, List<BaseModel>>>>(() =>
            {
                TransformationCodeGenerator codegen = new TransformationCodeGenerator();
                SchemaModels models = previewRegistry.GetFromRegistry(um.userId);
                List<Type> types = null;
                bool skipUpdate = false;
                TypeConfig incomingTypeConfig = new TypeConfig(um.typeConfig);
                if (models == null || (models != null && previewRegistry.CompareTypeConfig(um.typeConfig, models.SModels[0].TypeConfiguration) == false))//zero for testing for now
                {
                    if (um.FileName.EndsWith(".csv"))
                    {
                        types = codegen.Code(um.typeConfig);
                    }else if ( um.FileName.EndsWith(".json"))
                    {
                        types = codegen.CodeJSON(um.typeConfig);
                    }
                    else if (um.FileName.Contains("twitter"))
                    {
                        types = codegen.CodeJSON(um.typeConfig);
                    }
                }
                else
                {
                    if (models.SModels != null)
                    {
                        skipUpdate = true;
                        types = models.SModels[0].AllTypes;
                    }
                }

                if (um.FileName.EndsWith(".csv"))
                {
                    if (types != null && types.Count > 1)
                    {
                        var MapType = types.Where(x => x.FullName.Contains("Mappers")).FirstOrDefault();
                        object mapperObject = Activator.CreateInstance(MapType);
                        object originalObject = Activator.CreateInstance(types[0]);
                        if (!skipUpdate)
                        {
                            if (models == null)
                            {
                                SchemaModel smodel = new SchemaModel() { TypeConfiguration = incomingTypeConfig, AllTypes = types };
                                previewRegistry.AddToRegistry(smodel, um.userId, true);
                            }
                            else
                            {
                                SchemaModel smodel = new SchemaModel() { TypeConfiguration = incomingTypeConfig, AllTypes = types };
                                previewRegistry.AddToRegistry(smodel, um.userId, true);
                            }
                        }

                        var conf = new ReaderConfiguration
                        (originalObject.GetType(), mapperObject.GetType(), um.FileName, SourceType.Csv);
                        conf.ConfigurationDetails = um.readerConfiguration;
                        CsvReader preCsvreader = new CsvReader(conf);
                        Dictionary<string, List<BaseModel>> previewData = new Dictionary<string, List<BaseModel>>();
                        for (int i = 0; i < 10; i++)
                        {
                            IRecord rec;
                            MethodInfo methodMap = types[0].GetMethod("MapIt");
                            MethodInfo methodGetModels = types[0].GetMethod("GetModels");
                            if (preCsvreader.GetRecords(out rec, originalObject.GetType()) == true)
                            {
                                Type t = rec.Instance.GetType();

                                methodMap.Invoke(rec.Instance, null);// new object[] {rec.Instance }
                                List<BaseModel> ret = (List<BaseModel>)methodGetModels.Invoke(rec.Instance, new object[] { (int)0 });
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
                            models = previewRegistry.GetFromRegistry(um.userId);//only userid input include schemaid soon
                        }
                        var resp = Tuple.Create(models.SModels[0].SchemaId, previewData);
                        return resp;

                    }
                }
                else if (um.FileName.EndsWith(".json") || um.FileName.Contains("twitter"))
                {
                    if (types != null && types.Count > 1)
                    {
                        Type originalType = types.Where(x => x.FullName.Contains("OriginalRecord")).FirstOrDefault();
                        object originalObject = Activator.CreateInstance(originalType);
                        if (!skipUpdate)
                        {
                            if (models == null)
                            {
                                SchemaModel smodel = new SchemaModel() { TypeConfiguration = incomingTypeConfig, AllTypes = types };
                                previewRegistry.AddToRegistry(smodel, um.userId, true);
                            }
                            else
                            {
                                SchemaModel smodel = new SchemaModel() { TypeConfiguration = incomingTypeConfig, AllTypes = types };
                                previewRegistry.AddToRegistry(smodel, um.userId, true);
                            }
                        }

                        var conf = new ReaderConfiguration
                        (originalObject.GetType(), null, um.FileName, SourceType.Json);
                        Readers.BaseReader jsonReader = null;
                        if (um.FileName.EndsWith(".json"))
                        {
                            jsonReader = new Readers.JsonReader(conf);
                        }
                        else
                        {
                            conf.ConfigurationDetails = um.readerConfiguration;
                            ((TwitterConfiguration)conf.ConfigurationDetails).MaxTotalResults = 16;
                            jsonReader = new Readers.TwitterReader(conf, types);
                        }
                        Dictionary<string, List<BaseModel>> previewData = new Dictionary<string, List<BaseModel>>();
                        for (int i = 0; i < 10; i++)
                        {
                            IRecord rec;
                            MethodInfo methodMap = originalType.GetMethod("MapIt");
                            MethodInfo methodGetModels = originalType.GetMethod("GetModels");
                            if (jsonReader.GetRecords(out rec) == true)
                            {
                                    Type t = rec.Instance.GetType();

                                    methodMap.Invoke(rec.Instance, null);// new object[] {rec.Instance }
                                    List<BaseModel> ret = (List<BaseModel>)methodGetModels.Invoke(rec.Instance, new object[] { (int)0});
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
                                    else
                                    {
                                       
                                    }
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (models == null)
                        {
                            models = previewRegistry.GetFromRegistry(um.userId);//only userid input include schemaid soon
                        }
                        var resp = Tuple.Create(models.SModels[0].SchemaId, previewData);
                        return resp;

                    }
                }
                    return null;
            });
            return await Task.Factory.StartNew<Tuple<string, Dictionary<string,List<BaseModel>>>>(Function);
           // return null;
        }



    }
}
