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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoadServiceApi.Controllers
{
    // [Produces("application/json")]
    [Route("api/LoadData")]
    [Authorize]
    [ApiController]

    public class LoadDataController : Controller
    {
        private readonly IHubContext<ProgressHub> _progressHubContext;
        private readonly IRepository _repository;
        private readonly IQueue _queue;
        private readonly IMapper _mapper;
        private int _numRunningJobs;
        private LoadModels LoadModels { get; set; }
        private const int MaxJobCount = 5;
        private PreviewRegistry previewRegistry;
        private string _connectionString;
        private string _postgresWriter;
        private string _elasticSearch;
        private string _mongoDBString;
        //  private LoadModels LoadModels { get; set; }
        //IHubContext<ProgressHub> progressHubContext,
        public LoadDataController(IRepository repo, IMapper mapper, LoadModels LoadModels, IOptions<ConnectionStringsConfig> optionsAccessor)//LoadModels LoadModels)
        {
            int g = 0;
            // _progressHubContext = progressHubContext;
            _repository = repo;
            _mapper = mapper;
            //  _queue = queue;
            previewRegistry = new PreviewRegistry();
            this.LoadModels = LoadModels;
            _connectionString = optionsAccessor.Value.DefaultConnection;
            _postgresWriter = optionsAccessor.Value.PostgresConnection;
            _elasticSearch = optionsAccessor.Value.ElasticSearchString;
            _mongoDBString = optionsAccessor.Value.MongoDBString;
            // this.LoadModels = LoadModels;
        }

        public async Task<TypeConfig> CheckSchemaAndUpdate(int ProjectId, int reader_type_id, string file_name, int file_id, List<TypeConfig> TypeConfigList, string configuration)
        {
            string className = string.Empty;
            if (reader_type_id == 1)
            {
                CsvReaderConfiguration Csvconf = null;
                if (!string.IsNullOrEmpty(configuration))
                {
                    Csvconf = JsonConvert.DeserializeObject<CsvReaderConfiguration>(configuration);
                }
                var fieldInfoList = new CsvModelGenerator().GetAllFields(file_name, ref className, ((CsvReaderConfiguration)Csvconf).delimiter, "", "", (CsvReaderConfiguration)Csvconf);
                if (fieldInfoList == null || fieldInfoList.Count == 0) return null;

                var matchedTypeConfig = TypeConfigList.Find(x => previewRegistry.CompareBaseFields(x.BaseClassFields, fieldInfoList) == PreviewRegistry.EnumSchemaDiffType.SameBase);
                if (matchedTypeConfig != null)
                {
                    await _repository.SetSchemaId(file_id, matchedTypeConfig.SchemaId);
                    return matchedTypeConfig;
                }
            }
            else if (reader_type_id == 2 && !file_name.Contains("twitter"))
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
        [HttpPost("{ProjectId}/loadmodel")]
        public async Task<ActionResult<int>> Post(int ProjectId, [FromBody] int[] FileId)//we will make it a different incoming class with more props
        {

            int count = 10000;
            int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int jobId = _repository.GetNewJobId();
            Console.WriteLine("job Id : " + jobId);
            var files = await _repository.GetProjectFiles(ProjectId, FileId);
            if (files == null)
                Console.WriteLine("files: null");

            Console.WriteLine("files: " + files);
            var projectSchemas = await _repository.GetSchemasAsync(userId, ProjectId, true);
            Console.WriteLine("files: " + files);
            List<TypeConfig> TypeConfigList = new List<TypeConfig>();

            if (projectSchemas != null)
            {
                var retMap = _mapper.Map<ProjectSchema[], SchemaDTO[]>(projectSchemas);
                foreach (var projectSchema in retMap)
                {

                    var objTypeConfig = JsonConvert.DeserializeObject<TypeConfig>(projectSchema.TypeConfig);
                    var schemaModelArray = _repository.GetModelsAsync(userId, projectSchema.SchemaId);
                    foreach (var configObj in objTypeConfig.ModelInfoList)
                    {
                        for (int j = 0; j < schemaModelArray.Result.Length; j++)
                        {
                            if (schemaModelArray.Result[j] != null)
                            {
                                if (configObj.ModelName == schemaModelArray.Result[j].ModelName)
                                {
                                    configObj.ModelId = schemaModelArray.Result[j].ModelId;
                                }
                            }
                        }
                    }
                    objTypeConfig.SchemaName = projectSchema.SchemaName;
                    objTypeConfig.SchemaId = projectSchema.SchemaId;
                    if (objTypeConfig != null)
                    {
                        TypeConfigList.Add(objTypeConfig);
                    }
                }
            }
            await _repository.AddJob(userId, ProjectId, jobId, 0, FileId.ToList());

            string fullPath = "";
            ; foreach (var file in files)
            {
                fullPath = "";
                int readerTypeId = 0;
                if (!string.IsNullOrEmpty(file.FileName))
                {

                    if (file.FileName.Contains(".csv"))
                        readerTypeId = 1;
                    else if (file.FileName.Contains(".json"))
                        readerTypeId = 2;
                    else if (file.FileName.Contains(".log"))
                        readerTypeId = 3;
                    fullPath = Path.Combine(file.FilePath, file.FileName);
                }
                else
                {
                    fullPath = "twitter";
                }

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

                var writer = await _repository.GetWritersInProject(userId, ProjectId);
                TypeConfig retSchema = null;
                if (readerTypeId == 1 || readerTypeId == 2)
                {
                    retSchema = await CheckSchemaAndUpdate(ProjectId, readerTypeId, fullPath, file.ProjectFileId, TypeConfigList, Configuration);
                    if (retSchema == null) continue;

                    await LoadModels.Execute(userId, fullPath, new List<TypeConfig> { retSchema }, file.ProjectFileId, jobId, Configuration, ProjectId, _connectionString, _postgresWriter, writer, _elasticSearch, _mongoDBString);
                }
                else
                {
                    await LoadModels.Execute(userId, fullPath, TypeConfigList, file.ProjectFileId, jobId, Configuration, ProjectId, _connectionString, _postgresWriter, writer, _elasticSearch, _mongoDBString);
                }



            }
            return jobId;
        }

        //reader.
        //        var readers = await _repository.GetReadersInProjectByTypes(ProjectId, readerTypeId);
        //ReaderDTO[] readerDTOs = null;
        //        if (readers != null && readers.Any())
        //        {
        //            readerDTOs = _mapper.Map<ReaderDTO[]>(readers);
        //        }
        //[HttpPost("{ProjectId}/loadmodel")]
        //public async Task<ActionResult<int>> Post1(int ProjectId, [FromBody]int[] FileId)//we will make it a different incoming class with more props
        //{

        //    int count = 10000;
        //    int userId = Convert.ToInt32(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //    int jobId = _repository.GetNewJobId();
        //    await _repository.AddJob(userId, ProjectId, jobId, 0, FileId.ToList());
        //    BackgroundWorker bg = new BackgroundWorker();
        //    bg.DoWork += (obj, e) => backgroundWorker1_DoWork(jobId, FileId.ToList(), _repository);
        //    bg.RunWorkerAsync();
        //    //  _queue.QueueAsyncTask(() => PerformBackgroundJob(jobId, request.FileId));
        //    // await Task.Run(()=>PerformBackgroundJob());
        //    return jobId;
        //    //var result = await this.LoadModels.Execute(id, request.FileName);
        //    //return result;
        //    // return 1;
        //}
        private async void backgroundWorker1_DoWork(int jobId, List<int> fileId, IRepository _repository)// object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var options = SqlServerDbContextOptionsExtensions.UseSqlServer<DAPDbContext>(new DbContextOptionsBuilder<DAPDbContext>(), "Server=localhost\\SQLEXPRESS;Database=dap_master;Trusted_Connection=True;").Options;
            var dbContext = new DAPDbContext(options);
            IRepository repo = new Repository(dbContext, null);
            for (int j = 0; j < fileId.Count; j++)
            {
                await repo.UpdateJobStatus(jobId, 2, fileId[j]);
                await repo.UpdateJobStart(jobId, fileId[j]);
                for (int i = 0; i <= 100; i += 1)
                {
                    // await _progressHubContext.Clients.User(User.Identity?.Name)//(ProgressHub.GROUP_NAME)
                    //                          .SendAsync("processing", (int)(i  / 100));

                    // Debug.WriteLine($"Job COntinuing{i}");
                    await Task.Delay(200);

                }

                await repo.UpdateJobEnd(jobId, fileId[j]);
                await repo.UpdateJobStatus(jobId, 3, fileId[j]);
            }
        }
        private async Task PerformBackgroundJob(int jobId, List<int> fileId)
        {
            for (int j = 0; j < fileId.Count; j++)
            {
                await _repository.UpdateJobStatus(jobId, 2, fileId[j]);
                for (int i = 0; i <= 100; i += 1)
                {
                    // await _progressHubContext.Clients.User(User.Identity?.Name)//(ProgressHub.GROUP_NAME)
                    //                          .SendAsync("processing", (int)(i  / 100));

                    Debug.WriteLine($"Job COntinuing{i}");
                    await Task.Delay(100);

                }
                await _repository.UpdateJobStatus(jobId, 3, fileId[j]);
            }
        }
    }
}
