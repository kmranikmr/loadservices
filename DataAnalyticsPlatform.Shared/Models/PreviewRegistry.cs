/// <summary>
/// This  provides a set of models and utilities for handling schema and data transformation
/// within the Data Analytics Platform. It includes definitions for user models, schema models,
/// transformation processes, and a preview registry for managing model states and configurations.
/// 
/// Key Classes and Functionalities:
/// 
/// - UserModelKey: Represents key information for a user model, including user, project, and model identifiers.
/// - SchemaModel: Defines a schema model with properties such as ModelId, SchemaId, and ClassDefinition,
///   along with support for handling type configurations and field information.
/// - SchemaModels: A collection of SchemaModel instances.
/// - Transformations: Placeholder for future transformation logic and utilities.
/// - TransformedModel: Holds transformed models and their types.
/// - PreviewRegistry: Manages the registry of schema models and transformed models. Provides methods to
///   add to and retrieve from the registry, and includes functionality for comparing type configurations and
///   field information.
/// 
/// This namespace supports the dynamic handling and transformation of data models, facilitating schema 
/// comparisons and ensuring data consistency across various user and project configurations.
/// </summary>
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAnalyticsPlatform.Shared.Models
{
    public class UserModelKey
    {
        public string User { get; set; }
        public string UserId { get; set; }
        public string ProjectId { get; set; }
        public string ModelId { get; set; }
        public string ModelName { get; set; }


    }

    public class SchemaModel
    {
        public string ModelId { get; set; }
        public string SchemaId { get; set; }
        public string ClassDefinition { get; set; }
        public string ClassoDefinitionXML { get; set; }
        public object ModelObject { get; set; }
        [JsonIgnore]
        public List<Type> AllTypes { get; set; }
        public TypeConfig TypeConfiguration { get; set; }
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
            if (Models.ContainsKey(userId))
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
            if ((inTypeConfig.BaseClassFields.All(item => otherConfig.BaseClassFields.Contains(item)) &&
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
            if (same_base && same_models)
            {
                return true;
            }
            return false;
        }

        public enum EnumSchemaDiffType
        {
            None,
            SameModelsBase,
            SameBase,
            DiffBaseModels

        }
        public class FieldInfoComparer : IEqualityComparer<FieldInfo>
        {
            public bool Equals(FieldInfo x, FieldInfo y)
            {
                return x.Name.ToLower() == y.Name.ToLower();
                //throw new NotImplementedException();
            }

            public int GetHashCode(FieldInfo obj)
            {
                return 0;
                //throw new NotImplementedException();
            }
        }

        public EnumSchemaDiffType CompareBaseFields(List<FieldInfo> infieldInfoList, List<FieldInfo> otherFieldInfoList)
        {
            Console.WriteLine("CompareBaseFields ");
            if (infieldInfoList == null || otherFieldInfoList == null) return EnumSchemaDiffType.None;
            if (infieldInfoList.Count != otherFieldInfoList.Count)
            {
                Console.WriteLine("EnumSchemaDiffType.DiffBaseModels");
                // return EnumSchemaDiffType.DiffBaseModels;
            }
            Console.WriteLine("checking detailed");
            bool rv = Helper.ScrambledEquals(infieldInfoList.Select(x => x.Name).ToList(), otherFieldInfoList.Select(x => x.Name).ToList());
            Console.WriteLine("checking detailed " + rv);
          
            if (rv)//otherFieldInfoList.All(item => infieldInfoList.Contains(item)) || rv)
            {
                Console.WriteLine("Same Base");
                return EnumSchemaDiffType.SameBase;
                //same base
            }
            Console.WriteLine("Diff Base");

            return EnumSchemaDiffType.DiffBaseModels;
        }
        public EnumSchemaDiffType CompareTypeConfigDetailed(TypeConfig inTypeConfig, TypeConfig otherConfig)
        {
            //foreach ( var inTypeConfig in ListinTypeConfig)
            //   { 
            if (inTypeConfig == null || otherConfig == null) return EnumSchemaDiffType.None;
            bool same_base = false;
            bool same_models = false;
            if ((inTypeConfig.BaseClassFields.Count != otherConfig.BaseClassFields.Count)
                || (inTypeConfig.ModelInfoList.Count > 0 && otherConfig.ModelInfoList.Count > 0 && inTypeConfig.ModelInfoList.Count != otherConfig.ModelInfoList.Count))
            {
                Console.WriteLine("EnumSchemaDiffType.DiffBaseModels");
                //return EnumSchemaDiffType.DiffBaseModels;
            }
            bool rv = Helper.ScrambledEquals(otherConfig.BaseClassFields.Select(x => x.Name).ToList(), inTypeConfig.BaseClassFields.Select(x => x.Name).ToList());
            var t1 = inTypeConfig.BaseClassFields.Except(otherConfig.BaseClassFields).ToList();
            var t2 = otherConfig.BaseClassFields.Except(inTypeConfig.BaseClassFields).ToList();

            if ((inTypeConfig.BaseClassFields.All(item => otherConfig.BaseClassFields.Contains(item)) &&
                otherConfig.BaseClassFields.All(item => inTypeConfig.BaseClassFields.Contains(item))) || rv)
            {
                same_base = true;
                //same base
            }
            if (inTypeConfig.ModelInfoList.All(item => otherConfig.ModelInfoList.Contains(item)) &&
               otherConfig.ModelInfoList.All(item => inTypeConfig.ModelInfoList.Contains(item)))
            {
                same_models = true;
            }
            if (same_base && same_models)
            {
                return EnumSchemaDiffType.SameModelsBase;
            }
            else if (same_base)
            {
                return EnumSchemaDiffType.SameBase;
            }
            return EnumSchemaDiffType.DiffBaseModels;
           
        }
    }
   
}

