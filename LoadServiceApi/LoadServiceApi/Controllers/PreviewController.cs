using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using DataAnalyticsPlatform.Actors;
using DataAnalyticsPlatform.Actors.Preview;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared.PostModels;
using LoadServiceApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TypeConfig = DataAnalyticsPlatform.Shared.TypeConfig;

namespace LoadServiceApi
{
    [Route("api/preview")]
    [ApiController]
    [Authorize]
    public class PreviewController : Controller
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<PreviewController> _logger;
        private readonly PreviewRegistry _previewRegistry;
        private readonly GetModels _getModels;
        private readonly GenerateModel _generateModel;
        private readonly UpdateModel _updateModel;
        private readonly List<string> _fileIdList;
        private readonly List<int> _fileIdsUsed;
        private readonly IMemoryCache _cache;

        public PreviewController(
            IRepository repo, 
            IMapper mapper,
            GetModels getModels, 
            GenerateModel generateModel, 
            UpdateModel updateModel, 
            IMemoryCache cache,
            ILogger<PreviewController> logger)
        {
            _repository = repo;
            _mapper = mapper;
            _getModels = getModels;
            _generateModel = generateModel;
            _updateModel = updateModel;
            _previewRegistry = new PreviewRegistry();
            _fileIdList = new List<string>();
            _fileIdsUsed = new List<int>();
            _cache = cache;
            _logger = logger;
            
            _logger.LogInformation("PreviewController initialized");
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{Projectid}/GetSchemas")]
        public async Task<List<TypeConfig>> GetSchemas(int Projectid)
        {
            _logger.LogInformation($"Getting schemas for Project ID: {Projectid}");
            
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _logger.LogInformation($"User ID: {userId} requesting schemas for Project ID: {Projectid}");
            
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
                
                _logger.LogInformation($"Found {retConfig.Count} schemas for Project ID: {Projectid}");
                return retConfig;
            }
            
            _logger.LogWarning($"No schemas found for Project ID: {Projectid}");
            return null;
        }

        [HttpPost("{ProjectId}/generatemodel")]
        public async Task<List<TypeConfig>> GenerateModelV2(int ProjectId, [FromBody] int[] FileId)
        {
            _logger.LogInformation($"Generating model for Project ID: {ProjectId} with {FileId.Length} files");
            
            List<TypeConfig> result = new List<TypeConfig>();
            List<TypeConfig> retConfig = new List<TypeConfig>();
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _logger.LogInformation($"User ID: {userId} generating model for Project ID: {ProjectId}");

            var files = await _repository.GetProjectFiles(ProjectId, FileId);
            if (files == null) 
            {
                _logger.LogWarning($"No files found for Project ID: {ProjectId}");
                return null;
            }
            
            _logger.LogInformation($"Found {files.Length} files for Project ID: {ProjectId}");

            foreach (var file in files)
            {
                _fileIdList.Clear();
                _fileIdsUsed.Clear();
                _cache.Remove("Files");
                _cache.Remove("FileIds");

                int readerTypeId = 0;
                string fullPath = "";

                if (!string.IsNullOrEmpty(file.FileName))
                {
                    if (file.FileName.Contains(".csv")) readerTypeId = 1;
                    else if (file.FileName.Contains(".json")) readerTypeId = 2;
                    else if (file.FileName.Contains(".log")) readerTypeId = 3;

                    fullPath = Path.Combine(file.FilePath, file.FileName);
                    _logger.LogInformation($"Processing file: {fullPath} with reader type: {readerTypeId}");
                }
                else
                {
                    fullPath = "twitter";
                    _logger.LogInformation("Processing Twitter data");
                }

                string Configuration = fullPath == "twitter"
                    ? file.SourceConfiguration
                    : (await _repository.GetReaderAsync((int)file.ReaderId)).ReaderConfiguration;

                _fileIdList.Add(fullPath);
                _fileIdsUsed.Add(file.ProjectFileId);
                
                _logger.LogInformation($"Executing model generation for file: {fullPath}");
                try
                {
                    var singleConfig = await this._generateModel.Execute(userId, fullPath, Configuration);
                    if (singleConfig != null && singleConfig.TypeConfiguration != null)
                    {
                        result.Add(singleConfig.TypeConfiguration);
                        _logger.LogInformation($"Successfully generated model for file: {fullPath}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to generate model for file: {fullPath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error generating model for file: {fullPath}");
                }
            }

            _cache.Set("Files", _fileIdList);
            _cache.Set("FileIds", _fileIdsUsed);
            _logger.LogInformation($"Set cache with {_fileIdList.Count} files and {_fileIdsUsed.Count} file IDs");

            var projectSchema = await _repository.GetSchemasAsync(userId, ProjectId, true);
            PreviewRegistry previewReg = new PreviewRegistry();
            
            _logger.LogInformation($"Processing existing schemas for Project ID: {ProjectId}");
            
            if (projectSchema != null)
            {
                var retMap = _mapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchema);

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

                    foreach (var oneTypeconfig in result)
                    {
                        bool newSchema = true;

                        foreach (var projSchema in retMap)
                        {
                            var objTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(projSchema.TypeConfig);
                            var EnumCompare = previewReg.CompareTypeConfigDetailed(oneTypeconfig, objTypeConfig);

                            if (EnumCompare == PreviewRegistry.EnumSchemaDiffType.SameBase ||
                                EnumCompare == PreviewRegistry.EnumSchemaDiffType.SameModelsBase)
                            {
                                newSchema = false;
                                _logger.LogInformation($"Found matching schema for generated model: {projSchema.SchemaName}");
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
                                    _logger.LogInformation("Added new schema to return configuration");
                                }
                            }
                            else
                            {
                                retConfig.Add(oneTypeconfig);
                                _logger.LogInformation("Added first schema to return configuration");
                            }
                        }
                    }
                    
                    _logger.LogInformation($"Returning {retConfig.Count} schemas for Project ID: {ProjectId}");
                    return retConfig;
                }
                else
                {
                    _logger.LogInformation("No existing schemas found, returning generated schemas");
                    retConfig.AddRange(result);
                }

