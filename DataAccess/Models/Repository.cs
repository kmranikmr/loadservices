using DataAccess.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Repository : IRepository
    {

        private readonly DAPDbContext _context;

        private readonly ILogger<Repository> _logger;


        public Repository(DAPDbContext context, ILogger<Repository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add<T>(T entity) where T : class
        {
           // _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
           // _logger.LogInformation($"Removing an object of type {entity.GetType()} to the context.");
            _context.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

        #region Project

        public async Task<Project> GetProjectByName(string projectName)
        {
            IQueryable<Project> query = _context.Projects.Where(x => string.Compare(x.ProjectName, projectName, true) == 0);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Project[]> GetAllProjectsByUserId(int userId, bool includeSummary = false)
        {
            IQueryable<Project> query = _context.Projects;

            if (includeSummary)
            {
                query = query.Include(c => c.ProjectSchemas).Include(p => p.Jobs);
            }

            query = query.Where(x => x.CreatedBy == userId && x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }

        public async Task<Project[]> GetAllProjectsAsync(bool includeSummary = false)
        {
            IQueryable<Project> query = _context.Projects;

            if (includeSummary)
            {
                query = query.Include(c => c.ProjectSchemas).Include(p => p.Jobs);
            }

            query = query.Where(x => x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }

        public async Task<Project> GetProjectAsync(int userId, int projectId, bool includeSummary = false)
        {
            IQueryable<Project> query = _context.Projects;

            if (includeSummary)
            {
                query = query.Include(c => c.ProjectSchemas).Include(p => p.Jobs);
            }

            query = query.Where(x => x.CreatedBy == userId && x.ProjectId == projectId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Project> SetFavorite(int userId, int projectId, bool flag)
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project != null && project.CreatedBy == userId)
            {
                project.IsFavorite = flag;

                _context.Entry(project).Property(x => x.IsFavorite).IsModified = true;

                await _context.SaveChangesAsync();

                return project;
            }

            return null;
        }
        #endregion

        #region Models
        public async Task<SchemaModel[]> GetModelsAsync(int userId, int schemaId)
        {
            IQueryable<SchemaModel> query = _context.SchemaModels;

            query = query.Where(x => x.UserId == userId && x.SchemaId == schemaId && x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }


        #endregion

        #region Schema

        public async Task<ProjectSchema[]> GetSchemasAsync(int userId, int projectId, bool includeModels = false)
        {
            IQueryable<ProjectSchema> query = _context.ProjectSchemas;

            if (includeModels)
            {
                query = query.Include(ps => ps.SchemaModels)
                    .ThenInclude(m => m.ModelMetadatas);
            }

            query = query.Where(x => x.ProjectId == projectId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }
        public async Task<bool>  AddSchemaAsync(ProjectSchema projectSchema)
        {

            //projectSchema.SchemaModels = new List<SchemaModel>() { };

            // _context.Entry(projectSchema.SchemaModels).State = EntityState.Unchanged;
            // foreach ( ModelMetadata mdata in projectSchema.SchemaModels.Select(x=>x.ModelMetadatas))
            // {
            //     _context.Entry(mdata).State = EntityState.Unchanged;
            // }

            _context.ProjectSchemas.Add(projectSchema);
            _context.Entry(projectSchema).State = EntityState.Added;
            return await _context.SaveChangesAsync() > 0 ;

            
        }
        public async Task<ProjectSchema> SetSchemaAsync(int schemaId, ProjectSchema projectSchema)
        {
            try {

                if (schemaId != 0)
                {
                    var pschema = await _context.ProjectSchemas.FindAsync(schemaId);
                   // _context.Entry(projectSchema).State = EntityState.Modified;
                    _context.Entry(pschema).CurrentValues.SetValues(projectSchema);
                   
                    foreach (var model in projectSchema.SchemaModels)
                    {
                        
                        model.UserId = projectSchema.UserId;
                        model.SchemaId = projectSchema.SchemaId;
                        model.IsActive = projectSchema.IsActive;
                        var existingChild = pschema.SchemaModels
                            .Where(c => ( (c.ModelId == model.ModelId ) ||(model.ModelId == 0 && model.ModelConfig == null)))
                            .SingleOrDefault();

                        if (existingChild != null)
                        {
                            if ( model.ModelId == 0 && model.ModelConfig == null)
                            {
                                model.ModelId = existingChild.ModelId;
                                model.ProjectId = existingChild.ProjectId;
                            }
                            _context.Entry(existingChild).CurrentValues.SetValues(model);

                        }
                        else
                        {
                       

                            {
                                var newModel = new SchemaModel { ModelConfig = model.ModelConfig, ModelName = model.ModelName, UserId = model.UserId, ProjectId = projectSchema.ProjectId, ModelMetadatas = model.ModelMetadatas };
                                pschema.SchemaModels.Add(newModel);
                            }
                        }

                    }

                   // if (projectSchema.SchemaModels.Count == 0 )
                    {
                        //if (projectSchema.ModelMetadatas != null)
                        {
                           
                            //_context.Entry(pschema.ModelMetadatas).CurrentValues.SetValues(projectSchema.ModelMetadatas);
                           // foreach (var meta in projectSchema.ModelMetadatas)
                          //  {
                             //   _context.ModelMetadatas.Add(meta);
                                //    var metaCopy = new ModelMetadata
                                //    {
                                //        ProjectId = meta.ProjectId,
                                //        ColumnName = meta.ColumnName,
                                //        DataType = meta.DataType,

                           // };
                            //    pschema.ModelMetadatas.Add(metaCopy);
                           // }

                        }
                      //  _context.Entry(pschema.ModelMetadatas).CurrentValues.SetValues(projectSchema.ModelMetadatas);
                    }

                    //    pschema.SchemaName = projectSchema.SchemaName;
                    //pschema.SchemaModels = projectSchema.SchemaModels;
                    //pschema.TypeConfig = projectSchema.TypeConfig;
                  
                    // pschema = projectSchema;
                   
                    await _context.SaveChangesAsync();
                    return pschema;
                }
                return null;
            }
            catch (Exception ex)
            {
                int G = 0;
                return null;
            }
        }

        public async Task<bool> DeleteSchema(int userId, int projectId, int schemaId)
        {
            var pSchema = await _context.ProjectSchemas.FindAsync(schemaId);

            if (pSchema != null && pSchema.UserId == userId)
            {
                pSchema.IsDeleted = true;

                _context.Entry(pSchema).Property(x => x.IsDeleted).IsModified = true;

                return await _context.SaveChangesAsync() > 0;

            }

            return false;
        }
        public async Task<ProjectSchema> GetSchemaAsync(int schemaId, bool includeModels = false)
        {
            IQueryable<ProjectSchema> query = _context.ProjectSchemas;

            if (includeModels)
            {
                query = query.Include(ps => ps.SchemaModels)
                    .ThenInclude(m => m.ModelMetadatas);
            }
           
            query = query.Where(x => x.SchemaId == schemaId && x.IsDeleted == false && x.IsActive == true);

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #region Jobs

        public async Task<Job[]> GetJobsInProject(int userId, int projectId)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Where(x => x.UserId == userId && x.ProjectId == projectId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }

        public int GetNewJobId()
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT NEXT VALUE FOR job_sequence";
                _context.Database.OpenConnection();

                var jobId = command.ExecuteScalar();

                return Convert.ToInt32(jobId);
            }
        }

        public async Task<Job[]> GetJobSummary(int jobId)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Include(j => j.Project).Include(j => j.ProjectFile).ThenInclude(pf=>pf.Reader).ThenInclude(r=>r.ReaderType);

            query = query.Include(j => j.ProjectFile).ThenInclude(pf => pf.SourceType);

            query = query.Where(x => x.JobId == jobId && x.IsActive == true && !x.IsDeleted);

            return await query.ToArrayAsync();
        }

        public async Task<Job> GetJobAsync(int userId, int jobId)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Where(x => x.UserId == userId && x.JobId == jobId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Job> UpdateJobStatus(int jobId, int statusId, int projectFileId)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(jobId, projectFileId);

                if (job != null)
                {
                    job.JobStatusId = statusId;
                    _context.Entry(job).Property(x => x.JobStatusId).IsModified = true;
                    await _context.SaveChangesAsync();
                    return job;
                }
                return null;
            }catch(Exception ex)
            {
                int j = 0;
                return null;
            }
        }

        public Job UpdateJobStatusSync(int jobId, int statusId, int projectFileId)
        {
            try
            {
                var job = _context.Jobs.Find(jobId, projectFileId);

                if (job != null)
                {
                    job.JobStatusId = statusId;
                    _context.Entry(job).Property(x => x.JobStatusId).IsModified = true;
                    _context.SaveChanges();
                    return job;
                }
                return null;
            }
            catch (Exception ex)
            {
                int j = 0;
                return null;
            }
        }
        public async Task<bool> AddJob(int userId, int projectId, int jobId, int schemaId, List<int> FileId)
        {
            IQueryable<Job> query = _context.Jobs;

            query = query.Include(j => j.JobStatus);

            query = query.Where(x => x.UserId == userId && x.ProjectId == projectId && x.IsActive == true && !x.IsDeleted);

            var jobs = query.Where(x => x.JobStatus.StatusName == "Created" && FileId.Any(y => y == x.ProjectFileId)).ToList();

            //if (jobs != null && jobs.Count > 0 )
            //{
            //    foreach (Job job in jobs)
            //    {
            //        // job.IsActive = false;
            //        job.IsDeleted = true;
            //        _context.Entry(job).Property(x => x.IsDeleted).IsModified = true;
            //        await _context.SaveChangesAsync();
            //    }
            //}

            var projFiles =  _context.ProjectFiles.Where(x=> FileId.Any(y => y == x.ProjectFileId)).ToList();

            foreach (ProjectFile projFile in projFiles)
            {
                if (projFile.SchemaId == null) continue;

                var schema = await GetSchemaAsync((int)projFile.SchemaId, true);
                var foundJob = jobs.Find(x => x.ProjectFileId == projFile.ProjectFileId);
                //if (foundJob != null)
                //{
                //    if ( foundJob.JobStatusId == 1)
                //    {
                //        foundJob.SchemaId = (int)projFile.SchemaId;
                //        _context.Entry(foundJob).Property(x => x.SchemaId).IsModified = true;
                       
                //    }
                //}
                //else
                //{
                    Job job1 = new Job()
                    {
                        JobId = jobId,
                        ProjectFileId = projFile.ProjectFileId,
                        SchemaId = projFile.SchemaId,
                        ProjectId = projectId,
                        UserId = userId,
                        JobStatusId = 1,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = DateTime.Now,
                        JobDescription = schema.TypeConfig

                    };
                    _context.Jobs.Add(job1);
                    //_context.Entry(job1).State = EntityState.Added;
                   
                //}
            }
            await _context.SaveChangesAsync();



            return true;
        }
        public async Task<bool> UpdateJob(int userId, int projectId, int schemaId, List<int> FileId)
        {
            return false;
        }
        public async Task<bool> UpdateJobStart(int jobId, int projectFileId)
        {
            var job = await _context.Jobs.FindAsync(jobId, projectFileId);

            if (job != null)
            {
                job.StartedOn = DateTime.Now;
                _context.Entry(job).Property(x => x.StartedOn).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> UpdateJobEnd(int jobId, int projectFileId)
        {
            var job = await _context.Jobs.FindAsync(jobId, projectFileId);

            if (job != null)
            {
                job.CompletedOn = DateTime.Now;
                _context.Entry(job).Property(x => x.CompletedOn).IsModified = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region Writers

        public async Task<Writer[]> GetWritersInProject(int userId, int projectId)
        {
            IQueryable<ProjectWriter> query = _context.ProjectWriters.Include(pr => pr.Project).Include(pr => pr.Writer).ThenInclude(w => w.WriterType);

            query = query.Where(x => x.ProjectId == projectId && x.Project.CreatedBy == userId && x.Writer.IsDeleted == false && x.Writer.IsActive == true);

            return await query.Select(x => x.Writer).Include(x=>x.WriterType).ToArrayAsync();
        }

        public async Task<Writer[]> GetWritersViaUser(int userId)
        {
            IQueryable<Writer> query = _context.Writers;

            query = query.Include(x => x.WriterType);

            query = query.Where(x => x.UserId == userId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<Writer[]> GetWriters()
        {
            IQueryable<Writer> query = _context.Writers;

            query = query.Include(x => x.WriterType);

            query = query.Where(x => x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<Writer> GetWriterAsync(int writerId)
        {
            IQueryable<Writer> query = _context.Writers;

            query = query.Include(x => x.WriterType);

            query = query.Where(x => x.WriterId == writerId && x.IsDeleted == false && x.IsActive == true);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ProjectWriter> GetProjectWriterAsync(int projectId, int writerId)
        {
            IQueryable<ProjectWriter> query = _context.ProjectWriters;

            query = query.Where(x => x.WriterId == writerId && x.ProjectId == projectId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<WriterType[]> GetWriterTypes()
        {
            IQueryable<WriterType> query = _context.WriterTypes;

            query = query.Where(x => x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        #endregion

        #region Readers

        public async Task<Reader[]> GetReadersInProject(int userId, int projectId)
        {
            IQueryable<ProjectReader> query = _context.ProjectReaders.Include(pr => pr.Project).Include(pr => pr.Reader);

            query = query.Where(x => x.ProjectId == projectId && x.Project.CreatedBy == userId && x.Reader.IsDeleted == false && x.Reader.IsActive == true);

            return await query.Select(x => x.Reader).ToArrayAsync();
        }

        public async Task<Reader[]> GetReadersViaUser(int userId)
        {
            IQueryable<Reader> query = _context.Readers;

            query = query.Where(x => x.UserId == userId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<Reader[]> GetReadersInProjectByTypes(int projectId, int readerTypeId)
        {
            IQueryable<ProjectReader> query = _context.ProjectReaders.Include(pr => pr.Reader);

            query = query.Where(x => x.ProjectId == projectId && x.Reader.ReaderTypeId == readerTypeId && x.Reader.IsDeleted == false && x.Reader.IsActive == true);

            return await query.Select(x => x.Reader).ToArrayAsync();
        }

        public async Task<Reader> GetReaderAsync(int readerId)
        {
            Console.WriteLine(" reader async");
            IQueryable<Reader> query = _context.Readers;
            Console.WriteLine(" reader async 2");
            query = query.Where(x => x.ReaderId == readerId && x.IsDeleted == false && x.IsActive == true);
            Console.WriteLine(" reader async 3");
            return await query?.FirstOrDefaultAsync();

        }

        public async Task<ReaderType[]> GetReaderTypes()
        {
            IQueryable<ReaderType> query = _context.ReaderTypes;

            query = query.Where(x => x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<bool> DeleteReader(int userId, int readerId)
        {
            var reader = await _context.Readers.FindAsync(readerId);

            if (reader != null && reader.UserId == userId)
            {
                reader.IsDeleted = true;

                _context.Entry(reader).Property(x => x.IsDeleted).IsModified = true;

                return await _context.SaveChangesAsync() > 0;

            }

            return false;
        }



        #endregion

        #region ProjectFiles

        public async Task<int>AddProjectFiles(ProjectFile projectfile)
        {

            //projectSchema.SchemaModels = new List<SchemaModel>() { };

            // _context.Entry(projectSchema.SchemaModels).State = EntityState.Unchanged;
            // foreach ( ModelMetadata mdata in projectSchema.SchemaModels.Select(x=>x.ModelMetadatas))
            // {
            //     _context.Entry(mdata).State = EntityState.Unchanged;
            // }
          
            _context.ProjectFiles.Add(projectfile);
            _context.Entry(projectfile).State = EntityState.Added;
            await _context.SaveChangesAsync();
            return projectfile.ProjectFileId;


        }
        public async Task<ProjectFile[]> GetProjectFiles(int projectId, int sourceTypeId)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles;

            query = query.Where(x => x.ProjectId == projectId && x.SourceTypeId == sourceTypeId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<ProjectFile[]> GetProjectFiles(int projectId)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles;

            query = query.Where(x => x.ProjectId == projectId && x.IsDeleted == false && x.IsActive == true);

            return await query.ToArrayAsync();
        }

        public async Task<ProjectFile[]> GetProjectFiles(int projectId, int[] fileId)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles;

            query = query.Where(x => x.ProjectId == projectId && x.IsDeleted == false && x.IsActive == true && fileId.Contains(x.ProjectFileId) );

            return await query.ToArrayAsync();
        }
        public  async Task<bool> SetReaderId(Dictionary<int,int> projectFileIdReaderIdDict)
        {
            IQueryable<ProjectFile> query = _context.ProjectFiles.Where(pf => projectFileIdReaderIdDict.ContainsKey(pf.ProjectFileId));

            var projectFiles = await query.ToArrayAsync();

            if (projectFiles.Any())
            {
                foreach (var item in projectFiles)
                {
                    item.ReaderId = projectFileIdReaderIdDict[item.ProjectFileId];
                    _context.Entry(item).Property(x => x.ReaderId).IsModified = true;
                }

                return await _context.SaveChangesAsync() > 0;
            }

            return false;            
        }

        public async Task<bool> SetSchemaId(int projectFIleId, int schemaId)
        {
            ProjectFile projectFile = await _context.ProjectFiles.FindAsync(projectFIleId);


            if (projectFile != null)
            {
                projectFile.SchemaId = schemaId;
                _context.Entry(projectFile).Property(x => x.SchemaId).IsModified = true;
                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<int> GetReaderFromProjectFile(int projectFileId)
        {
            ProjectFile projectFile = await _context.ProjectFiles.FindAsync(projectFileId);
            if (projectFile != null)
            {
                return (int)projectFile.ReaderId;
            }
            return -1;
        }
        #endregion

        #region Automation
        public async Task<ProjectAutomation[]> GetProjectAutomations()
        {
            IQueryable<ProjectAutomation> query = _context.ProjectAutomations;

            query = query.Where(x => x.IsActive == true && x.IsDeleted == false);

            return await query.ToArrayAsync();
        }
        #endregion

    }
}
