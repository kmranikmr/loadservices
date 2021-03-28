//using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.SharedUtils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAnalyticsWebApi
{
    public class PreviewRequest
    {
        public string FileName { get; set; }
    }
    public class PreviewUpdate
    {
        public string FileName { get; set; }
        public TypeConfig updatedConfig { get; set; }
    }

    public class PreviewUpdateResponse
    {
        public string SchemaId { get; set; }
        public Dictionary<string, List<BaseModel> > ModelsPreview { get; set; }
    }
    public class LoadRequest
    {
        public string FileName { get; set; }
        public string SchemaId { get; set; }
    }
}
