using DataAnalyticsPlatform.SharedUtils;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataAnalyticsPlatform.Shared.DataAccess;
using Microsoft.CSharp;
using DataAnalyticsPlatform.Shared;
namespace DataAnalyticsPlatform.Common.Helpers
{
    public class ModelCompiler
    {
        private readonly CodeGenHelper _codeGenHelper;

        public ModelCompiler()
        {
            _codeGenHelper = new CodeGenHelper();
        }

        public Type GenerateModelCode(string generatedCode, string className)
        {
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(generatedCode, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.GenModel");
            if (types != null) 
                return types[0];
            return null;
        }

        public List<Type> GenerateCSVModelCode(TypeConfig typeConfig, int jobId = 0, string fileName = "")
        {
            StringBuilder codeBuilder = new StringBuilder();
            
            // Add necessary imports
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using CsvHelper.Configuration;");
            codeBuilder.AppendLine("using System.Linq;");
            codeBuilder.AppendLine("using System.Text.RegularExpressions;");
            codeBuilder.AppendLine("using DataAnalyticsPlatform.Shared.DataAccess;");
            codeBuilder.AppendLine("namespace DataAnalyticsPlatform.Common {");
            
            // Generate Original Record class
            codeBuilder.AppendLine("public partial class OriginalRecord {");
            codeBuilder.AppendLine("  static int rows = 1;");
            codeBuilder.AppendLine("  public OriginalRecord() { Init(); rowid = rows++; }");
            codeBuilder.AppendLine("  partial void Init();");
            
            bool hasRowid = false;
            
            // Add base class fields
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                codeBuilder.Append(_codeGenHelper.GetDataTypeString(fieldInfo.DataType, fieldInfo.Name));
                if (fieldInfo.Name.Contains("rowid"))
                {
                    hasRowid = true;
                }
            }
            
            // Add standard fields if not already present
            if (!hasRowid)
            {
                codeBuilder.AppendLine("  public int rowid { get; set; }");
            }
            codeBuilder.AppendLine("  public int fileid { get; set; }");
            codeBuilder.AppendLine("  public long sessionid { get; set; }");
            codeBuilder.AppendLine("  public string FileName { get; set; }");
            codeBuilder.AppendLine("}");
            
            // Generate model classes
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelName = modelInfo.ModelName.Replace("-", "");
                codeBuilder.AppendLine($"public partial class {modelName} : BaseModel {{");
                
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    string columnName = _codeGenHelper.FormatColumnName(fieldInfo.Name);
                    codeBuilder.Append(_codeGenHelper.GetDataTypeString(fieldInfo.DataType, columnName));
                }
                
                if (!hasRowid)
                {
                    codeBuilder.AppendLine("  public int rowid { get; set; }");
                }
                codeBuilder.AppendLine("  public int fileid { get; set; }");
                codeBuilder.AppendLine("  public long sessionid { get; set; }");
                codeBuilder.AppendLine("  public string FileName { get; set; }");
                codeBuilder.AppendLine("}");
            }
            
