using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.Models;
using DataAccess.DTO;
using LoadServiceApi.Shared;
//using DataAnalyticsPlatform.Shared.Models;
//using LoadServiceApi.TestData;
using Microsoft.AspNetCore.Mvc;
///using LoadServiceApi.Shared.Models;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.DataAccess;
using Newtonsoft.Json;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using DataAnalyticsPlatform.Actors.Preview;
using DataAnalyticsPlatform.Actors;
using DataAnalyticsPlatform.Shared;
using TypeConfig = DataAnalyticsPlatform.Shared.TypeConfig;
using DataAnalyticsPlatform.Shared.PostModels;
using DataAnalyticsPlatform.Common;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LoadServiceApi
{
    [Route("api/preview")]
    [ApiController]
    [Authorize]
    public class PreviewController : Controller
    {
        // public TestData.TestData testData = new TestData.TestData();
        private readonly IRepository _repository;

        private readonly IMapper _mapper;
        private PreviewRegistry previewRegistry;
        private GetModels GetModels { get; set; }
        private GenerateModel GenerateModel { get; set; }
        private UpdateModel UpdateModel { get; set; }

        private List<string> FileIdList;
        private List<int> FileIdsUsed;
        private IMemoryCache _cache;
        public PreviewController(IRepository repo, IMapper mapper,
               GetModels GetModels, GenerateModel GenerateModel, UpdateModel UpdateModel, IMemoryCache Cache)
        {
            _repository = repo;
            _mapper = mapper;
            this.GetModels = GetModels;
            this.GenerateModel = GenerateModel;
            this.UpdateModel = UpdateModel;
            previewRegistry = new PreviewRegistry();
            FileIdList = new List<string>();
            FileIdsUsed = new List<int>();
            _cache = Cache;
        }
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        [HttpGet("{Projectid}/GetSchemas")]
        public async Task<List<DataAnalyticsPlatform.Shared.TypeConfig>> GetSchemas(int Projectid)//we will make it a different incoming class with more props
        {
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var projectSchema = await _repository.GetSchemasAsync(userId, Projectid, true);
            if (projectSchema != null)
            {

                var retMap = _mapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchema);
                List<TypeConfig> retConfig = new List<TypeConfig>();
                foreach (var projSchema in retMap)
                {
                    var objTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(projSchema.TypeConfig);
                    objTypeConfig.SchemaId = projSchema.SchemaId;
                    objTypeConfig.SchemaName = projSchema.SchemaName;
                    foreach (var model in projSchema.SchemaModels)
                    {
                        foreach (var typeConfigModel in objTypeConfig.ModelInfoList)
                        {
                            if (model.ModelName == typeConfigModel.ModelName)
                            {
                                typeConfigModel.ModelId = model.ModelId;
                            }
                        }
                    }
                    retConfig.Add(objTypeConfig);
                }
                return retConfig;
            }
            return null;
        }

        [HttpPost("{ProjectId}/generatemodel")]
        public async Task<List<DataAnalyticsPlatform.Shared.TypeConfig>> GenerateModelV2(int ProjectId, [FromBody]int[] FileId)//we will make it a different incoming class with more props
        {
            List<DataAnalyticsPlatform.Shared.TypeConfig> result = new List<DataAnalyticsPlatform.Shared.TypeConfig>();
            List<DataAnalyticsPlatform.Shared.TypeConfig> retConfig = new List<DataAnalyticsPlatform.Shared.TypeConfig>();
            //  getfiles( for this eids)
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Console.WriteLine("User Id : " + userId);
            var files = await _repository.GetProjectFiles(ProjectId, FileId);
            if ( files == null)
                Console.WriteLine("null file");
            if (files == null) return null;
            // result = await Task.FromResult(testData.GenerateModel(userId, ProjectId, FileId));
            foreach (var file in files)
            {
                Console.WriteLine("file " + file.FileName);
                Console.WriteLine("clearing cachce");
                FileIdList.Clear();
                FileIdsUsed.Clear();

                _cache.Remove("Files");
                _cache.Remove("FileIds");
                Console.WriteLine("clearing cachce");

                int readerTypeId = 0;
                string fullPath = "";
                if (!string.IsNullOrEmpty(file.FileName))
                {
                    if (file.FileName.Contains(".csv"))
                        readerTypeId = 1;
                    else if (file.FileName.Contains(".json"))
                        readerTypeId = 2;
                    else if (file.FileName.Contains(".log"))
                        readerTypeId = 3;

                    fullPath = Path.Combine(file.FilePath, file.FileName);
                    Console.WriteLine("Ful Path " + fullPath);
                    if (System.IO.File.Exists(fullPath) == true)
                    {
                        int g = 0;
                    }
                }
                else
                {
                    fullPath = "twitter";
                    // if (file.ReaderId == null)
                    //   file.ReaderId = 2;//lets call it json for now

                }
                Console.WriteLine("file iteration 1");
                if (_repository == null)
                    Console.WriteLine("file iteration 1 null");
                string Configuration = "";

                if (fullPath == "twitter")
                {
                    Configuration = file.SourceConfiguration;
                }
                else
                {
                    var reader = await _repository.GetReaderAsync((int)file.ReaderId);
                    Configuration = reader.ReaderConfiguration == null ? reader.ConfigurationName : reader.ReaderConfiguration;
                }
                Console.WriteLine("file iteration 2");
                FileIdList.Add(fullPath);
                FileIdsUsed.Add(file.ProjectFileId);
                Console.WriteLine("file iteration 3");
                var singleConfig = await this.GenerateModel.Execute(userId, fullPath, Configuration);
                Console.WriteLine("file iteration 4");
                result.Add(singleConfig.TypeConfiguration);
            }
            _cache.Set("Files", FileIdList);
            _cache.Set("FileIds", FileIdsUsed);
            var projectSchema = await _repository.GetSchemasAsync(userId, ProjectId, true);
            PreviewRegistry previewReg = new PreviewRegistry();

            if (projectSchema != null)
            {
                var retMap = _mapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchema);
                foreach (var projSchema in retMap)
                {
                    var objTypeConfig = JsonConvert.DeserializeObject<DataAnalyticsPlatform.Shared.TypeConfig>(projSchema.TypeConfig);
                    objTypeConfig.SchemaId = projSchema.SchemaId;
                    objTypeConfig.SchemaName = projSchema.SchemaName;
                    foreach (var model in projSchema.SchemaModels)
                    {
                        foreach (var typeConfigModel in objTypeConfig.ModelInfoList)
                        {
                            if (model.ModelName == typeConfigModel.ModelName)
                            {
                                typeConfigModel.ModelId = model.ModelId;
                            }
                        }
                    }
                    retConfig.Add(objTypeConfig);
                }
                int indexr = 0;
                foreach (var oneTypeconfig in result)
                {
                    bool newSchema = true;
                    foreach (var projSchema in retMap)
                    {
                        var objTypeConfig = JsonConvert.DeserializeObject<DataAnalyticsPlatform.Shared.TypeConfig>(projSchema.TypeConfig);
                        var EnumCompare = previewReg.CompareTypeConfigDetailed(oneTypeconfig, objTypeConfig);
                        if (EnumCompare == PreviewRegistry.EnumSchemaDiffType.SameBase ||
                            EnumCompare == PreviewRegistry.EnumSchemaDiffType.SameModelsBase)
                        {
                            //same schema
                            newSchema = false;


                        }

                    }
                    if (newSchema)
                    {
                        if (retConfig.Count > 0)
                        {
                            var EnumCompare = previewReg.CompareTypeConfigDetailed(oneTypeconfig, retConfig[0]);
                            if (EnumCompare != PreviewRegistry.EnumSchemaDiffType.SameBase &&
                            EnumCompare != PreviewRegistry.EnumSchemaDiffType.SameModelsBase)
                            {
                                retConfig.Add(oneTypeconfig);
                            }
                        }
                        else
                        {
                            retConfig.Add(oneTypeconfig);
                        }


                    }

                    //if new schema lets persist
                }

                //lets add new if schema doenst have anything - its first time

                return retConfig;
            }
            else
            {
                //new project
                retConfig.AddRange(result);
            }

            return retConfig;
        }

        [HttpPost("{ProjectId}/{SchemaId}/{ModelId}/getpreview")]
        public async Task<ActionResult<List<Dictionary<string, object>>>> Post(int ProjectId, int SchemaId, int ModelId)//we will make it a different incoming class with more props
        {

            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            PreviewUpdateResponse result = null;
            PreviewUpdateResponse ret = null;
            Console.WriteLine("update model ");
            var existingProject = await _repository.GetProjectAsync(userId, ProjectId);
            if (existingProject != null)
            {
                Console.WriteLine("update model existing");
                var schemaModels = await _repository.GetSchemaAsync(SchemaId, true);
                TypeConfig SchemaTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(schemaModels.TypeConfig);
                Console.WriteLine("update model schema found");
                Dictionary<string, List<BaseModel>> PreviewData = null;
                _cache.TryGetValue("Files", out FileIdList);
                _cache.TryGetValue("FileIds", out FileIdsUsed);
                var schemaModel = schemaModels.SchemaModels.Where(x => x.ModelId == ModelId).FirstOrDefault();
                Console.WriteLine("update model schemamodel");
                if (FileIdList == null)
                {
                    Console.WriteLine("NullFieleid");
                    return null;
                }
                int fileIndex = 0;
                ProjectFile[] projFiles;
               // if (FileIdsUsed.Count < fileIndex)
                {
                    projFiles = await _repository.GetProjectFiles(ProjectId, FileIdsUsed.ToArray());
                }
                string fullPath = "";
                foreach (var file in projFiles)
                {
                    fullPath = "";
                    if (!string.IsNullOrEmpty(file.FileName))
                    {
                        int readerTypeId = 0;
                        if (file.FileName.Contains(".csv"))
                            readerTypeId = 1;
                        else if (file.FileName.Contains(".json"))
                            readerTypeId = 2;
                        else if (file.FileName.Contains(".log"))
                            readerTypeId = 3;
                        Console.WriteLine("file iteration 1");
                        if (_repository == null)
                            Console.WriteLine("file iteration 1 null");

                        fullPath = Path.Combine(file.FilePath, file.FileName);
                    }
                    else
                    {
                        fullPath = "twitter";
                       // if (file.ReaderId == null)
                          //  file.ReaderId = 2;//lets call it json for now
                    }
                   // var reader = await _repository.GetReaderAsync((int)file.ReaderId);

                    string Configuration = "";

                    if (fullPath == "twitter")
                    {
                        Configuration = file.SourceConfiguration;
                    }
                    else
                    {
                        var reader = await _repository.GetReaderAsync((int)file.ReaderId);
                        Configuration = reader.ReaderConfiguration;
                    }
                    //ProjectFile[] projFiles;
                    //if (FileIdsUsed.Count < fileIndex)
                    //{
                    //    projFiles = await _repository.GetProjectFiles(ProjectId, FileIdsUsed[fileIndex]);
                    //}
                    //else
                    //{
                    //    break;
                    //}
                    fileIndex++;
                    var Preview = await this.UpdateModel.Execute(userId, SchemaTypeConfig, fullPath , Configuration);
                    Console.WriteLine("Preview Done");
                    if (Preview == null )
                    {
                        Console.WriteLine("Preview is Null");
                        return null;
                    }
                    var modelSelected = Preview.Item2.Where(x => x.Value.Any(y => y.ModelName == schemaModel.ModelName)).ToDictionary(x=>x.Key, x=>x.Value);//Select(x => schemaModel.Where(y => y.ModelName == x.Key) );
                    if (modelSelected == null)
                    {
                        Console.WriteLine("modelSelected is Null");
                        return null;
                    }
                    if (PreviewData == null)
                    {
                        PreviewData = (Dictionary<string, List<BaseModel>>)modelSelected;
                        //  PreviewData.ToDictionary()
                    }
                    else
                    {
                        foreach (KeyValuePair<string, List<BaseModel>> kvp in modelSelected)
                        {
                            if (PreviewData.ContainsKey(kvp.Key))
                            {
                                PreviewData[kvp.Key].AddRange(kvp.Value);
                            }
                            else
                            {
                                PreviewData.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }
                }
                List<Dictionary<string, object>> DataCollection = new List<Dictionary<string, object>>();
                //result = await Task.FromResult(testData.UpdateModel(userId, ProjectId));
                Console.WriteLine("data collection");
                if (PreviewData != null)
                {
                    var t = PreviewData.ToDictionary(k => k.Key, k => (object)k.Value);
                    Console.WriteLine("data collection done");
                    foreach (KeyValuePair<string, List<BaseModel>> kvp in PreviewData)
                    {
                        foreach (var model in kvp.Value)
                        {
                            var tye = model.GetType();
                            Console.WriteLine("data collection model");
                            var row = (Dictionary<string, object>)model.ToDictionary();
                            Console.WriteLine("data collection dict");
                            DataCollection.Add(row);
                        }
                    }
                    if (DataCollection.Count > 0)
                        return DataCollection;
                    // return (List<Dictionary<string, object>>)new List<Dictionary<string, object>>() { PreviewData.Cast<Dictionary<string,object>>().ToDictionary()};
                }
            }
            //if (ret != null && ret.ModelsPreview.ContainsKey(ModelId))
            //{
            //    return ret.ModelsPreview[ModelId];
            //}
            return null;
        }

        [HttpPost("{ProjectId}/{SchemaId}/updatemodel")]
        public async Task<List<SchemaModelMapping>> Post(int ProjectId, int SchemaId, [FromBody]PreviewUpdate previewUpdate)//we will make it a different incoming class with more props
        {
            //return null;
            PreviewUpdateResponse result = null;
            List<SchemaModelMapping> RetSchemaModelMappingList = null;
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            System.Console.WriteLine("updatemodel");
            try
            {
                var existingProject = await _repository.GetProjectAsync(userId, ProjectId);
                System.Console.WriteLine("check exiting project");
                if ( previewUpdate.updatedConfig.ModelInfoList == null || previewUpdate.updatedConfig.ModelInfoList.Count == 0 )//lets make a copy of basefields
                {
                    previewUpdate.updatedConfig.ModelInfoList = new List<ModelInfo>();
                    previewUpdate.updatedConfig.ModelInfoList.Add(new ModelInfo());
                    previewUpdate.updatedConfig.ModelInfoList[0].ModelFields = new List<DataAnalyticsPlatform.Shared.FieldInfo>(previewUpdate.updatedConfig.BaseClassFields);
                    previewUpdate.updatedConfig.ModelInfoList[0].ModelName = "OriginalModel";
               
                }
                SchemaDTO schemaDTO = TypeConfigToSchemaDTO.Tranform(previewUpdate.updatedConfig, ProjectId, SchemaId, userId);
                schemaDTO.SchemaName = previewUpdate.SchemaName;
                var NewprojectSchema = _mapper.Map<ProjectSchema>(schemaDTO);
                NewprojectSchema.UserId = userId;

                foreach (var mod in NewprojectSchema.SchemaModels)
                {
                    mod.UserId = userId;
                }
                ProjectSchema MatchedSchema = null;
                if (existingProject != null)
                {
                    _cache.TryGetValue("FileIds", out FileIdsUsed);
                    System.Console.WriteLine("exiting project");
                    var projectSchema = await _repository.GetSchemasAsync(userId, ProjectId, true);
                    PreviewRegistry.EnumSchemaDiffType diffType = PreviewRegistry.EnumSchemaDiffType.None;
                    foreach (var project in projectSchema)
                    {

                        var thisTypeConifg = JsonConvert.DeserializeObject<TypeConfig>(project.TypeConfig);
                        diffType = previewRegistry.CompareTypeConfigDetailed(thisTypeConifg, previewUpdate.updatedConfig);
                        // var test = thisTypeConifg.BaseClassFields.Where(y => previewUpdate.updatedConfig.BaseClassFields.Any(z => y.Name == z.Name)); 
                        if (diffType == PreviewRegistry.EnumSchemaDiffType.SameBase ||
                            diffType == PreviewRegistry.EnumSchemaDiffType.SameModelsBase)
                        {
                            MatchedSchema = project;
                            break;
                        }
                    }

                    // var MatchedSchema =  projectSchema.Where(x=> JToken.DeepEquals(x.TypeConfig, NewprojectSchema.TypeConfig)).FirstOrDefault();
                    if (SchemaId != 0)
                    {
                        MatchedSchema = projectSchema.Where(x => x.SchemaId == SchemaId).FirstOrDefault();
                    }
                    if (SchemaId == 0 && MatchedSchema != null)
                    {
                        SchemaId = MatchedSchema.SchemaId;
                    }
                    if (MatchedSchema != null)
                    {
                        NewprojectSchema.SchemaId = SchemaId;
                        NewprojectSchema.IsActive = true;
                        var RetprojectSchema = _repository.SetSchemaAsync(SchemaId, NewprojectSchema);
                        if (FileIdsUsed != null)
                        {
                            for (int i = 0; i < FileIdsUsed.Count; i++)
                            {
                                await _repository.SetSchemaId(FileIdsUsed[i], SchemaId);
                            }
                        }
                    }
                    else
                    {
                        _repository.Add(NewprojectSchema);

                        await _repository.SaveChangesAsync();
                        projectSchema = await _repository.GetSchemasAsync(userId, ProjectId, true);
                        if (projectSchema != null)
                        {
    
                            var thisSchema = projectSchema.Where(x => x.SchemaName == previewUpdate.SchemaName).FirstOrDefault();
                            //add automtaion folder
                            var folderName = Path.Combine("AutoIngestion", "UserData_" + userId + "\\" + thisSchema.Project.ProjectName+"_"+ thisSchema.SchemaName);

                            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                            if (!Directory.Exists(pathToSave))
                            {
                                Directory.CreateDirectory(pathToSave);
                                if (FileIdsUsed != null && FileIdsUsed.Count > 0)
                                {
                                    var readerId = _repository.GetReaderFromProjectFile(FileIdsUsed[0]);//get teh file to get reader

                                    if (readerId != null && readerId.Result > 0 )
                                    { //add automation folder
                                        var projAutomation = new ProjectAutomation
                                        {
                                            CreatedBy = userId,
                                            ProjectId = ProjectId,
                                            FolderPath = pathToSave,
                                            ProjectSchemaId = thisSchema.SchemaId,
                                            ReaderId = readerId.Result
                                        };

                                        _repository.Add(projAutomation);
                                    }
                                }
                            }
                            if (thisSchema != null)
                            {
                                for (int i = 0; i < FileIdsUsed.Count; i++)
                                {
                                    await _repository.SetSchemaId(FileIdsUsed[i], thisSchema.SchemaId);
                                }
                            }
                        }


                    }
                    List<SchemaModelMapping> schemaModelMappingList = new List<SchemaModelMapping>();
                    foreach (ProjectSchema pSchema in projectSchema)
                    {
                        List<ModelMapping> ModelMappingList = new List<ModelMapping>();
                        foreach (DataAccess.Models.SchemaModel schemaModel in pSchema.SchemaModels)
                        {
                            if (schemaModel.ModelName != null)
                            {
                                ModelMapping modelMapping = new ModelMapping { ModelId = schemaModel.ModelId, ModelName = schemaModel.ModelName };
                                ModelMappingList.Add(modelMapping);
                            }
                        }
                        schemaModelMappingList.Add(new SchemaModelMapping { SchemaMName = pSchema.SchemaName, SchemaId = pSchema.SchemaId, ModelMap = ModelMappingList });
                    }
                    RetSchemaModelMappingList = schemaModelMappingList;

                    // result = await Task.FromResult(testData.UpdateModel(userId, ProjectId));

                    return RetSchemaModelMappingList;
                }
                else
                {
                    System.Console.WriteLine("not exiting project");
                    return null;
                }
                // {
                //     var NewprojectSchema = _mapper.Map<ProjectSchema>(schemaDTO);
                //     NewprojectSchema.UserId = userId;
                //     NewprojectSchema.SchemaId = previewUpdate.SchemaId;
                // var projectSchema = await _repository.GetSchemasAsync(userId, ProjectId, true);
                //var MatchedSchema = projectSchema.Where(x => x.SchemaId == previewUpdate.SchemaId).FirstOrDefault();
                // if (MatchedSchema != null)
                //  {

                //           _repository.SetSchemaAsync(previewUpdate.SchemaId, NewprojectSchema);
                //     }
                // else
                //    {

                //   _repository.Add(NewprojectSchema);

                //                        await _repository.SaveChangesAsync();
                //   }
                //result = await Task.FromResult(testData.UpdateModel(userId, ProjectId));

                //  }
                //  else
                // {
                //   return this.StatusCode(StatusCodes.Status204NoContent, " No Project found");
                //}
                //var projectSchema = await _repository.GetSchemaAsync(schemaId, true);

                //if (projectSchema == null)          
                //{

                //}
                //if (existingProject != null)
                //{
                //    var pSchema = _mapper.Map<ProjectSchema>(schemaDTO);

                //    if (pSchema != null )
                //    { 
                //    pSchema.ProjectId = ProjectId;

                //    pSchema.UserId = userId;

                //    pSchema.SchemaName = 
                //    _repository.Add(projectSchema);

                //    await _repository.SaveChangesAsync();
                //}



            }
            catch (Exception ex)
            {
                System.Console.WriteLine("exception " + ex.Message);
                return null;
            }
            return null;
        }

        [HttpDelete("{ProjectId}/{SchemaId}")]
        public async Task<IActionResult> DeleteSchema([FromRoute] int projectId, [FromRoute]int SchemaId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            bool result = await _repository.DeleteSchema(userId, projectId, SchemaId);

            if (result == false)
            {
                return NotFound();
            }
            else
            {
                return Ok(SchemaId);
            }
        }
        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
