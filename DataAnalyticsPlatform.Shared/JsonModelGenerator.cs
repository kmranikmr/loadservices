using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xamasoft.JsonClassGenerator;
namespace DataAnalyticsPlatform.Shared
{
    public class JsonModelGenerator
    {
        private CodeDomProvider provider;
        private Dictionary<string, int> FieldNameExists;
        public JsonModelGenerator()
        {
            provider = CodeDomProvider.CreateProvider("C#");
            FieldNameExists = new Dictionary<string, int>();
        }

        public string CheckandGetName(string name)
        {
            if ( !provider.IsValidIdentifier(name))
            {
                return name + "0";
            }
            if ( FieldNameExists.ContainsKey(name))
            {
                FieldNameExists[name] = FieldNameExists[name] + 1;
                return name + FieldNameExists[name];
            }
            else
            {
                FieldNameExists.Add(name, 1);
            }
            return name;
        }
        public DataType GetTypeData(JsonProperty prop)
        {
            if (prop.Type == JsonObjectType.String)
                return DataType.String;
            else if (prop.Type == JsonObjectType.Boolean)
                return DataType.Boolean;
            if (prop.Type == JsonObjectType.Integer)
                return DataType.Int;
            if (prop.Type == JsonObjectType.Object)
                return DataType.Object;
            if (prop.Type == JsonObjectType.Number)
                return DataType.Double;

            return DataType.Object;
        }
        public DataType GetTypeData(JsonType type)
        {
            if (type.Type == JsonTypeEnum.String || type.Type == JsonTypeEnum.NullableSomething)
                return DataType.String;
            else if (type.Type == JsonTypeEnum.Date || type.Type == JsonTypeEnum.NullableDate)
                return DataType.DateTime;
            else if (type.Type == JsonTypeEnum.Boolean || type.Type == JsonTypeEnum.NullableBoolean)
                return DataType.Boolean;
            if (type.Type == JsonTypeEnum.Integer || type.Type == JsonTypeEnum.NullableInteger)
                return DataType.Int;
            if (type.Type == JsonTypeEnum.Object )
                return DataType.Object;
            if (type.Type == JsonTypeEnum.Float || type.Type == JsonTypeEnum.NullableFloat)
                return DataType.Double;
            if (type.Type == JsonTypeEnum.Array)
                return DataType.ObjectArray;

            return DataType.Object;
        }