            // Generate CsvHelper mapper
            codeBuilder.AppendLine("public class Mappers : ClassMap<OriginalRecord> {");
            codeBuilder.AppendLine("  public Mappers() {");
            
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                string className = fieldInfo.Name.Replace("-", "");
                string fieldName = fieldInfo.DisplayName.Replace("\"", "");
                codeBuilder.AppendLine($"    Map(m => m.{className}).Name(\"{fieldName}\");");
            }
            
            codeBuilder.AppendLine("    AutoMap();");
            codeBuilder.AppendLine("  }");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("}");
            
            // Generate additional code using CodeDom
            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");
            ns.Imports.AddRange(new[]
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
            
            var myClass = new CodeTypeDeclaration("OriginalRecord");
            myClass.IsClass = true;
            myClass.IsPartial = true;
            
            // Add model fields
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelName = modelInfo.ModelName.Replace("-", "");
                var field = new CodeMemberField(modelName, modelName);
                field.Attributes = MemberAttributes.Public;
                myClass.Members.Add(field);
            }
            
            // Add models collection
            var modelsField = new CodeMemberField("List<BaseModel>", "models");
            modelsField.Attributes = MemberAttributes.Public;
            myClass.Members.Add(modelsField);
            
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
            myClass.Members.Add(initMethod);
            
            // Add MapIt, SetProps, SetValues methods
            GenerateHelperMethods(myClass, typeConfig, hasRowid, jobId, fileName);
            
            // Compile code
            ns.Types.Add(myClass);
            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            
            var provider = new CSharpCodeProvider();
            var sw = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions());
            
            codeBuilder.Append(sw.ToString());
            
            // Generate the types
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(codeBuilder.ToString(), new[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.ModelGen");
            
            return types?.ToList();
        }

        private void GenerateHelperMethods(CodeTypeDeclaration myClass, TypeConfig typeConfig, bool hasRowid, int jobId, string fileName)
        {
            // MapIt method
            var mapItMethod = new CodeMemberMethod
            {
                Name = "MapIt",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            // SetProps method
            var setPropsMethod = new CodeMemberMethod
            {
                Name = "SetProps",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            // SetValues method
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
                    string columnName = _codeGenHelper.FormatColumnName(fieldInfo.Name);
                    
                    // Add property to MapIt
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.{columnName} = {fieldInfo.Map};"));
                    
                    // Add property to props and values
                    if (commaStart > 0)
                    {
                        setValuesMethod.Statements.Add(new CodeSnippetStatement(","));
                        setPropsMethod.Statements.Add(new CodeSnippetStatement(","));
                    }
                    
                    setValuesMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.{columnName}"));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement($"\"{columnName}\""));
                    
                    commaStart++;
                }
                
                // Add additional standard fields
                if (!hasRowid)
                {
                    // MapIt implementation
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.rowid = rowid;"));
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.sessionid = {jobId};"));
                    mapItMethod.Statements.Add(new CodeSnippetStatement($"{modelName}.FileName = \"{fileName}\";"));
                    
                    // Add to props and values
                    setValuesMethod.Statements.Add(new CodeSnippetStatement(","));
                    setPropsMethod.Statements.Add(new CodeSnippetStatement(","));
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
            myClass.Members.Add(mapItMethod);
            myClass.Members.Add(setPropsMethod);
            myClass.Members.Add(setValuesMethod);
            myClass.Members.Add(getModelsMethod);
        }

        public List<Type> GenerateJSONModel(TypeConfig typeConfig, int jobId = 0)
        {
            List<string> classes = new List<string>();
            StringBuilder codeBuilder = new StringBuilder();
            
            // Add namespace and imports
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using System.Linq;");
            codeBuilder.AppendLine("using System.Collections;");
            codeBuilder.AppendLine("namespace DataAnalyticsPlatform.Common {");
            
            // Create original record class
            string originalRecordClass = "public partial class OriginalRecord {\n";
            originalRecordClass += "  static int rows = 1;\n";
            originalRecordClass += "  public OriginalRecord() { Init(); rowid = rows++; }\n";
            originalRecordClass += "  partial void Init();\n";
            
            // Check for rowid and fileid in base fields
            bool hasRowid = typeConfig.BaseClassFields.Any(x => x.Name == "rowid");
            if (!hasRowid)
            {
                originalRecordClass += "  public int rowid { get; set; }\n";
                originalRecordClass += "  public long sessionid { get; set; }\n";
            }
            
            bool hasFileid = typeConfig.BaseClassFields.Any(x => x.Name == "fileid");
            if (!hasFileid)
            {
                originalRecordClass += "  public int fileid { get; set; }\n";
            }
            
            // Generate classes from field info
            Dictionary<string, bool> fieldNameExists = new Dictionary<string, bool>();
            _codeGenHelper.GenerateClassFromFieldInfo(
                typeConfig.BaseClassFields,
                classes,
                originalRecordClass,
                "}\n",
                fieldNameExists
            );
            
            fieldNameExists.Clear();
            
            // Add all generated classes
            foreach (string classCode in classes)
            {
                codeBuilder.AppendLine(classCode);
            }
            
            // Generate model classes
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelClass = $"public partial class {modelInfo.ModelName} : BaseModel {{\n";
                
                if (!hasRowid)
                {
                    modelClass += "  public int rowid { get; set; }\n";
                    modelClass += "  public long sessionid { get; set; }\n";
                }
                
                bool hasModelFileid = modelInfo.ModelFields.Any(x => x.Name == "fileid");
                if (!hasModelFileid)
                {
                    modelClass += "  public int fileid { get; set; }\n";
                }
                
                List<string> modelClasses = new List<string>();
                _codeGenHelper.GenerateClassFromFieldInfo(
                    modelInfo.ModelFields,
                    modelClasses,
                    modelClass,
                    "}\n",
                    fieldNameExists
                );
                
                // Add model classes
                foreach (string classCode in modelClasses)
                {
                    codeBuilder.AppendLine(classCode);
                }
            }
            
            codeBuilder.AppendLine("}"); // Close namespace
            
            // Generate CodeDom code for partial methods
            var codeClass = new CodeTypeDeclaration("OriginalRecord");
            string additionalCode = AddModelPartials(ref codeClass, typeConfig, hasRowid, jobId);
            codeBuilder.Append(additionalCode);
            
            // Compile the code
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(
                codeBuilder.ToString(),
                new[] { "DataAnalyticsPlatform.Common.dll" },
                "DataAnalyticsPlatform.ModelGen"
            );
            
            return types?.ToList();
        }

        private string AddModelPartials(ref CodeTypeDeclaration myClass, TypeConfig typeConfig, bool hasRowid = false, int jobId = 0)
        {
            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");
            ns.Imports.AddRange(new[]
            {
                new CodeNamespaceImport("System.IO"),
                new CodeNamespaceImport("System.Collections.Generic"),
                new CodeNamespaceImport("System.Collections"),
                new CodeNamespaceImport("System.Linq"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Common"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Shared.DataModels")
            });
            
            myClass.IsClass = true;
            myClass.IsPartial = true;
            
            // Add model fields
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                var field = new CodeMemberField(modelInfo.ModelName, modelInfo.ModelName);
                field.Attributes = MemberAttributes.Public;
                myClass.Members.Add(field);
            }
            
            // Add models list
            var modelsField = new CodeMemberField("List<BaseModel>", "models");
            modelsField.Attributes = MemberAttributes.Public;
            myClass.Members.Add(modelsField);
            
            // Add Init method
            var initMethod = new CodeMemberMethod
            {
                Name = "Init",
                ReturnType = new CodeTypeReference("partial void"),
                Attributes = MemberAttributes.Final
            };
            
            initMethod.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            myClass.Members.Add(initMethod);
            
            // Add MapIt method
            var mapItMethod = new CodeMemberMethod
            {
                Name = "MapIt",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            // Process JSON model mapping logic
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                List<FieldInfo> fieldInfoList = modelInfo.ModelFields
                    .Where(x => x.Map.Contains("[]."))
                    .OrderBy(q => q.Map.Split('.').Length)
                    .Select(c => { c.Map = c.Map.Replace("[]", ""); return c; })
                    .ToList();
                
                HashSet<string> uniqueInnerClasses = new HashSet<string>();
                int count = 0;
                
                if (fieldInfoList.Count > 0)
                {
                    foreach (FieldInfo info in fieldInfoList)
                    {
                        string[] classTypes = info.Map.Split('.');
                        string myClassType = "";
                        
                        if (classTypes.Length > 0)
                        {
                            if (classTypes.Length == 1)
                                myClassType = classTypes[classTypes.Length - 1];
                            else
                                myClassType = string.Join(".", classTypes.Take(classTypes.Length - 1));
                        }
                        
                        CodeIterationStatement codeFor = null;
                        string mappingClass = "";
                        
                        foreach (FieldInfo finfo in modelInfo.ModelFields)
                        {
                            if (!uniqueInnerClasses.Contains(myClassType))
                            {
                                codeFor = null;
                                count++;
                                string myClassName = classTypes.Length >= 2 ? 
                                    classTypes[classTypes.Length - 2] : 
                                    classTypes[classTypes.Length - 1];
                                    
                                uniqueInnerClasses.Add(myClassType);
                                
                                codeFor = new CodeIterationStatement();
                                codeFor.TestExpression = new CodeSnippetExpression("e" + count + ".MoveNext()");
                                codeFor.IncrementStatement = new CodeSnippetStatement();
                                codeFor.InitStatement = new CodeSnippetStatement("IEnumerator e" + count + " = " + myClassType + ".GetEnumerator()");
                                
                                codeFor.Statements.Add(new CodeSnippetStatement($"{myClassName} {myClassName} = ({myClassName})e{count}.Current;"));
                                codeFor.Statements.Add(new CodeSnippetStatement($"{modelInfo.ModelName} = new {modelInfo.ModelName}();"));
                                codeFor.Statements.Add(new CodeSnippetStatement($"{modelInfo.ModelName}.ModelName = \"{modelInfo.ModelName}\";"));
                                
                                mappingClass = myClassName;
                            }
                            
                            if (codeFor != null)
                            {
                                string formattedPath = finfo.Map.Replace("[]", "");
                                
                                if (finfo.Map.Contains(myClassType))
                                {
                                    string mappingField = "";
                                    int indexEnd = formattedPath.LastIndexOf(".");
                                    
                                    if (indexEnd > -1)
                                    {
                                        mappingField = finfo.Map.Substring(indexEnd + 1);
                                    }
                                    
                                    codeFor.Statements.Add(new CodeSnippetStatement(
                                        $"{modelInfo.ModelName}.{finfo.Name} = {mappingClass}.{mappingField};"
                                    ));
                                }
                                else
                                {
                                    codeFor.Statements.Add(new CodeSnippetStatement(
                                        $"{modelInfo.ModelName}.{finfo.Name} = {formattedPath};"
                                    ));
                                }
                            }
                        }
                        
                        if (codeFor != null)
                        {
                            if (!hasRowid)
                            {
                                codeFor.Statements.Add(new CodeSnippetStatement(
                                    $"{modelInfo.ModelName}.rowid = ++rowid;"
                                ));
                                codeFor.Statements.Add(new CodeSnippetStatement(
                                    $"{modelInfo.ModelName}.sessionid = {jobId};"
                                ));
                            }
                            
                            codeFor.Statements.Add(new CodeSnippetStatement(
                                $"models.Add({modelInfo.ModelName});"
                            ));
                            
                            mapItMethod.Statements.Add(codeFor);
                        }
                    }
                }
                else
                {
                    // Simple mapping for non-array fields
                    mapItMethod.Statements.Add(new CodeSnippetStatement(
                        $"{modelInfo.ModelName} = new {modelInfo.ModelName}();"
                    ));
                    mapItMethod.Statements.Add(new CodeSnippetStatement(
                        $"{modelInfo.ModelName}.ModelName = \"{modelInfo.ModelName}\";"
                    ));
                    
                    foreach (FieldInfo finfo in modelInfo.ModelFields)
                    {
                        mapItMethod.Statements.Add(new CodeSnippetStatement(
                            $"{modelInfo.ModelName}.{finfo.Name} = {finfo.Map};"
                        ));
                    }
                    
                    mapItMethod.Statements.Add(new CodeSnippetStatement(
                        $"{modelInfo.ModelName}.rowid = ++rowid;"
                    ));
                    mapItMethod.Statements.Add(new CodeSnippetStatement(
                        $"models.Add({modelInfo.ModelName});"
                    ));
                }
            }
            
            myClass.Members.Add(mapItMethod);
            
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
            
            myClass.Members.Add(getModelsMethod);
            
            // Generate code
            ns.Types.Add(myClass);
            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            
            var provider = new CSharpCodeProvider();
            var sw = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions());
            
            return sw.ToString();
        }
    }
}
