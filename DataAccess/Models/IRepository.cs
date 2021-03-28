using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public interface IRepository
    {
        // General 
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveChangesAsync();

        // Projects
        Task<Project[]> GetAllProjectsAsync(bool includeSummary = false);
        Task<Project> GetProjectAsync(int userId, int projectId, bool includeSummary = false);
        Task<Project[]> GetAllProjectsByUserId(int userId, bool includeSummary = false);
        Task<Project> SetFavorite(int userId, int projectId, bool flag);

        //ProjectFile
        Task<ProjectFile[]> GetProjectFiles(int projectId, int sourceTypeId);
        Task<ProjectFile[]> GetProjectFiles(int projectId);
        Task<ProjectFile[]> GetProjectFiles(int projectId, int[] fileId);
        Task<int> GetReaderFromProjectFile(int projectFileId);
        Task<bool> SetReaderId(Dictionary<int, int> projectFileIdReaderIdDict);
        Task<bool> SetSchemaId(int projectFIleId, int schemaId);
        Task<int> AddProjectFiles(ProjectFile projectfile);
        // Schema - returns schemas along with models
        Task<ProjectSchema[]> GetSchemasAsync(int userId, int projectId, bool includeModels = false);
        Task<ProjectSchema> GetSchemaAsync(int schemaId, bool includeModels = false);
        Task<ProjectSchema> SetSchemaAsync(int schemaId, ProjectSchema projectSchema);
        Task<bool> AddSchemaAsync(ProjectSchema projectSchema);
        Task<bool> DeleteSchema(int userId, int projectId, int schemaId);
        //Jobs
        Task<Job[]> GetJobsInProject(int userId, int projectId);
        Task<Job[]> GetJobSummary(int projectId);
        Task<Job> GetJobAsync(int userId, int projectId);
        int GetNewJobId();
        Task<bool> AddJob(int userId, int projectId, int jobId, int schemaId, List<int> FileId);
        Task<bool> UpdateJob(int userId, int projectId, int schemaId, List<int> FileId);
        Task<Job> UpdateJobStatus(int jobId, int statusId, int projectFileId);
        Task<bool> UpdateJobStart(int jobId , int projectFileId);
        Task<bool> UpdateJobEnd(int jobId, int projectFileId);
        Job UpdateJobStatusSync(int jobId, int statusId, int projectFileId);
       
            //Readers
            Task<Writer[]> GetWritersInProject(int userId, int projectId);
        Task<Writer[]> GetWriters();
        Task<Writer> GetWriterAsync(int writerId);
        Task<ProjectWriter> GetProjectWriterAsync(int projectId, int writerId);

        //Readers
        Task<Reader[]> GetReadersInProject(int userId, int projectId);
        Task<Reader[]> GetReadersInProjectByTypes(int projectId, int readerTypeId);
        Task<Reader[]> GetReadersViaUser(int userId);
        Task<Reader> GetReaderAsync(int readerId);
        Task<ReaderType[]> GetReaderTypes();
        Task<bool> DeleteReader(int userId, int readerId);

        //Models
        Task<SchemaModel[]> GetModelsAsync(int userId, int schemaId);
        Task<Project> GetProjectByName(string projectName);
        #region automation

        Task<ProjectAutomation[]> GetProjectAutomations();

        #endregion
    }
}