        public string getDataTypeString(JsonType dataType, string Name)
        {
            Name = CheckandGetName(Name);
            string f = "";
            if (dataType.Type == JsonTypeEnum.Boolean)
                f = $"public bool {Name}" + "{get;set;}\n";
          
            else if (dataType.Type == JsonTypeEnum.Date)
                f = $"public System.DateTime {Name}" + "{get;set;}\n";
            else if (dataType.Type == JsonTypeEnum.Float)
                f = $"public double {Name}" + "{get;set;}\n";
            else if (dataType.Type == JsonTypeEnum.Integer)
                f = $"public int {Name}" + "{get;set;}\n";
            else if (dataType.Type == JsonTypeEnum.Long)
                f = $"public long {Name}" + "{get;set;}\n";
            else if (dataType.Type == JsonTypeEnum.String)
                f = $"public string {Name}" + "{get;set;}\n";

            else if (dataType.Type == JsonTypeEnum.Object)
            {
                f = $"public {Name}" + $" {Name}" + "{get;set;}\n";
            }
            return f;
        }
        public void GenerateFieldMetaData(JsonSchema4 currentSchema, List<FieldInfo> fieldInfo)
        {
            if (currentSchema.ActualProperties.Count > 0)
            {
                foreach (KeyValuePair<string, JsonProperty> kvp in currentSchema.ActualProperties)
                {
                    FieldInfo _fieldInfo = new FieldInfo(kvp.Key, GetTypeData(kvp.Value));

                    fieldInfo.Add(_fieldInfo);
                    if (kvp.Value.ActualSchema != null)
                    {
                        GenerateFieldMetaData(kvp.Value.ActualSchema, _fieldInfo.InnerFields);
                    }
                }

            }
        }
        public string getDataTypeString(DataType dataType, string Name, bool skipCheck = false)
        {
            if (!skipCheck)
            {
                Name = CheckandGetName(Name);
            }
            string f = "";
            if (dataType == DataType.Boolean)
                f = $"public bool {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Char)
                f = $"public char {Name}" + "{get;set;}\n";
            else if (dataType == DataType.DateTime)
                f = $"public System.DateTime {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Double)
                f = $"public double {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Int)
                f = $"public int {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Long)
                f = $"public long {Name}" + "{get;set;}\n";
            else if (dataType == DataType.String)
                f = $"public string {Name}" + "{get;set;}\n";
            else if (dataType == DataType.StringArray)
                f = $"public string[] {Name}" + "{get;set;}\n";
            else if (dataType == DataType.IntArray)
                f = $"public int[] {Name}" + "{get;set;}\n";
            else if (dataType == DataType.FloatArray)
                f = $"public float[] {Name}" + "{get;set;}\n";
            else if (dataType == DataType.ObjectArray)
                f = $"public {Name}[] {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Object)
            {
                f = $"public {Name}" + $" {Name}" + "{get;set;}\n";
            }
            return f;
        }
        public string GenerateClassfromFieldInfo(List<FieldInfo> FieldinfoList, List<string> classes, string prefix, string suffix)
        {
            string f = prefix;

            foreach (FieldInfo fieldInfo in FieldinfoList)
            {
                f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
                if (fieldInfo.DataType == DataType.Object)
                {
                    GenerateClassfromFieldInfo(fieldInfo.InnerFields, classes, "public partial class " + fieldInfo.Name + "{\n", "}\n");
                }
            }
            f += suffix;
            classes.Add(f);
            return "";
        }
        public string ClassGenerator(List<FieldInfo> fieldInfoList, ref string className, string schemaName, int jobId = 0 )
        {
            string Pf = "using System;\n";
            Pf += "namespace " + schemaName + " {\n";
            Pf += "public partial class OriginalRecord"+ jobId+"{\n";
            //gen base
            Pf += "public OriginalRecord"+jobId+"(){ Init();}\n";
            Pf += "partial void Init();\n";
            List<string> classes = new List<string>();
            GenerateClassfromFieldInfo(fieldInfoList, classes, Pf, "}}\n");
            //foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            //{
            //    f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
            //}
            //f += "}\n";
            string f = classes.Aggregate((i, j) => i + "\n" + j);

            className = "OriginalRecord"+jobId;
            return f;
        }

