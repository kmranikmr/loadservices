using DataAnalyticsPlatform.Common.Helpers;
using DataAnalyticsPlatform.Shared.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataAnalyticsPlatform.SharedUtils;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using DataAnalyticsPlatform.Shared;
using Microsoft.CSharp;

namespace DataAnalyticsPlatform.Common.Builders
{
    public class TypeConfigBuilder
    {
        private readonly CodeGenHelper _codeGenHelper;
        private readonly ModelCompiler _modelCompiler;

        public TypeConfigBuilder(CodeGenHelper codeGenHelper, ModelCompiler modelCompiler)
        {
            _codeGenHelper = codeGenHelper;
            _modelCompiler = modelCompiler;
        }

        public TypeConfig BuildTypeConfig(string[] headers, Type dataType = null, string schemaName = "DataModel")
        {
            TypeConfig typeConfig = new TypeConfig
            {
                SchemaName = schemaName,
                BaseClassFields = new List<FieldInfo>(),
                ModelInfoList = new List<ModelInfo>()
            };

            if (headers == null || headers.Length == 0)
            {
                return typeConfig;
            }

            // Create base class fields from headers
            foreach (string header in headers)
            {
                string cleanHeader = CleanHeaderName(header);
                DataType fieldType = DetermineDataType(header);

                typeConfig.BaseClassFields.Add(new FieldInfo
                {
                    Name = cleanHeader,
                    DisplayName = header,
                    DataType = fieldType,
                    Map = cleanHeader
                });
            }

            // Create default model
            ModelInfo defaultModel = new ModelInfo
            {
                ModelName = "DefaultModel",
                ModelFields = new List<FieldInfo>()
            };

            // Add fields to default model
            foreach (FieldInfo baseField in typeConfig.BaseClassFields)
            {
                defaultModel.ModelFields.Add(new FieldInfo
                {
                    Name = baseField.Name,
                    DisplayName = baseField.DisplayName,
                    DataType = baseField.DataType,
                    Map = baseField.Name
                });
            }

            typeConfig.ModelInfoList.Add(defaultModel);

            return typeConfig;
        }

        public TypeConfig BuildJsonTypeConfig(object jsonObject, string schemaName = "JsonModel")
        {
            TypeConfig typeConfig = new TypeConfig
            {
                SchemaName = schemaName,
                BaseClassFields = new List<FieldInfo>(),
                ModelInfoList = new List<ModelInfo>()
            };

            if (jsonObject == null)
            {
                return typeConfig;
            }

            // Extract properties from JSON object and build a type config
            Dictionary<string, FieldInfo> baseFields = new Dictionary<string, FieldInfo>();
            ProcessJsonObject(jsonObject, "", baseFields);

            // Add fields to type config
            typeConfig.BaseClassFields = baseFields.Values.ToList();

            // Create default model
            ModelInfo defaultModel = new ModelInfo
            {
                ModelName = "JsonModel",
                ModelFields = new List<FieldInfo>()
            };

            // Map JSON fields to model
            foreach (var field in baseFields.Values)
            {
                FieldInfo modelField = new FieldInfo
                {
                    Name = field.Name,
                    DisplayName = field.DisplayName,
                    DataType = field.DataType,
                    Map = field.Name,
                    InnerFields = field.InnerFields
                };

                defaultModel.ModelFields.Add(modelField);
            }

            typeConfig.ModelInfoList.Add(defaultModel);

            return typeConfig;
        }

