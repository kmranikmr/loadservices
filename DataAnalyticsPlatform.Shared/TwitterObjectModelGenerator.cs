using AutoMapper;
using Bogus;
using DataAnalyticsPlatform.Shared.Models;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DataAnalyticsPlatform.Shared
{
    public class TwitterObjectModelGenerator
    {
        public TwitterObjectModelGenerator()
        {

        }

        public List<FieldInfo> GetAllFields(string filePath, LinqToTwitter.Status statusData)
        {
            List<FieldInfo> listOfFieldInfo = new List<FieldInfo>();
            if (statusData == null) return null;
            FieldInfo rootField = new FieldInfo("", DataType.Object);
            GetFieldsFromType(rootField, statusData);
            listOfFieldInfo = rootField.InnerFields;
            return listOfFieldInfo;
        }

        public List<FieldInfo> GetAllFields(string filePath, object org )
        {
            List<FieldInfo> listOfFieldInfo = new List<FieldInfo>();
            //  string code = File.ReadAllText("TwitterOriginal.cs");
            FieldInfo fieldInfo = new FieldInfo("", DataType.Object);
            GetFieldsFromType(fieldInfo, org);
            listOfFieldInfo = fieldInfo.InnerFields;
            return listOfFieldInfo;
        }
        public List<FieldInfo> GetAllFieldsWithDeserilization(string filePath, object org)
        {
            List<FieldInfo> listOfFieldInfo = new List<FieldInfo>();
            if (File.Exists(filePath))
            {
                String JSONtxt = File.ReadAllText(filePath);
                JsonSerializerSettings setting = new JsonSerializerSettings();
                var data = new Faker<Cord19.OriginalRecord>();
                data.StrictMode(false);
                var test = data.Generate();
                var obj = JsonConvert.DeserializeObject<Cord19.OriginalRecord>(JSONtxt, setting);
                FieldInfo rootField = new FieldInfo("", DataType.Object);
                GetFieldsFromType(rootField, obj);
                listOfFieldInfo = rootField.InnerFields;
                return listOfFieldInfo;
            }
            return null;
        }
        public static DataType GetDataTypeFromProp(Type type, bool isArray = false)
        {
            if (type == typeof(string))
            {
                if (isArray)
                    return DataType.StringArray;
                else
                    return DataType.String;
            }
            else if (type == typeof(System.String))
            {
                if (isArray)
                    return DataType.StringArray;
                else
                    return DataType.String;
            }
            else if (type == typeof(System.Int32))
            {
                return DataType.Int;
            }
            else if (type == typeof(System.UInt32))
            {
                return DataType.Int;
            }
            else if (type == typeof(System.UInt32))
            {
                return DataType.Int;
            }
            else if (type == typeof(System.UInt64))
            {
                return DataType.Long;
            }
            else if (type == typeof(System.Int64))
            {
                return DataType.Long;
            }
            else if (type == typeof(int))
            {
                return DataType.Int;
            }
            else if (type == typeof(long))
            {
                return DataType.Long;
            }
            else if (type == typeof(double))
            {
                return DataType.Double;
            }
            else if (type == typeof(System.Double))
            {
                return DataType.Double;
            }
            else if (type == typeof(System.Single))
            {
                return DataType.Double;
            }
            else if (type == typeof(float))
            {
                return DataType.Double;
            }
            else if (type == typeof(Int16))
            {
                return DataType.Int;
            }
            else if (type == typeof(bool))
            {
                return DataType.Boolean;
            }
            else if (type == typeof(System.Boolean))
            {
                return DataType.Boolean;
            }
            else if (type == typeof(DateTime))
            {
                return DataType.DateTime;
            }
            else if ( type == typeof (System.String[]))
            {
                return DataType.StringArray;
            }
            else if ( type == typeof(IDictionary))
            {
                return DataType.Dict;
            }
            else if (type == typeof(Dictionary<,>))
            {
                return DataType.Dict;
            }
            else
            {

                if (isArray)
                    return DataType.ObjectArray;
                else
                return DataType.Object;
            }
        }

        //public void GetFieldsFromType(List<FieldInfo> fieldInfos, object propValue)
        //{
        //    if (propValue == null)
        //        return;

        //    if (fieldInfos == null) return;
        //    Type objType = propValue.GetType();
        //    var childProps = propValue.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    foreach (var prop in childProps)
        //    {
        //        var tt = prop.PropertyType;
        //        FieldInfo _fieldInfo = null;
        //        var name = prop.Name;
        //        object value = null;
        //        if (tt.IsArray)
        //        {
        //            var proparray = (object[])prop.GetValue(propValue, null);
        //            if (proparray.Length > 0)
        //            {
        //                _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(proparray[0].GetType(), true));
        //                fieldInfos.Add(_fieldInfo);
        //                GetFieldsFromType(_fieldInfo.InnerFields, proparray[0]);
        //            }
        //            else
        //            {
        //                var ty = proparray.GetType();

        //                _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(proparray.GetType()));
        //                fieldInfos.Add(_fieldInfo);

        //                //GetFieldsFromType(_fieldInfo.InnerFields, value);
        //            }
        //        }
        //        else
        //        {
        //            value = prop.GetValue(propValue, null);
        //        }
        //        if (value == null || name == "Length") continue;
        //        var elems = propValue as IList;
        //        if (elems != null)
        //        {
        //            _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(value.GetType()));
        //            fieldInfos.Add(_fieldInfo);
        //            foreach (var item in elems)
        //            {

        //                GetFieldsFromType(_fieldInfo.InnerFields, item);
        //            }
        //        }
        //        else
        //        {
        //            if (prop.PropertyType.Assembly == objType.Assembly)
        //            {
        //                _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(value.GetType()));
        //                fieldInfos.Add(_fieldInfo);
        //                GetFieldsFromType(_fieldInfo.InnerFields, value);
        //            }
        //            else
        //            {
        //                if (tt.IsArray)
        //                {
        //                    if (value != null)
        //                    {
        //                        _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(tt.GetElementType()));
        //                        fieldInfos.Add(_fieldInfo);
        //                    }
        //                }
        //                else
        //                {
        //                    if (value != null)
        //                    {
        //                        _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(value.GetType()));
        //                        fieldInfos.Add(_fieldInfo);
        //                    }
        //                }
        //            }
        //        }

        //    }
        //}

        public void GetFieldsFromType( FieldInfo fieldInfo , object propValue)
        {
            if (propValue == null)
                return;

            if (fieldInfo == null) return;
            Type objType = propValue.GetType();
            var childProps = propValue.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !p.GetIndexParameters().Any()); ;
            foreach (var prop in childProps)
            {
                var tt = prop.PropertyType;
                FieldInfo _fieldInfo = null;
                var name = prop.Name;
                object value = null;
                if (tt.IsArray)
                {
                    var proparray = (object[])prop.GetValue(propValue, null);
                    if (proparray.Length > 0)
                    {
                        _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(proparray[0].GetType(), true));
                        fieldInfo.AddField(_fieldInfo);
                        GetFieldsFromType(_fieldInfo, proparray[0]);
                        //if (fieldInfo.InnerFields != null)
                        //{
                        //    fieldInfo.InnerFields.Add(_fieldInfo);
                        //    GetFieldsFromType(_fieldInfo.InnerFields, proparray[0]);
                        //}
                    }
                    else
                    {
                        var ty = proparray.GetType();
                       
                        _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(proparray.GetType()));
                        //fieldInfos.Add(_fieldInfo);
                        fieldInfo.AddField(_fieldInfo);
                        //GetFieldsFromType(_fieldInfo.InnerFields, value);
                    }
                }
                else
                {
                   
                    value = prop.GetValue(propValue);
                }
                if (value == null || name == "Length") continue;
                var elems = propValue as IList;
                if (elems != null)
                {
                    DataType t = GetDataTypeFromProp(value.GetType());
                    _fieldInfo = new FieldInfo(name, t);
                    //fieldInfos.Add(_fieldInfo);
                    if (t == DataType.Object || t == DataType.ObjectArray)
                    {
                        fieldInfo.AddField(_fieldInfo);
                    }
                    
                    foreach (var item in elems)
                    {
                        GetFieldsFromType( _fieldInfo, item);
                    }
                }
                else
                {
                    if (prop.PropertyType.Assembly == objType.Assembly)
                    {
                        var t = GetDataTypeFromProp(value.GetType());
                        _fieldInfo = new FieldInfo(name, t);
                        //if (t == DataType.Object || t == DataType.ObjectArray)
                        {
                            fieldInfo.AddField(_fieldInfo);
                        }
                        GetFieldsFromType(_fieldInfo, value);
                    }
                    else
                    {
                        if (tt.IsArray)
                        {
                            if (value != null)
                            {
                                var t = GetDataTypeFromProp(tt.GetElementType());
                                _fieldInfo = new FieldInfo(name, t);
                                fieldInfo.AddField(_fieldInfo);
                            }
                        }
                        else
                        {
                            if (value != null)
                            {
                                _fieldInfo = new FieldInfo(name, GetDataTypeFromProp(value.GetType()));
                                fieldInfo.AddField(_fieldInfo);
                            }
                        }
                    }
                }

            }
        }
    }
}
