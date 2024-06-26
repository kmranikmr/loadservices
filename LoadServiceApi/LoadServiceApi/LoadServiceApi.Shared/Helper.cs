//using DataAnalyticsPlatform.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
//using Newtonsoft.Json.Schema.Generation;
using System.Xml.Serialization;

namespace LoadServiceApi.Shared
{
    public static class ObjectToDictionaryHelper

    {

        public static IDictionary<string, object> ToDictionary(this object source)

        {

            return source.ToDictionary<object>();

        }



        public static IDictionary<string, T> ToDictionary<T>(this object source)

        {

            if (source == null)

                ThrowExceptionWhenSourceArgumentIsNull();



            var dictionary = new Dictionary<string, T>();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))

                AddPropertyToDictionary<T>(property, source, dictionary);



            return dictionary;

        }



        private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
        {
            object value = property.GetValue(source);

            if (IsOfType<T>(value))
                dictionary.Add(property.Name, (T)value);
            else
                dictionary.Add(property.Name, default(T));

        }



        private static bool IsOfType<T>(object value)

        {

            return value is T;

        }



        private static void ThrowExceptionWhenSourceArgumentIsNull()

        {

            throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");

        }

    }
    public static class Helper
    {
        public static string GetJson(object record)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(record);

            return json;
        }

        public static string GetJsonSchema(Type type)
        {
            return "";
            //JSchemaGenerator jsonSchemaGenerator = new JSchemaGenerator();
            //JSchema schema = jsonSchemaGenerator.Generate(type);
            //schema.Title = type.Name;

            //return schema.ToString();
        }

        public static T DeserializeFromXmlString<T>(string xml) where T : new()
        {
            T xmlObject = new T();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(xml);
            xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }


        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}
