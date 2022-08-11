using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Shared.PostModels
{
    public class PreviewRequest
    {
        public string FileName { get; set; }

        public PreviewRequest(string fileName)
        {
            FileName = fileName;
        }
    }
    public class PreviewUpdate
    {
        public string SchemaName { get; set; }
        public string FileName { get; set; }
        public TypeConfig updatedConfig { get; set; }
        public int[] FileId { get; set; }
        public PreviewUpdate(string fileName, TypeConfig config)
        {
            FileName = fileName;
            updatedConfig = config;
        }
    }

    public class PreviewUpdateResponse
    {
        // public int SchemaId { get; set; }
        // public int UserId { get; set; }
        //  public int ProjectId { get; set; }
        //public List<int> FileId { get; set; }
        // public List<ModelMapping> ModelMapping { get; set; }
        public Dictionary<int, List<Dictionary<string, object>>> ModelsPreview { get; set; }
    }

    public class LoadRequest
    {
        public string FileName { get; set; }
        public string SchemaId { get; set; }

        public LoadRequest(string fileName, string schemaId)
        {
            FileName = fileName;
            SchemaId = schemaId;
        }
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
}
