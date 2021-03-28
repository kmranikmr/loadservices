//using DataAnalyticsPlatform.Common;

using LoadServiceApi.Shared.Models;
using LoadServiceApi.Shared;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoadServiceApi.Shared;

namespace LoadServiceApi
{
    public class PreviewRequest
    {
        public int UserId { get; set; }
        // public string FileName { get; set; }
        public List<int> FileId { get; set; }
        public int ProjectId { get; set; }

    }
    public class PreviewUpdate
    {
        public List<int> FileId { get; set; }

        public string SchemaName { get; set; }
       // public int SchemaId { get; set; }
        public TypeConfig updatedConfig { get; set; }
    }

    public class PreviewUpdateV2
    {
        // public string FileName { get; set; }
        public List<int> FileId { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public string SchemaName { get; set; }
        public int SchemaId { get; set; }
        public List<TypeConfig> updatedConfig { get; set; }
    }

  
    public class PreviewUpdateResponse
    {
        // public int SchemaId { get; set; }
        // public int UserId { get; set; }
        //  public int ProjectId { get; set; }
        //public List<int> FileId { get; set; }
       // public List<ModelMapping> ModelMapping { get; set; }
        public Dictionary<int, List<Dictionary<string, object> >> ModelsPreview { get; set; }
    }
    public class LoadRequest
    {
        public List<int> FileId { get; set; }
        public int ProjectId { get; set; }
        public int SchemaId { get; set; }
        public int UserId { get; set; }
    }

    public class ModelMapping
    {
        public string ModelName { get; set; }
        public int ModelId { get; set; }
    }

    public class SchemaModelMapping
    {
        public int SchemaId { get; set; }
        public string SchemaMName { get; set; }
        public List<ModelMapping> ModelMap { get; set; }
    }

    public class ProjectProcessInfo
    {
        public string ProjectName { get; set; }
        public int NumberofSchema { get; set; }
        public int NumberofModels { get; set; }

    }
    public class JobResult
    {
        public string SourceType { get; set; }
        public string InputName { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
