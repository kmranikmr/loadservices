using AutoMapper;
using Coravel.Queuing.Interfaces;
using DataAccess.DTO;
using DataAccess.Models;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataModels;
using DataAnalyticsPlatform.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoadServiceApi.Controllers
{
    [Route("api/LoadData")]
    [Authorize]
    [ApiController]
    public class LoadDataController : Controller
    {
        private readonly IHubContext<ProgressHub> _progressHubContext;
        private readonly IRepository _repository;
        private readonly IQueue _queue;
        private readonly IMapper _mapper;
        private readonly ILogger<LoadDataController> _logger;
        private readonly LoadModels _loadModels;
        private readonly PreviewRegistry _previewRegistry;

        private readonly string _connectionString;
        private readonly string _postgresWriter;
        private readonly string _elasticSearch;
        private readonly string _mongoDBString;

        private const int MaxJobCount = 5;

        public LoadDataController(
            IRepository repository,
            IMapper mapper,
            LoadModels loadModels,
            IOptions<ConnectionStringsConfig> optionsAccessor,
            ILogger<LoadDataController> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _loadModels = loadModels;
            _previewRegistry = new PreviewRegistry();

            _connectionString = optionsAccessor.Value.DefaultConnection;
            _postgresWriter = optionsAccessor.Value.PostgresConnection;
            _elasticSearch = optionsAccessor.Value.ElasticSearchString;
            _mongoDBString = optionsAccessor.Value.MongoDBString;

            _logger.LogInformation("LoadDataController initialized");
        }

        /// <summary>
        /// Validates and updates schema information.
        /// </summary>
        private async Task<TypeConfig> CheckSchemaAndUpdate(
            int projectId,
            int readerTypeId,
            string fileName,
            int fileId,
            List<TypeConfig> typeConfigList,
            string configuration)
        {
            _logger.LogInformation($"Checking schema for file {fileId} (reader {readerTypeId})");

            string className = string.Empty;
            List<FieldInfo> fieldInfoList = null;

            switch (readerTypeId)
            {
                case 1: // CSV
                    if (!string.IsNullOrEmpty(configuration))
                    {
                        var csvConf = JsonConvert.DeserializeObject<CsvReaderConfiguration>(configuration);
                        fieldInfoList = new CsvModelGenerator().GetAllFields(fileName, ref className, csvConf.delimiter, "", "", csvConf);
                    }
                    break;

                case 2: // JSON
                    if (!fileName.Contains("twitter"))
                    {
                        string classString = "";
                        fieldInfoList = new JsonModelGenerator().GetAllFields(fileName, configuration, ref className, ref classString, "test");
                    }
                    break;
            }

            if (fieldInfoList == null || fieldInfoList.Count == 0)
            {
                _logger.LogWarning($"No field info found for file {fileName}");
                return null;
            }

            var matchedTypeConfig = typeConfigList.Find(
                x => _previewRegistry.CompareBaseFields(x.BaseClassFields, fieldInfoList) == PreviewRegistry.EnumSchemaDiffType.SameBase);

            if (matchedTypeConfig != null)
            {
                _logger.LogInformation($"Matched schema {matchedTypeConfig.SchemaId} for file {fileId}");
                await _repository.SetSchemaId(fileId, matchedTypeConfig.SchemaId);
            }
            else
            {
                _logger.LogInformation($"No matching schema found for file {fileId}");
            }

            return matchedTypeConfig;
        }

        /// <summary>
        /// Starts a data loading job.
        /// </summary>
        [HttpPost("{ProjectId}/loadmodel")]
        public async Task<ActionResult<int>> Post(int ProjectId, [FromBody] int[] fileIds)
        {
            _logger.LogInformation($"Starting load job for project {ProjectId} with {fileIds.Length} files");

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int jobId = _repository.GetNewJobId();
            _logger.LogInformation($"Created job ID: {jobId} for user {userId}");

            var files = await _repository.GetProjectFiles(ProjectId, fileIds);
            if (files == null) return BadRequest("No files found for the specified project");

            var typeConfigList = await BuildTypeConfigList(userId, ProjectId);

            await _repository.AddJob(userId, ProjectId, jobId, 0, fileIds.ToList());
            _logger.LogInformation($"Job {jobId} added to repository");

            foreach (var file in files)
            {
                await ProcessFile(userId, ProjectId, jobId, file, typeConfigList);
            }

            _logger.LogInformation($"Job {jobId} processing started successfully");
            return jobId;
        }

        /// <summary>
        /// Builds a list of TypeConfigs for the project.
        /// </summary>
        private async Task<List<TypeConfig>> BuildTypeConfigList(int userId, int projectId)
        {
            var typeConfigList = new List<TypeConfig>();
            var projectSchemas = await _repository.GetSchemasAsync(userId, projectId, true);

            if (projectSchemas == null) return typeConfigList;

            var schemaDTOs = _mapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchemas);

            foreach (var schemaDTO in schemaDTOs)
            {
                var objTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(schemaDTO.TypeConfig);
                var schemaModels = await _repository.GetModelsAsync(userId, schemaDTO.SchemaId);

                foreach (var modelInfo in objTypeConfig.ModelInfoList)
                {
                    var matchedModel = schemaModels.FirstOrDefault(m => m?.ModelName == modelInfo.ModelName);
                    if (matchedModel != null) modelInfo.ModelId = matchedModel.ModelId;
                }

                objTypeConfig.SchemaName = schemaDTO.SchemaName;
                objTypeConfig.SchemaId = schemaDTO.SchemaId;

                typeConfigList.Add(objTypeConfig);
            }

            return typeConfigList;
        }

        /// <summary>
        /// Processes a single file.
        /// </summary>
        private async Task ProcessFile(int userId, int projectId, int jobId, ProjectFile file, List<TypeConfig> typeConfigList)
        {
            int readerTypeId = GetReaderType(file.FileName);
            string fullPath = string.IsNullOrEmpty(file.FileName) ? "twitter" : Path.Combine(file.FilePath, file.FileName);

            string configuration = await GetFileConfiguration(file, fullPath);

            var writers = await _repository.GetWritersInProject(userId, projectId);
            TypeConfig schema = null;

            if (readerTypeId == 1 || readerTypeId == 2)
            {
                schema = await CheckSchemaAndUpdate(projectId, readerTypeId, fullPath, file.ProjectFileId, typeConfigList, configuration);
                if (schema == null) return;
                await _loadModels.Execute(userId, fullPath, new List<TypeConfig> { schema }, file.ProjectFileId, jobId, configuration, projectId,
                                         _connectionString, _postgresWriter, writers, _elasticSearch, _mongoDBString);
            }
            else
            {
                await _loadModels.Execute(userId, fullPath, typeConfigList, file.ProjectFileId, jobId, configuration, projectId,
                                         _connectionString, _postgresWriter, writers, _elasticSearch, _mongoDBString);
            }

            _logger.LogInformation($"Processed file {file.ProjectFileId} successfully");
        }

        /// <summary>
        /// Determines reader type from file extension.
        /// </summary>
        private int GetReaderType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
             return 0;

            if (fileName.Contains(".csv"))
                return 1;
            else if (fileName.Contains(".json"))
                return 2;
            else if (fileName.Contains(".log"))
                return 3;
            
            return 0;
        }

        /// <summary>
        /// Retrieves configuration for a file.
        /// </summary>
        private async Task<string> GetFileConfiguration(ProjectFile file, string fullPath)
        {
            if (fullPath == "twitter") return file.SourceConfiguration;

            var reader = await _repository.GetReaderAsync((int)file.ReaderId);
            return reader.ReaderConfiguration;
        }
    }
}