        private void ProcessJsonObject(object jsonObject, string path, Dictionary<string, FieldInfo> fields)
        {
            if (jsonObject == null)
                return;

            // Process object properties
            var properties = jsonObject.GetType().GetProperties();
            foreach (var prop in properties)
            {
                string propName = prop.Name;
                string propPath = string.IsNullOrEmpty(path) ? propName : $"{path}.{propName}";
                object value = prop.GetValue(jsonObject);

                if (value == null)
                    continue;

                Type propType = value.GetType();

                // Handle arrays
                if (propType.IsArray)
                {
                    Array array = value as Array;
                    if (array.Length > 0)
                    {
                        object firstItem = array.GetValue(0);
                        Type elementType = firstItem.GetType();

                        if (IsSimpleType(elementType))
                        {
                            // Simple array type
                            DataType arrayType = GetDataTypeFromType(elementType, true);
                            AddField(fields, propName, propPath, arrayType);
                        }
                        else
                        {
                            // Complex array type - create a field for the array and process its elements
                            DataType arrayType = DataType.ObjectArray;
                            FieldInfo arrayField = AddField(fields, propName, propPath, arrayType);

                            // Process the first item to get the structure
                            ProcessJsonObject(firstItem, $"{propPath}[]", fields);
                        }
                    }
                }
                // Handle collections
                else if (propType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
                {
                    // Similar logic as arrays
                    var collection = value as System.Collections.ICollection;
                    if (collection.Count > 0)
                    {
                        var enumerator = collection.GetEnumerator();
                        enumerator.MoveNext();
                        object firstItem = enumerator.Current;
                        Type elementType = firstItem.GetType();

                        if (IsSimpleType(elementType))
                        {
                            DataType collectionType = GetDataTypeFromType(elementType, true);
                            AddField(fields, propName, propPath, collectionType);
                        }
                        else
                        {
                            DataType collectionType = DataType.ObjectArray;
                            FieldInfo collectionField = AddField(fields, propName, propPath, collectionType);

                            ProcessJsonObject(firstItem, $"{propPath}[]", fields);
                        }
                    }
                }
                // Handle simple types
                else if (IsSimpleType(propType))
                {
                    DataType dataType = GetDataTypeFromType(propType);
                    AddField(fields, propName, propPath, dataType);
                }
                // Handle complex objects
                else
                {
                    DataType objectType = DataType.Object;
                    FieldInfo objectField = AddField(fields, propName, propPath, objectType);

                    // Process nested object
                    ProcessJsonObject(value, propPath, fields);
                }
            }
        }

        private FieldInfo AddField(Dictionary<string, FieldInfo> fields, string name, string path, DataType dataType)
        {
            string cleanName = CleanHeaderName(name);
            
            FieldInfo field = new FieldInfo
            {
                Name = cleanName,
                DisplayName = name,
                DataType = dataType,
                Map = path,
                InnerFields = new List<FieldInfo>()
            };

            if (!fields.ContainsKey(path))
            {
                fields.Add(path, field);
            }

            return field;
        }

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || 
                   type == typeof(string) || 
                   type == typeof(decimal) || 
                   type == typeof(DateTime) || 
                   type == typeof(DateTimeOffset) || 
                   type == typeof(TimeSpan) || 
                   type == typeof(Guid) || 
                   type.IsEnum;
        }

        private DataType GetDataTypeFromType(Type type, bool isArray = false)
        {
            if (isArray)
            {
                if (type == typeof(string))
                    return DataType.StringArray;
                else if (type == typeof(int) || type == typeof(long) || type == typeof(short))
                    return DataType.IntArray;
                else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                    return DataType.FloatArray;
                else
                    return DataType.ObjectArray;
            }
            else
            {
                if (type == typeof(bool))
                    return DataType.Boolean;
                else if (type == typeof(char))
                    return DataType.Char;
                else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
                    return DataType.DateTime;
                else if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
                    return DataType.Double;
                else if (type == typeof(int) || type == typeof(short))
                    return DataType.Int;
                else if (type == typeof(long))
                    return DataType.Long;
                else if (type == typeof(string))
                    return DataType.String;
                else
                    return DataType.Object;
            }
        }

