/*
 * This file defines the JsonReader class responsible for reading JSON data and converting it into structured records.
 * 
 * JsonReader:
 * - Inherits from BaseReader and implements methods for reading records from JSON files based on a provided configuration.
 * - Uses Newtonsoft.Json library for JSON parsing and deserialization.
 * - Initializes with a ReaderConfiguration instance containing details about the JSON file path.
 * - Reads the entire JSON content from the file into memory and deserializes it into a list of objects (JsonData).
 * - Supports deserialization into either a single object or a list of objects based on the JSON structure.
 * - Implements GetRecords method to retrieve records as IRecord instances, iterating over the JsonData list.
 * - Provides error handling for JSON parsing and deserialization exceptions.
 * - Implements Dispose method to properly dispose of resources such as the StreamReader used for file reading.
 * 
 * This class facilitates the ingestion of JSON data within the Data Analytics Platform, ensuring that JSON files are parsed and converted into structured records for further processing and analysis.
 */


using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DataAnalyticsPlatform.Readers
{
    public class JsonReader : BaseReader
    {
        public List<object> JsonData;
        private StreamReader _streamReader = null;
        private int ListElement = 0;
        private dynamic dynJson;
        private int FileId { get; set; }
        public JsonReader(ReaderConfiguration conf) : base(conf)
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
