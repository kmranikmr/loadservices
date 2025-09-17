
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataAnalyticsPlatform.Shared.Models
{
    public class UserModelKey
    {
        public string User { get; set; }
        public string UserId { get; set; }
        public string DatasetId { get; set; }
        public string ModelId { get; set; }
        public string ModelName { get; set; }
    }

    public class SchemaModel
    {
        public string SchemaId { get; set; }
        public string ClassDefinition { get; set; }
        public string ClassoDefinitionXML { get; set; }
        public object ModelObject { get; set; }
        
        [JsonIgnore]
        public List<Type> AllTypes { get; set; }
        
        public TypeConfig TypeCoonfiguration { get; set; }
        public List<FieldInfo> ListOfFieldInfo { get; set; }

        public SchemaModel()
        {
            AllTypes = new List<Type>();
            SchemaId = Guid.NewGuid().ToString();
        }
    }

    public class SchemaModels
    {
        public List<SchemaModel> SModels { get; set; } = new List<SchemaModel>();
    }

    public class Transformations
    {
        // Empty class, placeholder for future implementation
    }

    public class TransformedModel
    {
        public List<object> TModels { get; set; }
        public List<Type> ModelTypes { get; set; }
    }
    
    public class PreviewRegistry
    {
        public Dictionary<int, SchemaModels> Models { get; private set; }
        public Dictionary<int, TransformedModel> TransformedModels { get; private set; }
        public Dictionary<int, Type> _dictSchemaType { get; private set; } = new Dictionary<int, Type>();
        
        public PreviewRegistry()
        {
            Models = new Dictionary<int, SchemaModels>();
            TransformedModels = new Dictionary<int, TransformedModel>();
        }

        public SchemaModels GetFromRegistry(int userId)
        {
            if (Models.ContainsKey(userId))
            {
                return Models[userId];
            }
            return null;
        }
        
        public void AddToRegistry(SchemaModel model, int userId, bool allowOne = false)
        {
            if (Models.ContainsKey(userId))
            {
                if (allowOne && Models[userId].SModels != null)
                {
                    Models[userId].SModels.Clear();
                }
                Models[userId].SModels.Add(model);
            }
            else
            {
                SchemaModels models = new SchemaModels();
                models.SModels.Add(model);
                Models.Add(userId, models);
            }
        }
        public bool CompareTypeConfig(TypeConfig inTypeConfig, TypeConfig otherConfig)
        {
            if (inTypeConfig == null || otherConfig == null)
            {
                return false;
            }
            
            bool same_base = false;
            bool same_models = false;
            
            // Check if collections have the same count
            if ((inTypeConfig.BaseClassFields.Count != otherConfig.BaseClassFields.Count) || 
                (inTypeConfig.ModelInfoList.Count != otherConfig.ModelInfoList.Count))
            {
                return false;
            }
            
            // Check equality of collections
            bool rv = Helper.ScrambledEquals(inTypeConfig.BaseClassFields, otherConfig.BaseClassFields);
            
            // Compare base class fields
            if (inTypeConfig.BaseClassFields.All(item => otherConfig.BaseClassFields.Contains(item)) &&
                otherConfig.BaseClassFields.All(item => inTypeConfig.BaseClassFields.Contains(item)))
            {
                same_base = true;
            }
            
            // Compare model info lists
            if (inTypeConfig.ModelInfoList.All(item => otherConfig.ModelInfoList.Contains(item)) &&
                otherConfig.ModelInfoList.All(item => inTypeConfig.ModelInfoList.Contains(item)))
            {
                same_models = true;
            }
            
            return same_base && same_models;
        }
    }
}