        private string CleanHeaderName(string header)
        {
            // Clean header name to make it a valid C# identifier
            string cleanName = Regex.Replace(header, @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
            cleanName = new string(cleanName.Where(c => Char.IsLetterOrDigit(c) || c == '_').ToArray());
            
            if (string.IsNullOrEmpty(cleanName) || !char.IsLetter(cleanName[0]) && cleanName[0] != '_')
            {
                cleanName = "_" + cleanName;
            }
            
            return _codeGenHelper.CheckAndGetName(cleanName);
        }

        private DataType DetermineDataType(string value)
        {
            // Try to infer data type from value
            if (_codeGenHelper.CheckDateTime(value))
            {
                return DataType.DateTime;
            }
            
            if (bool.TryParse(value, out _))
            {
                return DataType.Boolean;
            }
            
            if (int.TryParse(value, out _))
            {
                return DataType.Int;
            }
            
            if (long.TryParse(value, out _))
            {
                return DataType.Long;
            }
            
            if (double.TryParse(value, out _))
            {
                return DataType.Double;
            }
            
            // Default to string if we can't determine type
            return DataType.String;
        }

        public TypeConfig MergeTypeConfigs(TypeConfig config1, TypeConfig config2)
        {
            TypeConfig mergedConfig = new TypeConfig
            {
                SchemaName = config1.SchemaName,
                BaseClassFields = new List<FieldInfo>(config1.BaseClassFields),
                ModelInfoList = new List<ModelInfo>(config1.ModelInfoList)
            };
            
            // Merge base class fields
            foreach (var field in config2.BaseClassFields)
            {
                if (!mergedConfig.BaseClassFields.Any(f => f.Name == field.Name))
                {
                    mergedConfig.BaseClassFields.Add(field);
                }
            }
            
            // Merge model info lists
            foreach (var model in config2.ModelInfoList)
            {
                var existingModel = mergedConfig.ModelInfoList.FirstOrDefault(m => m.ModelName == model.ModelName);
                
                if (existingModel != null)
                {
                    // Merge fields in existing model
                    foreach (var field in model.ModelFields)
                    {
                        if (!existingModel.ModelFields.Any(f => f.Name == field.Name))
                        {
                            existingModel.ModelFields.Add(field);
                        }
                    }
                }
                else
                {
                    // Add new model
                    mergedConfig.ModelInfoList.Add(model);
                }
            }
            
            return mergedConfig;
        }
        
        public List<Type> Code(TypeConfig typeConfig, int jobId = 0, string fileName = "")
        {
            List<string> classNames = new List<string>();
            string code = "";
            string f = "using System; using System.Collections.Generic; using CsvHelper.Configuration; using System.Linq; using System.Text.RegularExpressions; using DataAnalyticsPlatform.Shared.DataAccess;\n namespace DataAnalyticsPlatform.Common{";
            
            f += "public partial class OriginalRecord{\n";
            
            // Generate base class
            f += "static int rows = 1;";
            f += "public OriginalRecord(){ Init(); rowid = rows++;}\n";
            f += "partial void Init();\n";
            
            bool hasRowid = false;
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                f += _codeGenHelper.GetDataTypeString(fieldInfo.DataType, fieldInfo.Name);
                if (fieldInfo.Name.Contains("rowid"))
                {
                    hasRowid = true;
                }
            }
            
            if (!hasRowid)
            {
                f += "public int rowid{get;set;}\n";
            }
            f += "public int fileid{get;set;}\n";
            f += "public long sessionid{get;set;}\n";
            f += "public string FileName{get;set;}\n";
            f += "}\n";
            code = f;
            
            // Generate model classes
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelName = modelInfo.ModelName.Replace("-", "");
                f = String.Format($"public partial class { modelName } : BaseModel");
                f += "{\n";
                
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    bool dateBool = _codeGenHelper.CheckDateTime(fieldInfo.Name);
                    string columnName = "";
                    
                    if (dateBool)
                    {
                        columnName = "day" + fieldInfo.Name.Replace("/", "_");
                    }
                    else
                    {
                        columnName = Regex.Replace(fieldInfo.Name, @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
                        columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());
                    }
                    
                    columnName = _codeGenHelper.CheckAndGetName(columnName);
                    
                    string fieldNameTrim = columnName;
                    f += _codeGenHelper.GetDataTypeString(fieldInfo.DataType, fieldNameTrim);
                }
                
                if (!hasRowid)
                {
                    f += "public int rowid{get;set;}\n";
                }
                f += "public int fileid{get;set;}\n";
                f += "public long sessionid{get;set;}\n";
                f += "public string FileName{get;set;}\n";
                f += "}\n";
                code += f;
                f = "";
            }
            
