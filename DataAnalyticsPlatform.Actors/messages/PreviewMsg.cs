using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnalyticsPlatform.Actors.messages
{
    public partial class PreviewActor
    {
        public class GetModel : MsgUserId
        {
           // public string file_name = "";
            public GetModel(int userId = 0) : base(userId) { }
        }

        public class GenerateModel : MsgUserId
        {
            public string file_name = "";
            public int jobId { get; set; }
            public CustomConfiguration readerConfiguration { get; set; }
            public GenerateModel(int userId = 0, string file_name = "", string customConfiguration = "", int jobid = 0) : base(userId)
            {
                this.file_name = file_name;
                jobId = jobid;
                if (!string.IsNullOrEmpty(customConfiguration) )
                {
                    if (file_name.Contains(".csv"))
                    {
                        readerConfiguration = JsonConvert.DeserializeObject<CsvReaderConfiguration>(customConfiguration);
                    }
                    else if ( file_name.Contains("twitter"))
                    {
                        readerConfiguration = JsonConvert.DeserializeObject <TwitterConfiguration>(customConfiguration);
                    }
                    else
                    {
                        if (readerConfiguration == null)
                            readerConfiguration = new CustomConfiguration();
                        readerConfiguration.readerName = customConfiguration;
                    }
                }
            }
        }

        public class UpdateModel : MsgUserId
        {
            public TypeConfig typeConfig ;
            public List<ModelInfo> splitSchemaList;
            public string FileName { get; set; }
            public CustomConfiguration readerConfiguration { get; set; }
            public UpdateModel(int userId = 0, TypeConfig typeConfig =null, string FileName = "", string customConfiguration = "") : base(userId)
            {
                //this.userId = userId;
                this.splitSchemaList = typeConfig.ModelInfoList;
                this.typeConfig = typeConfig;
                this.FileName = FileName;
                if (!string.IsNullOrEmpty(customConfiguration))
                {
                    if (FileName.Contains(".csv"))
                    {
                        readerConfiguration = JsonConvert.DeserializeObject<CsvReaderConfiguration>(customConfiguration);
                    }
                    else if (FileName.Contains("twitter"))
                    {
                        readerConfiguration = JsonConvert.DeserializeObject<TwitterConfiguration>(customConfiguration);
                    }
                    else
                    {
                        if (readerConfiguration == null)
                            readerConfiguration = new CustomConfiguration();

                        readerConfiguration.readerName = customConfiguration;
                    }
                }
            }
        }

    }
}
