
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;

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
        public List<Type> AllTypes { get; set;}
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

        public enum EnumSchemaDiffType
        {
            None,
            SameModelsBase,
            SameBase,
            DiffBaseModels

        }
        public class FieldInfoComparer : IEqualityComparer<FieldInfo>
        {
            //public int Compare(FieldInfo x, FieldInfo y)
            //{
            //    if (x == null)
            //    {
            //        if (y == null)
            //        {
            //            return 0;
            //        }
            //        else
            //        {
            //            return -1;
            //        }
            //    }
            //    else
            //    {
            //        if (y == null)
            //        {
            //            return 1;
            //        }
            //        else
            //        {
            //            int retVal = x.Name.CompareTo(y.Name);

            //            return retVal;
            //        }
            //    }
            //}

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
            if ( infieldInfoList.Count != otherFieldInfoList.Count )
            {
                Console.WriteLine("EnumSchemaDiffType.DiffBaseModels");
               // return EnumSchemaDiffType.DiffBaseModels;
            }
             Console.WriteLine("checking detailed");
            bool rv = Helper.ScrambledEquals(infieldInfoList.Select(x=>x.Name).ToList(), otherFieldInfoList.Select(x=>x.Name).ToList());
            Console.WriteLine("checking detailed " + rv); 
     //       for ( int j = 0; j < infieldInfoList.Count; j++)
     //       {
     //            Console.WriteLine(infieldInfoList[j].Name + " " + otherFieldInfoList[j].Name + " " + 
     //                     infieldInfoList[j].DisplayName + " " + otherFieldInfoList[j].DisplayName);
	    //}

            // if ((infieldInfoList.All(item => otherFieldInfoList.Contains(item)) &&
            //  otherFieldInfoList.All(item => infieldInfoList.Contains(item))))
            //var intersected = otherFieldInfoList.Intersect(infieldInfoList, new FieldInfoComparer());
            //intersected.Count() == otherFieldInfoList.Count ||
            if ( rv)//otherFieldInfoList.All(item => infieldInfoList.Contains(item)) || rv)
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
                || (inTypeConfig.ModelInfoList.Count > 0 && otherConfig.ModelInfoList.Count > 0  && inTypeConfig.ModelInfoList.Count != otherConfig.ModelInfoList.Count))
            {
                Console.WriteLine("EnumSchemaDiffType.DiffBaseModels");
                //return EnumSchemaDiffType.DiffBaseModels;
            }
            bool rv = Helper.ScrambledEquals(otherConfig.BaseClassFields.Select(x => x.Name).ToList(), inTypeConfig.BaseClassFields.Select(x => x.Name).ToList());
            var t1 = inTypeConfig.BaseClassFields.Except(otherConfig.BaseClassFields).ToList();
            var t2 = otherConfig.BaseClassFields.Except(inTypeConfig.BaseClassFields).ToList();

            if ((inTypeConfig.BaseClassFields.All(item => otherConfig.BaseClassFields.Contains(item)) &&
                otherConfig.BaseClassFields.All(item => inTypeConfig.BaseClassFields.Contains(item)))  || rv )
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
            // }
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

