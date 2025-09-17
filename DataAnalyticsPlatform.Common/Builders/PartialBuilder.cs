using DataAnalyticsPlatform.Common.Helpers;
using DataAnalyticsPlatform.Shared.DataAccess;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataAnalyticsPlatform.Shared;
using Microsoft.CSharp;

namespace DataAnalyticsPlatform.Common.Builders
{
    public class PartialBuilder
    {
        private readonly CodeGenHelper _codeGenHelper;

        public PartialBuilder()
        {
            _codeGenHelper = new CodeGenHelper();
        }

        public string BuildPartialClass(TypeConfig typeConfig, string className, bool hasRowid = false, int jobId = 0)
        {
            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");
            ns.Imports.AddRange(new[]
            {
                new CodeNamespaceImport("System"),
                new CodeNamespaceImport("System.IO"),
                new CodeNamespaceImport("System.Collections.Generic"),
                new CodeNamespaceImport("System.Linq"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Common"),
                new CodeNamespaceImport("DataAnalyticsPlatform.Shared.DataModels")
            });
            
            var classDeclaration = new CodeTypeDeclaration(className);
            classDeclaration.IsClass = true;
            classDeclaration.IsPartial = true;
            
            // Add model fields
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                var field = new CodeMemberField(modelInfo.ModelName, modelInfo.ModelName);
                field.Attributes = MemberAttributes.Public;
                classDeclaration.Members.Add(field);
            }
            
            // Add models collection
            var modelsField = new CodeMemberField("List<BaseModel>", "models");
            modelsField.Attributes = MemberAttributes.Public;
            classDeclaration.Members.Add(modelsField);
            
            // Add Init method
            var initMethod = new CodeMemberMethod
            {
                Name = "Init",
                ReturnType = new CodeTypeReference("partial void"),
                Attributes = MemberAttributes.Final
            };
            
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                initMethod.Statements.Add(new CodeSnippetStatement(
                    $"{modelInfo.ModelName} = new {modelInfo.ModelName}();"
                ));
            }
            
            initMethod.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            classDeclaration.Members.Add(initMethod);
            
            // Add MapIt method
            var mapItMethod = BuildMapItMethod(typeConfig, hasRowid, jobId);
            classDeclaration.Members.Add(mapItMethod);
            
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
            classDeclaration.Members.Add(getModelsMethod);
            
            // Generate code
            ns.Types.Add(classDeclaration);
            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            
            var provider = new CSharpCodeProvider();
            var sw = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions());
            
            return sw.ToString();
        }

        private CodeMemberMethod BuildMapItMethod(TypeConfig typeConfig, bool hasRowid, int jobId)
        {
            var mapItMethod = new CodeMemberMethod
            {
                Name = "MapIt",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                mapItMethod.Statements.Add(new CodeSnippetStatement(
                    $"{modelInfo.ModelName}.ModelName = \"{modelInfo.ModelName}\";"
                ));
                
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    mapItMethod.Statements.Add(new CodeSnippetStatement(
                        $"{modelInfo.ModelName}.{fieldInfo.Name} = {fieldInfo.Map};"
                    ));
                }
                
                if (!hasRowid)
                {
                    mapItMethod.Statements.Add(new CodeSnippetStatement(
                        $"{modelInfo.ModelName}.rowid = rowid;"
                    ));
                    mapItMethod.Statements.Add(new CodeSnippetStatement(
                        $"{modelInfo.ModelName}.sessionid = {jobId};"
                    ));
                }
                
                mapItMethod.Statements.Add(new CodeSnippetStatement(
                    $"models.Add({modelInfo.ModelName});"
                ));
            }
            
            return mapItMethod;
        }
        
        public string BuildPropsAndValuesMethod(TypeConfig typeConfig)
        {
            var propsMethod = new CodeMemberMethod
            {
                Name = "SetProps",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            var valuesMethod = new CodeMemberMethod
            {
                Name = "SetValues",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                // SetProps implementation
                propsMethod.Statements.Add(new CodeSnippetStatement(
                    $"{modelInfo.ModelName}.Props = new string[] {{"
                ));
                
                // SetValues implementation
                valuesMethod.Statements.Add(new CodeSnippetStatement(
                    $"{modelInfo.ModelName}.Values = new List<object>() {{"
                ));
                
                int commaStart = 0;
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    if (commaStart > 0)
                    {
                        valuesMethod.Statements.Add(new CodeSnippetStatement(","));
                        propsMethod.Statements.Add(new CodeSnippetStatement(","));
                    }
                    
                    valuesMethod.Statements.Add(new CodeSnippetStatement(
                        $"{modelInfo.ModelName}.{fieldInfo.Name}"
                    ));
                    propsMethod.Statements.Add(new CodeSnippetStatement(
                        $"\"{fieldInfo.Name}\""
                    ));
                    
                    commaStart++;
                }
                
                // Add standard fields
                if (commaStart > 0)
                {
                    valuesMethod.Statements.Add(new CodeSnippetStatement(","));
                    propsMethod.Statements.Add(new CodeSnippetStatement(","));
                }
                
                valuesMethod.Statements.Add(new CodeSnippetStatement(
                    $"{modelInfo.ModelName}.rowid"
                ));
                propsMethod.Statements.Add(new CodeSnippetStatement("\"rowid\""));
                
                valuesMethod.Statements.Add(new CodeSnippetStatement(","));
                propsMethod.Statements.Add(new CodeSnippetStatement(","));
                valuesMethod.Statements.Add(new CodeSnippetStatement(
                    $"{modelInfo.ModelName}.sessionid"
                ));
                propsMethod.Statements.Add(new CodeSnippetStatement("\"sessionid\""));
                
                valuesMethod.Statements.Add(new CodeSnippetStatement(","));
                propsMethod.Statements.Add(new CodeSnippetStatement(","));
                valuesMethod.Statements.Add(new CodeSnippetStatement(
                    $"{modelInfo.ModelName}.fileid"
                ));
                propsMethod.Statements.Add(new CodeSnippetStatement("\"fileid\""));
                
                // Close arrays
                valuesMethod.Statements.Add(new CodeSnippetStatement("}.ToArray();"));
                propsMethod.Statements.Add(new CodeSnippetStatement("};"));
            }
            
            // Generate code
            var provider = new CSharpCodeProvider();
            
            var swProps = new StringWriter();
            var swValues = new StringWriter();
            
            provider.GenerateCodeFromMember(propsMethod, swProps, new CodeGeneratorOptions());
            provider.GenerateCodeFromMember(valuesMethod, swValues, new CodeGeneratorOptions());
            
            return swProps.ToString() + Environment.NewLine + swValues.ToString();
        }
        
        public string AddModelPartials(ref CodeTypeDeclaration myClass, TypeConfig typeConfig, bool hasRowid = false, int jobId = 0)
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
