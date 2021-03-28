﻿
using System;
using System.Collections.Generic;
using System.Text;
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
        public List<Type> AllTypes { get; set;}
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

        public List<SchemaModel> SModels = new List<SchemaModel>();
    }

    public class Transformations
    {

    }

    public class TransformedModel
    {
        public List<object> TModels { get; set; }
        public List<Type> ModelTypes { get; set; }
    }
    public class PreviewRegistry
    {
        // public Dictionary<UserModelKey , SchemaModels> Models ;
        // public Dictionary<UserModelKey, TransformedModel> TransformedModels;
        //trying with simple key first
        public Dictionary<int, SchemaModels> Models;
        public Dictionary<int, TransformedModel> TransformedModels;
        public Dictionary<int, Type> _dictSchemaType = new Dictionary<int, Type>();
        public PreviewRegistry()
        {
            Models = new Dictionary<int, SchemaModels>();
            TransformedModels = new Dictionary<int, TransformedModel>();
            SchemaModels models = new SchemaModels();
           // models.SModels.Add(new SchemaModel() { ClassoDefinitionXML = "test" });
            //Models.Add(1, models);
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
            if ( Models.ContainsKey(userId))
            {
                if (allowOne)
                {
                    if (Models[userId].SModels != null)
                    {
                        Models[userId].SModels.Clear();
                    }
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
            if (inTypeConfig == null || otherConfig == null) return false;
            bool same_base = false;
            bool same_models = false;
            if ((inTypeConfig.BaseClassFields.Count != otherConfig.BaseClassFields.Count)
                || (inTypeConfig.ModelInfoList.Count != otherConfig.ModelInfoList.Count))
            {
                return false;
            }
            bool rv = Helper.ScrambledEquals(inTypeConfig.BaseClassFields, otherConfig.BaseClassFields);
            var t1 = inTypeConfig.BaseClassFields.Except(otherConfig.BaseClassFields).ToList();
            var t2 = otherConfig.BaseClassFields.Except(inTypeConfig.BaseClassFields).ToList();
            if ( (inTypeConfig.BaseClassFields.All(item => otherConfig.BaseClassFields.Contains(item)) &&
                otherConfig.BaseClassFields.All(item => inTypeConfig.BaseClassFields.Contains(item))))
            {
                same_base = true;
                //same base
            }
            if (inTypeConfig.ModelInfoList.All(item => otherConfig.ModelInfoList.Contains(item)) &&
               otherConfig.ModelInfoList.All(item => inTypeConfig.ModelInfoList.Contains(item)))
            {
                same_models = true;
            }
            if ( same_base && same_models)
            {
                return true;
            }
            return false;
        }
        //public T Create( int userId, int SchemaId, params object[] args) where T: class
        //{
        //    Type type = null;
        //    if (_dictSchemaType.TryGetValue(SchemaId, out type))
        //        return (T)Activator.CreateInstance(type, args);
        //    return default(T);
        //}

        //public void Register<Tderived>(int userId, int SchemaId) where Tderived : T
        //{
        //    var type = typeof(Tderived);
        //    _dictSchemaType.Add(SchemaId, type);
        //}
    }
}