                _logger.LogInformation($"Returning {retConfig.Count} schemas for Project ID: {ProjectId}");
                return retConfig;
            }        [HttpPost("{ProjectId}/{SchemaId}/{ModelId}/getpreview")]
        public async Task<ActionResult<List<Dictionary<string, object>>>> Post(int ProjectId, int SchemaId, int ModelId, [FromBody] int[] FileId)
        {
            _logger.LogInformation($"Getting preview for Project ID: {ProjectId}, Schema ID: {SchemaId}, Model ID: {ModelId}");
            
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _logger.LogInformation($"User ID: {userId} requesting preview");
            
            var existingProject = await _repository.GetProjectAsync(userId, ProjectId);
            if (existingProject != null)
            {
                var schemaModels = await _repository.GetSchemaAsync(SchemaId, true);
                if (schemaModels == null)
                {
                    _logger.LogWarning($"Schema ID {SchemaId} not found");
                    return null;
                }
                
                TypeConfig SchemaTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(schemaModels.TypeConfig);
                Dictionary<string, List<BaseModel>> PreviewData = null;

                ProjectFile[] projFiles = await _repository.GetProjectFiles(ProjectId, FileId);
                if (projFiles == null)
                {
                    _logger.LogWarning($"No files found for Project ID: {ProjectId}");
                    return null;
                }
                
                _logger.LogInformation($"Processing {projFiles.Length} files for preview");

                foreach (var file in projFiles)
                {
                    string fullPath = !string.IsNullOrEmpty(file.FileName)
                        ? Path.Combine(file.FilePath, file.FileName)
                        : "twitter";
                        
                    _logger.LogInformation($"Processing file: {fullPath}");

                    string Configuration = fullPath == "twitter"
                        ? file.SourceConfiguration
                        : (await _repository.GetReaderAsync((int)file.ReaderId)).ReaderConfiguration;

                    try
                    {
                        var Preview = await this._updateModel.Execute(userId, SchemaTypeConfig, fullPath, Configuration);
                        if (Preview == null)
                        {
                            _logger.LogWarning($"Failed to get preview for file: {fullPath}");
                            continue;
                        }
                        
                        _logger.LogInformation($"Successfully generated preview for file: {fullPath}");

                        var schemaModel = schemaModels.SchemaModels.FirstOrDefault(x => x.ModelId == ModelId);
                        if (schemaModel == null)
                        {
                            _logger.LogWarning($"Model ID {ModelId} not found in schema");
                            continue;
                        }
                        
                        var modelSelected = Preview.Item2.Where(x => x.Value.Any(y => y.ModelName == schemaModel.ModelName))
                            .ToDictionary(x => x.Key, x => x.Value);

                        if (PreviewData == null)
                            PreviewData = modelSelected;
                        else
                        {
                            foreach (var kvp in modelSelected)
                            {
                                if (PreviewData.ContainsKey(kvp.Key))
                                    PreviewData[kvp.Key].AddRange(kvp.Value);
                                else
                                    PreviewData.Add(kvp.Key, kvp.Value);
                            }
                        }
                        
                        _logger.LogInformation($"Added {modelSelected.Sum(x => x.Value.Count)} records to preview");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error generating preview for file: {fullPath}");
                    }
                }

                List<Dictionary<string, object>> DataCollection = new List<Dictionary<string, object>>();
                if (PreviewData != null)
                {
                    foreach (var kvp in PreviewData)
                    {
                        foreach (var model in kvp.Value)
                        {
                            DataCollection.Add((Dictionary<string, object>)model.ToDictionary());
                        }
                    }
                    
                    if (DataCollection.Count > 0)
                    {
                        _logger.LogInformation($"Returning {DataCollection.Count} records for preview");
                        return DataCollection;
                    }
                }
                
                _logger.LogWarning("No preview data found");
                return null;
            }
            
            _logger.LogWarning($"Project ID {ProjectId} not found for user {userId}");
            return null;
        }

        [HttpPost("{ProjectId}/{SchemaId}/updatemodel")]
        public async Task<List<SchemaModelMapping>> Post(int ProjectId, int SchemaId, [FromBody] PreviewUpdate previewUpdate)
        {
            _logger.LogInformation($"Updating model for Project ID: {ProjectId}, Schema ID: {SchemaId}");
            
            List<SchemaModelMapping> RetSchemaModelMappingList = null;
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _logger.LogInformation($"User ID: {userId} updating model");

            try
            {
                var existingProject = await _repository.GetProjectAsync(userId, ProjectId);
                if (existingProject == null)
                {
                    _logger.LogWarning($"Project ID {ProjectId} not found for user {userId}");
                    return null;
                }

                if (previewUpdate.updatedConfig.ModelInfoList == null || previewUpdate.updatedConfig.ModelInfoList.Count == 0)
                {
                    _logger.LogInformation("No model info list found, creating default model");
                    previewUpdate.updatedConfig.ModelInfoList = new List<ModelInfo> { 
                        new ModelInfo { 
                            ModelFields = new List<FieldInfo>(previewUpdate.updatedConfig.BaseClassFields), 
                            ModelName = "OriginalModel" 
                        } 
                    };
                }

                SchemaDTO schemaDTO = TypeConfigToSchemaDTO.Tranform(previewUpdate.updatedConfig, ProjectId, SchemaId, userId);
                schemaDTO.SchemaName = previewUpdate.SchemaName;
                _logger.LogInformation($"Transformed schema with name: {schemaDTO.SchemaName}");
                
                var NewprojectSchema = _mapper.Map<ProjectSchema>(schemaDTO);
                NewprojectSchema.UserId = userId;
                foreach (var mod in NewprojectSchema.SchemaModels) mod.UserId = userId;

                ProjectSchema MatchedSchema = null;
                int[] FileId = previewUpdate.FileId;
                _logger.LogInformation($"Processing {FileId.Length} files for schema update");

                if (existingProject != null)
                {
                    foreach (var fid in FileId) _fileIdsUsed.Add(fid);
                    var projectSchema = await _repository.GetSchemasAsync(userId, ProjectId, true);
                    PreviewRegistry.EnumSchemaDiffType diffType = PreviewRegistry.EnumSchemaDiffType.None;

                    foreach (var project in projectSchema)
                    {
                        var thisTypeConifg = JsonConvert.DeserializeObject<TypeConfig>(project.TypeConfig);
                        diffType = _previewRegistry.CompareTypeConfigDetailed(thisTypeConifg, previewUpdate.updatedConfig);

                        if (diffType == PreviewRegistry.EnumSchemaDiffType.SameBase ||
                            diffType == PreviewRegistry.EnumSchemaDiffType.SameModelsBase)
                        {
                            MatchedSchema = project;
                            _logger.LogInformation($"Found matching schema: {project.SchemaName} with ID: {project.SchemaId}");
                            break;
                        }
                    }

                    if (SchemaId != 0)
                    {
                        MatchedSchema = projectSchema.FirstOrDefault(x => x.SchemaId == SchemaId);
                        _logger.LogInformation($"Using specified Schema ID: {SchemaId}");
                    }
                    
                    if (SchemaId == 0 && MatchedSchema != null)
                    {
                        SchemaId = MatchedSchema.SchemaId;
                        _logger.LogInformation($"Using matched Schema ID: {SchemaId}");
                    }

                    if (MatchedSchema != null)
                    {
                        NewprojectSchema.SchemaId = SchemaId;
                        NewprojectSchema.IsActive = true;
                        await _repository.SetSchemaAsync(SchemaId, NewprojectSchema);
                        _logger.LogInformation($"Updated existing schema with ID: {SchemaId}");

                        if (_fileIdsUsed != null)
                        {
                            foreach (var fid in _fileIdsUsed)
                            {
                                await _repository.SetSchemaId(fid, SchemaId);
                                _logger.LogInformation($"Associated file ID: {fid} with Schema ID: {SchemaId}");
                            }
                        }
                    }
                    else
                    {
                        _repository.Add(NewprojectSchema);
                        await _repository.SaveChangesAsync();
                        _logger.LogInformation("Created new schema");
                        
                        projectSchema = await _repository.GetSchemasAsync(userId, ProjectId, true);
                        var thisSchema = projectSchema.FirstOrDefault(x => x.SchemaName == previewUpdate.SchemaName);
                        
                        if (thisSchema != null)
                        {
                            _logger.LogInformation($"New schema created with ID: {thisSchema.SchemaId}, Name: {thisSchema.SchemaName}");
                            
                            var folderName = Path.Combine("AutoIngestion", "UserData_" + userId, thisSchema.Project.ProjectName);
                            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                            if (!Directory.Exists(pathToSave))
                            {
                                Directory.CreateDirectory(pathToSave);
                                _logger.LogInformation($"Created directory: {pathToSave}");
                            }

                            if (_fileIdsUsed != null)
                            {
                                foreach (var fid in _fileIdsUsed)
                                {
                                    await _repository.SetSchemaId(fid, thisSchema.SchemaId);
                                    _logger.LogInformation($"Associated file ID: {fid} with new Schema ID: {thisSchema.SchemaId}");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find newly created schema with name: {previewUpdate.SchemaName}");
                        }
                    }

                    RetSchemaModelMappingList = projectSchema.Select(pSchema => new SchemaModelMapping
                    {
                        SchemaMName = pSchema.SchemaName,
                        SchemaId = pSchema.SchemaId,
                        ModelMap = pSchema.SchemaModels.Where(x => x.ModelName != null)
                            .Select(m => new ModelMapping { ModelId = m.ModelId, ModelName = m.ModelName }).ToList()
                    }).ToList();

                    _logger.LogInformation($"Returning {RetSchemaModelMappingList.Count} schema model mappings");
                    return RetSchemaModelMappingList;
                }
                else
                {
                    _logger.LogWarning($"Project ID {ProjectId} not found for user {userId}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating model for Project ID: {ProjectId}, Schema ID: {SchemaId}");
                return null;
            }
        }

        [HttpDelete("{ProjectId}/{SchemaId}")]
        public async Task<IActionResult> DeleteSchema([FromRoute] int projectId, [FromRoute] int SchemaId)
        {
            _logger.LogInformation($"Deleting schema ID: {SchemaId} from project ID: {projectId}");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for delete schema request");
                return BadRequest(ModelState);
            }
            
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _logger.LogInformation($"User ID: {userId} deleting schema");

            try
            {
                bool result = await _repository.DeleteSchema(userId, projectId, SchemaId);
                
                if (result)
                {
                    _logger.LogInformation($"Successfully deleted schema ID: {SchemaId}");
                    return Ok(SchemaId);
                }
                else
                {
                    _logger.LogWarning($"Schema ID: {SchemaId} not found or could not be deleted");
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting schema ID: {SchemaId}");
                return StatusCode(500, "An error occurred while deleting the schema");
            }
        }

        [HttpPost]
        public void Post([FromBody] string value) 
        {
            _logger.LogInformation("Default Post method called");
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) 
        {
            _logger.LogInformation($"Default Put method called with ID: {id}");
        }

        [HttpDelete("{id}")]
        public void Delete(int id) 
        {
            _logger.LogInformation($"Default Delete method called with ID: {id}");
        }
    }
}