        public void GetFieldsFromType(List<FieldInfo> fieldInfo, List<string> classes, JsonType RootType, string prefix, string suffix)
        {
            
            
            if (RootType != null)
            {
                if (RootType.Fields != null)
                {
                    string f = prefix;
                    foreach (var field in RootType.Fields)
                    {
                        FieldInfo _fieldInfo = null;// = new FieldInfo(field.MemberName, GetTypeData(field.Type));
                         //f+= getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name);
                        // f += $"public {field.Type.GetTypeName()} "+  field.MemberName + " {get ; set;}\n";
                        if (field.Type.Type == JsonTypeEnum.Array)
                        {
                            if (field.Type.InternalType.Fields == null )//field.Type.Fields == null)
                            {
                                var type = GetTypeData((JsonType)(field.Type.InternalType));
                                if (type == DataType.String)
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.StringArray);
                                }
                                else if (type == DataType.Int)
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.IntArray);
                                }
                                else if (type == DataType.Double)
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.FloatArray);
                                }
                                else
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.ObjectArray);
                                }
                                f += getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name, true);
                            }
                            else
                            {
                                _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), GetTypeData(field.Type));
                                f += getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name, true);

                            }
                        }
                        else
                        {
                               _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), GetTypeData(field.Type));
                            f += getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name, true);
                        }
                        fieldInfo.Add(_fieldInfo);
                        //this.CheckandGetName
                        if (field.Type.Type == JsonTypeEnum.Array && field.Type.InternalType.Fields != null)
                        {
                            GetFieldsFromType(_fieldInfo.InnerFields, classes, field.Type.InternalType, "public partial class " + (_fieldInfo.Name )+ "{\n", "}\n");
                        }//this.CheckandGetName
                        else if (field.Type.Type == JsonTypeEnum.Object)
                        {
                            GetFieldsFromType(_fieldInfo.InnerFields, classes, field.Type, "public partial class " + (_fieldInfo.Name) + "{\n", "}\n");
                        }

                    }
                    f += suffix;
                    classes.Add(f);
                }
             
            }
          

        }

        public void GetFieldsFromType(FieldInfo fieldToRet, List<FieldInfo> fieldInfo, List<string> classes, JsonType RootType, string prefix, string suffix)
        {


            if (RootType != null)
            {
                if (RootType.Fields != null)
                {
                    string f = prefix;
                    foreach (var field in RootType.Fields)
                    {
                        FieldInfo _fieldInfo = null;// = new FieldInfo(field.MemberName, GetTypeData(field.Type));
                                                    //f+= getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name);
                                                    // f += $"public {field.Type.GetTypeName()} "+  field.MemberName + " {get ; set;}\n";
                        if (field.Type.Type == JsonTypeEnum.Array)
                        {
                            if (field.Type.InternalType.Fields == null)//field.Type.Fields == null)
                            {
                                var type = GetTypeData((JsonType)(field.Type.InternalType));
                                if (type == DataType.String)
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.StringArray);
                                }
                                else if (type == DataType.Int)
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.IntArray);
                                }
                                else if (type == DataType.Double)
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.FloatArray);
                                }
                                else
                                {
                                    _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), DataType.ObjectArray);
                                }
                                f += getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name, true);
                            }
                            else
                            {
                                _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), GetTypeData(field.Type));
                                f += getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name, true);

                            }
                        }
                        else
                        {
                            _fieldInfo = new FieldInfo(this.CheckandGetName(field.MemberName), GetTypeData(field.Type));
                            f += getDataTypeString(_fieldInfo.DataType, _fieldInfo.Name, true);
                        }
                        // fieldInfo.AddField(_fieldInfo);
                        fieldToRet.AddField(_fieldInfo);
                        //this.CheckandGetName
                        if (field.Type.Type == JsonTypeEnum.Array && field.Type.InternalType.Fields != null)
                        {
                            GetFieldsFromType(fieldToRet.InnerFields[fieldToRet.InnerFields.Count - 1], _fieldInfo.InnerFields, classes, field.Type.InternalType, "public partial class " + (_fieldInfo.Name) + "{\n", "}\n");
                        }//this.CheckandGetName
                        else if (field.Type.Type == JsonTypeEnum.Object)
                        {
                            GetFieldsFromType(fieldToRet.InnerFields[fieldToRet.InnerFields.Count - 1], _fieldInfo.InnerFields, classes, field.Type, "public partial class " + (_fieldInfo.Name) + "{\n", "}\n");
                        }

                    }
                    f += suffix;
                    classes.Add(f);
                }

            }


        }
        public List<FieldInfo> GetAllFieldsV2(string filePath, string readerConfiguration, ref string className, ref string Classstring, string schema, int jobId = 0)
        {
            if (File.Exists(filePath))
            {
                string csCode = "";
                List<FieldInfo> fieldInfoList = null;
                //get one from array
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (StreamReader sr = new StreamReader(fs))
                using (JsonTextReader reader1 = new JsonTextReader(sr))
                {
                    while (reader1.Read())
                    {
                        if (reader1.TokenType == JsonToken.StartObject)
                        {
                            JObject obj = JObject.Load(reader1);
                            csCode = Helper.CreateCsfromJson(obj.ToString());
                            fieldInfoList = JsonReaderHelper.GetFieldInfos(csCode);
                            Classstring = csCode;
                         
                           break;
                        }
                    }
                }

                return fieldInfoList;

            }
            return null;
        }
        public List<FieldInfo> GetAllFields(string filePath, string readerConfiguration , ref string className, ref string Classstring, string schema , int jobId = 0)
        {

            List<FieldInfo> listOfFieldInfo = new List<FieldInfo>();

            if (readerConfiguration != null &&  (readerConfiguration.ToLower().Contains("cord19") || readerConfiguration.ToLower().Contains("json1")))
            {
                TwitterObjectModelGenerator gen = new TwitterObjectModelGenerator();
                Cord19.OriginalRecord cordObj = new Cord19.OriginalRecord();
                listOfFieldInfo = gen.GetAllFieldsWithDeserilization(filePath, cordObj);//
               // string code = File.ReadAllText("Cord19Test.cs");
                Classstring = "";
                className = "OriginalRecord" + jobId;
                return listOfFieldInfo;
            }
            if (File.Exists(filePath))
            {
                String JSONtxt = File.ReadAllText(filePath);
                dynamic dynJson = JsonConvert.DeserializeObject(JSONtxt);
                foreach( var item in dynJson)
                {
                    var jgen = new JsonClassGenerator();
                    jgen.Example = Convert.ToString(dynJson);
                    jgen.MainClass = "OriginalRecord"+jobId;

                    jgen.TargetFolder = null;
                    jgen.SingleFile = true;
                    jgen.GetTypeGenerated = true;
                    jgen.Namespace = schema;
                    jgen.UseNestedClasses = true;
                 //   jgen.AlwaysUseNullableValues = true;
                    
                    var types = jgen.GenerateTypes();
                   
                    List<string> classes = new List<string>();
                    //List<FieldInfo> fieldInfo = new FieldInfo();
                    var orderedTypes = types.OrderByDescending(x => x.IsRoot).ToList();
                    string Pf = "namespace " + schema + "{\n";
                    Pf += "public partial class OriginalRecord"+jobId+" {\n";
                    //gen base
                    Pf += "public OriginalRecord"+jobId+"(){ Init();}\n";
                    Pf += "partial void Init();\n";
                    // GetFieldsFromType(listOfFieldInfo, classes, orderedTypes[0], Pf, "}}\n");
                    FieldInfo fieldInfo = new FieldInfo("", DataType.Object);
                    GetFieldsFromType(fieldInfo, listOfFieldInfo, classes, orderedTypes[0], Pf, "}}\n");
                    string f = classes.Aggregate((i, j) => i + "\n" + j);
                    listOfFieldInfo = fieldInfo.InnerFields;
                    Classstring = f;
                  
                    break;//only one for now
                }
                //using (var reader = new JsonTextReader(new StringReader(filePath)))
                //{
                //    while (reader.Read())
                //    {
                //        if (reader.TokenType == JsonToken.StartObject)
                //        {
                //            // Load each object from the stream and do something with it
                //            JObject obj = JObject.Load(reader);
                //            var jSchema = JsonSchema4.FromSampleJson((string)obj.);
                //            GenerateFieldMetaData(jSchema, listOfFieldInfo);
                //        }

                //    }
                //}
                // String JSONtxt = File.ReadAllText(filePath);

                //Capture JSON string for each object, including curly brackets 
                //    Regex regex = new Regex(@".*(?<=\{)[^}]*(?=\}).*", RegexOptions.IgnoreCase);
                //   MatchCollection matches = regex.Matches(JSONtxt);
                //foreach (Match match in matches)
                {
                   // string objStr = JSONtxt.ToString();
                    //json lets get schema
                   
                   // break;
                }
               
            }
            className = "OriginalRecord"+jobId;
            return listOfFieldInfo;
        }
    }
}
