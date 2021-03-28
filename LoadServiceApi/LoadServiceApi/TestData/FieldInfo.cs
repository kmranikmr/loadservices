﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace LoadServiceApi.Shared
{
    public class FieldInfo
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public DataType DataType { get; set; }

        public int Length { get; set; }

        public string Map { get; set; }
        public string  Uuid { get; set; }

        public List<FieldInfo> InnerFields { get; set; }

        public override string ToString()
        {
            return $@"{Name} - {DataType} ({Length})";
        }

        public FieldInfo()
        {
            Uuid = Guid.NewGuid().ToString();
        }

        public FieldInfo(string name, DataType t) : this()
        {
            DataType = t;

            Name = name;

            Map = name;

            if (t == DataType.Object || t == DataType.ObjectArray)
            {
                InnerFields = new List<FieldInfo>();
            }
        }

        public void AddField(FieldInfo f)
        {
            if (DataType == DataType.Object || DataType == DataType.ObjectArray)
            {
                if (InnerFields == null) InnerFields = new List<FieldInfo>();

                f.Map = Map + "." + f.Name;

                InnerFields.Add(f);
            }
            else
            {
                throw new Exception("Fields can be added only on object data type.");
            }
        }


        public FieldInfo(FieldInfo d)
        {
            Name = d.Name;
            DataType = d.DataType;
            Length = d.Length;
            Map = d.Map;
            if (d.InnerFields != null && d.InnerFields.Count > 0)
            {
                InnerFields = new List<FieldInfo>();
                foreach (var item in d.InnerFields)
                {
                    InnerFields.Add(new FieldInfo(item));
                }
            }
        }

        public void Update(FieldInfo d)
        {
            Name = d.Name;
            DataType = d.DataType;
            Length = d.Length;
            Map = d.Map;
            if (d.InnerFields != null && d.InnerFields.Count > 0)
            {
                InnerFields = new List<FieldInfo>();
                foreach (var item in d.InnerFields)
                {
                    InnerFields.Add(new FieldInfo(item));
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FieldInfo))
                return false;

            var other = obj as FieldInfo;

            if ((DataType != other.DataType) || (Length != other.Length) || (Map != other.Map) || (Name != other.Name))
                return false;

            return true;
        }



        public static bool operator ==(FieldInfo x, FieldInfo y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(FieldInfo x, FieldInfo y)
        {
            return !(x == y);
        }
      
        internal static DataType GetFieldType(string dataType)
        {
            switch (dataType)
            {
                case "DateTime":
                    return DataType.DateTime;
                case "int":
                    return DataType.Int;
                case "double":
                    return DataType.Double;
                case "string":
                    return DataType.String;

            }

            return DataType.Object;
        }
    }

    public class TypeConfig
    {
        public string SchemaName { get; set; }

        public int SchemaId { get; set; }

        public List<int> AssociatedFileId { get; set; }
       
        public List<FieldInfo> BaseClassFields { get; set; }

        public List<ModelInfo> ModelInfoList { get; set; }

        public TypeConfig()
        {
            BaseClassFields = new List<FieldInfo>();
            ModelInfoList = new List<ModelInfo>();
        }
        public TypeConfig(TypeConfig type)
        {
            BaseClassFields = type.BaseClassFields;
            ModelInfoList = type.ModelInfoList;
        }
    }

    public class ModelInfo
    {
        public string ModelName { get; set; }

        public int ModelId { get; set; }
        public List<FieldInfo> ModelFields { get; set; }

        public ModelInfo()
        {
            ModelFields = new List<FieldInfo>();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ModelInfo))
                return false;

            var other = obj as ModelInfo;

            if ((ModelName != other.ModelName) || (!ModelFields.All(item => other.ModelFields.Contains(item)) )) 
                return false;

            return true;
        }

        public static bool operator ==(ModelInfo x, ModelInfo y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(ModelInfo x, ModelInfo y)
        {
            return !(x == y);
        }
    }

    public enum DataType
    {
        String,
        Int,
        Double,
        Boolean,
        Long,
        Char,
        DateTime,
        Object,
        ObjectArray,
        StringArray,
        IntArray,
        FloatArray
    }
}