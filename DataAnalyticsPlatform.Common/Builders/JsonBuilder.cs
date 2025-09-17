using DataAnalyticsPlatform.Common.Helpers;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.SharedUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using DataAnalyticsPlatform.Shared;
namespace DataAnalyticsPlatform.Common.Builders
{
    public class JsonBuilder
    {
        private readonly ModelCompiler _modelCompiler;
        private readonly CodeGenHelper _codeGenHelper;

        public JsonBuilder(CodeGenHelper codeGenHelper, ModelCompiler modelCompiler)
        {
            _modelCompiler = modelCompiler;
            _codeGenHelper = codeGenHelper;
        }

        public List<Type> BuildJsonModel(TypeConfig typeConfig, int jobId = 0)
        {
            return _modelCompiler.GenerateJSONModel(typeConfig, jobId);
        }

        public string GenerateJsonClassStructure(List<FieldInfo> fieldInfoList, Dictionary<string, bool> existingFields)
        {
            List<string> classes = new List<string>();
            _codeGenHelper.GenerateClassFromFieldInfo(
                fieldInfoList,
                classes,
                "public partial class JsonRoot {\n",
                "}\n",
                existingFields
            );

            return string.Join("\n", classes);
        }

        public string GenerateJsonMapping(ModelInfo modelInfo, string modelName)
        {
            StringBuilder mappingCode = new StringBuilder();
            
            mappingCode.AppendLine($"{modelName}.ModelName = \"{modelName}\";");
            
            foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
            {
                mappingCode.AppendLine($"{modelName}.{fieldInfo.Name} = {fieldInfo.Map};");
            }
            
            return mappingCode.ToString();
        }

        public string GenerateNestedJsonMapping(ModelInfo modelInfo, string parentPath, string modelName)
        {
            StringBuilder mappingCode = new StringBuilder();
            
            // Check if we need to handle arrays
            bool hasArrays = modelInfo.ModelFields.Any(f => f.Map.Contains("[]"));
            
            if (hasArrays)
            {
                // Group fields by their common array paths
                var fieldGroups = modelInfo.ModelFields
                    .Where(f => f.Map.Contains("[]"))
                    .GroupBy(f => f.Map.Substring(0, f.Map.IndexOf("[]") + 2))
                    .ToList();
                
                foreach (var group in fieldGroups)
                {
                    string arrayPath = group.Key;
                    string arrayVarName = "array_" + modelInfo.ModelName.ToLower() + "_" + arrayPath.Replace(".", "_").Replace("[]", "");
                    
                    mappingCode.AppendLine($"var {arrayVarName} = {arrayPath};");
                    mappingCode.AppendLine($"foreach (var item in {arrayVarName}) {{");
                    mappingCode.AppendLine($"  var {modelName} = new {modelName}();");
                    mappingCode.AppendLine($"  {modelName}.ModelName = \"{modelName}\";");
                    
                    foreach (var field in group)
                    {
                        string fieldPath = field.Map.Replace(arrayPath, "item");
                        mappingCode.AppendLine($"  {modelName}.{field.Name} = {fieldPath};");
                    }
                    
                    // Add additional standard fields
                    mappingCode.AppendLine($"  {modelName}.rowid = ++rowId;");
                    mappingCode.AppendLine($"  models.Add({modelName});");
                    mappingCode.AppendLine("}");
                }
            }
            else
            {
                // Simple mapping for non-array fields
                mappingCode.AppendLine($"{modelName}.ModelName = \"{modelName}\";");
                
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    mappingCode.AppendLine($"{modelName}.{fieldInfo.Name} = {fieldInfo.Map};");
                }
                
                mappingCode.AppendLine($"{modelName}.rowid = ++rowId;");
                mappingCode.AppendLine($"models.Add({modelName});");
            }
            
            return mappingCode.ToString();
        }

        public string GenerateCompleteJsonModel(TypeConfig typeConfig)
        {
            StringBuilder codeBuilder = new StringBuilder();
            
            // Add namespace and imports
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using System.Linq;");
            codeBuilder.AppendLine("using System.Collections;");
            codeBuilder.AppendLine("namespace DataAnalyticsPlatform.Common {");
            
            // Generate original record class and all model classes
            Dictionary<string, bool> fieldExists = new Dictionary<string, bool>();
            
            // Generate root class
            codeBuilder.AppendLine("public partial class OriginalRecord {");
            codeBuilder.AppendLine("  static int rowId = 1;");
            codeBuilder.AppendLine("  public OriginalRecord() { Init(); }");
            codeBuilder.AppendLine("  partial void Init();");
            codeBuilder.AppendLine("  public int rowid { get; set; }");
            codeBuilder.AppendLine("  public int fileid { get; set; }");
            codeBuilder.AppendLine("  public long sessionid { get; set; }");
            
            // Add base fields
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                codeBuilder.Append(_codeGenHelper.GetDataTypeString(fieldInfo.DataType, fieldInfo.Name));
            }
            
            codeBuilder.AppendLine("}");
            
            // Generate model classes
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                codeBuilder.AppendLine($"public partial class {modelInfo.ModelName} : BaseModel {{");
                codeBuilder.AppendLine("  public int rowid { get; set; }");
                codeBuilder.AppendLine("  public int fileid { get; set; }");
                codeBuilder.AppendLine("  public long sessionid { get; set; }");
                
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    codeBuilder.Append(_codeGenHelper.GetDataTypeString(fieldInfo.DataType, fieldInfo.Name));
                }
                
                codeBuilder.AppendLine("}");
            }
            
            codeBuilder.AppendLine("}"); // Close namespace
            
            return codeBuilder.ToString();
        }
        
        public List<Type> CodeJSON(TypeConfig typeConfig, int jobId = 0)
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
            string additionalCode = AddJsonModelPartials(ref codeClass, typeConfig, hasRowid, jobId);
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
        
        private string AddJsonModelPartials(ref CodeTypeDeclaration myClass, TypeConfig typeConfig, bool hasRowid = false, int jobId = 0)
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
}