            // Generate mapper class
            code += @"public class Mappers : ClassMap<OriginalRecord>{  public Mappers(){ ";
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                string classNameField = fieldInfo.Name.Replace("-", "");
                string fieldNameTrim = fieldInfo.DisplayName.Replace("\"", "");
                code += $"Map(m => m.{classNameField}).Name(\"{fieldNameTrim}\");\n";
            }
            code += "AutoMap(); }    }";
            code += "}\n";
            
            // Generate additional code with CodeDom
            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");
            ns.Imports.AddRange(new CodeNamespaceImport[]
            {
                new CodeNamespaceImport("System"),
                new CodeNamespaceImport("System.IO"),
                new CodeNamespaceImport("System.Collections.Generic"),
                new CodeNamespaceImport("System.Linq"),
                new CodeNamespaceImport("System.Text.RegularExpressions"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Common"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Shared.DataModels"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Shared.DataAccess")
            });
            
            var myclass = new CodeTypeDeclaration("OriginalRecord");
            myclass.IsClass = true;
            myclass.IsPartial = true;
            
            // Add methods to the class
            GenerateCSVModelMethods(myclass, typeConfig, hasRowid, jobId, fileName);
            
            ns.Types.Add(myclass);
            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            
            var provider = new CSharpCodeProvider();
            StringWriter sw = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions());
            
            code += sw.ToString();
            
            // Compile the code
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(code, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.ModelGen");
            
            if (types == null)
                return null;
            else
                return types.ToList();
        }
        
        private void GenerateCSVModelMethods(CodeTypeDeclaration myclass, TypeConfig typeConfig, bool hasRowid, int jobId, string fileName)
        {
            // Add model fields
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelName = modelInfo.ModelName.Replace("-", "");
                CodeMemberField field = new CodeMemberField(modelName, modelName);
                field.Attributes = MemberAttributes.Public;
                myclass.Members.Add(field);
            }
            
            // Add models collection
            var modelsField = new CodeMemberField("List<BaseModel>", "models");
            modelsField.Attributes = MemberAttributes.Public;
            myclass.Members.Add(modelsField);
            
            // Add Init method
            var initMethod = new CodeMemberMethod
            {
                Name = "Init",
                ReturnType = new CodeTypeReference("partial void"),
                Attributes = MemberAttributes.Final
            };
            
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelName = modelInfo.ModelName.Replace("-", "");
                initMethod.Statements.Add(new CodeSnippetStatement($"{modelName} = new {modelName}();"));
            }
            
            initMethod.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            myclass.Members.Add(initMethod);
            
            // Add MapIt method
            var mapItMethod = new CodeMemberMethod
            {
                Name = "MapIt",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            // Add SetProps method
            var setPropsMethod = new CodeMemberMethod
            {
                Name = "SetProps",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            // Add SetValues method
            var setValuesMethod = new CodeMemberMethod
            {
                Name = "SetValues",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelName = modelInfo.ModelName.Replace("-", "");
                
                // MapIt implementation
                mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.ModelName = \"{modelName}\";"));
                
                // SetProps implementation
                setPropsMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.Props = new string[] {{"));
                
                // SetValues implementation
                setValuesMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.Values = new List<object>() {{"));
                
                int commaStart = 0;
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    bool dateBool = _codeGenHelper.CheckDateTime(fieldInfo.Name);
                    string columnName = dateBool
                        ? "day" + fieldInfo.Name.Replace("/", "_")
                        : Regex.Replace(fieldInfo.Name, @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
                    
                    columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());
                    columnName = _codeGenHelper.CheckAndGetName(columnName);
                    
                    // Add property mapping to MapIt
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.{columnName} = {fieldInfo.Map};"));
                    
                    // Add to props and values
                    if (commaStart > 0)
                    {
                        setValuesMethod.Statements.Add(new CodeSnippetStatement(","));
                        setPropsMethod.Statements.Add(new CodeSnippetStatement(","));
                    }
                    
                    setValuesMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.{columnName}"));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement($"\"{columnName}\""));
                    
                    commaStart++;
                }
                
                // Add standard fields
                if (!hasRowid)
                {
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.rowid = rowid;"));
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.sessionid = {jobId};"));
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.FileName = \"{fileName}\";"));
                    
                    if (commaStart > 0)
                    {
                        setValuesMethod.Statements.Add(new CodeSnippetStatement(","));
                        setPropsMethod.Statements.Add(new CodeSnippetStatement(","));
                    }
                    
                    setValuesMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.rowid"));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement("\"rowid\""));
                    
                    setValuesMethod.Statements.Add(new CodeSnippetStatement(","));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement(","));
                    setValuesMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.sessionid"));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement("\"sessionid\""));
                    
                    setValuesMethod.Statements.Add(new CodeSnippetStatement(","));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement(","));
                    setValuesMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.FileName"));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement("\"FileName\""));
                }
                
                // Close arrays
                setValuesMethod.Statements.Add(new CodeSnippetStatement("}.ToArray();"));
                setPropsMethod.Statements.Add(new CodeSnippetStatement("};"));
                
                // Add model to models list
                mapItMethod.Statements.Add(new CodeSnippetStatement($"models.Add({modelName});"));
            }
            
            // Call props and values setup from MapIt
            mapItMethod.Statements.Add(new CodeSnippetStatement("SetProps();"));
            mapItMethod.Statements.Add(new CodeSnippetStatement("SetValues();"));
            
            // Add GetModels method
            var getModelsMethod = new CodeMemberMethod
            {
                Name = "GetModels",
                ReturnType = new CodeTypeReference(typeof(List<BaseModel>)),
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            getModelsMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "file_id"));
            getModelsMethod.Statements.Add(new CodeSnippetStatement("fileid = file_id;"));
            getModelsMethod.Statements.Add(new CodeSnippetStatement("return models;"));
            
            // Add methods to class
            myclass.Members.Add(mapItMethod);
            myclass.Members.Add(setPropsMethod);
            myclass.Members.Add(setValuesMethod);
            myclass.Members.Add(getModelsMethod);
        }
    }
}
