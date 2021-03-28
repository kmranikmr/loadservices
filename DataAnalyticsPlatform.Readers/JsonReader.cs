using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace DataAnalyticsPlatform.Readers
{
    public class JsonReader : BaseReader
    {
        public List<object> JsonData;
        private StreamReader _streamReader = null;
        private int ListElement = 0;
        private dynamic dynJson;
        private int FileId { get; set; }
        public JsonReader(ReaderConfiguration conf): base(conf)
        {
            _streamReader = new StreamReader(conf.SourcePath);
            FileId = Helper.GetFileId(conf.SourcePath);
            JsonData = new List<object>();
        }

        public override bool GetRecords(out IRecord record)
        {
            bool result = false;
            if (JsonData.Count == 0)
            {
                string json = _streamReader.ReadToEnd();
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                //JsonData = JsonConvert.DeserializeObject<List<object>>(json);
                var token = JToken.Parse(json);

                if (token is JArray)
                {
                    dynJson = JsonConvert.DeserializeObject(json, settings);
                    foreach (var item in dynJson)
                    {
                        var obj = JsonConvert.DeserializeObject(Convert.ToString(item), GetConfiguration().ModelType);
                        JsonData.Add(obj);
                    }
                }
                else
                {
                    var obj = JsonConvert.DeserializeObject(json, GetConfiguration().ModelType, settings);
                    JsonData.Add(obj);
                }
                
            }
            if (ListElement < JsonData.Count)
            {
                record = new SingleRecord(JsonData[ListElement]);
                record.FileId = FileId;
                result = true;
                ListElement++;
            }
            else
            {
               
                record = null;
                Dispose();
            }
            return result;
            // throw new NotImplementedException();
        }
        public override bool GetRecords(out IRecord record, Type t)
        {
            throw new NotImplementedException();
        }
        public override DataTable Preview(int size)
        {
            throw new NotImplementedException();
        }
        private void Dispose()
        {
           
            _streamReader.Dispose();
        }
    }
}
