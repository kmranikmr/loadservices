using LoadServiceApi.Shared.Models;
using LoadServiceApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using LoadServiceApi.Shared;

namespace LoadServiceApi.TestData
{
    public class TestData
    {
        public PreviewRegistry previewRegistry { get; set; }
        public TestData()
        {
            previewRegistry = new PreviewRegistry();
        }
        public SchemaModels GetModels(int userId)
        {
            var models = previewRegistry.GetFromRegistry(userId);
            return models;
        }
        public List<TypeConfig> GenerateModel(int userId, int projectId,  int[] Ids)
        {
            List<TypeConfig> typeConfigList = new List<TypeConfig>();
            TypeConfig typeConfig = null;
            SchemaModel smodel = null;
                        //DataAccess.Models.
            try
            {
                using (StreamReader reader = new StreamReader(@"TestData\generatedmode.json"))
                {
                    if (reader != null)
                    {

                        string json = reader.ReadToEnd();
                        smodel = JsonConvert.DeserializeObject<SchemaModel>(json);
                        smodel.ProjectId = projectId;
                        previewRegistry.AddToRegistry(smodel, userId);
                    }
                }
            }
            catch(Exception ex)
            {
                int g = 0;
            }
            typeConfig = new TypeConfig();
            typeConfig.BaseClassFields = smodel.ListOfFieldInfo;
            typeConfigList.Add(typeConfig);
            smodel = null;
            using (StreamReader reader = new StreamReader(@"TestData\generatemode2.json"))
            {
                if (reader != null)
                {

                    string json = reader.ReadToEnd();
                    smodel = JsonConvert.DeserializeObject<SchemaModel>(json);
                    smodel.ProjectId = projectId;
                    previewRegistry.AddToRegistry(smodel, userId);
                }
            }
            typeConfigList.Add(new TypeConfig() { BaseClassFields = smodel.ListOfFieldInfo});
            return typeConfigList;
        }
        public PreviewUpdateResponse UpdateModel(int userId, int projectId)
        {
            PreviewUpdateResponse updateResp = null;
            //return Task.Run(() =>
           // {


                using (StreamReader reader = new StreamReader(@"TestData/updateresponse.json"))
                {
                    if (reader != null)
                    {
                        string json = reader.ReadToEnd();
                        updateResp = JsonConvert.DeserializeObject<PreviewUpdateResponse>(json);
                        //var dict = updateResp.ModelsPreview.ToDictionary(x => x.Key, y => y.Value);
                        //updateResp.ProjectId = projectId;
                        // previewRegistry.AddToRegistry(schemaModel, userId);
                    }
                }

                return updateResp;
            //}
            //);
        }
    }
}
