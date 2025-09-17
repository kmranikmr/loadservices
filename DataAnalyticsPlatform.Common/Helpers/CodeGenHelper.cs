using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared;
namespace DataAnalyticsPlatform.Common.Helpers
{
    public class CodeGenHelper
    {
        private CodeDomProvider provider;
 
        public CodeGenHelper()
        {
            provider = CodeDomProvider.CreateProvider("C#");
        }

        public string GetDataTypeString(DataType dataType, string name)
        {
            string formattedName = name.Replace("-", "");
            string propertyCode = "";

            switch (dataType)
            {
                case DataType.Boolean:
                    propertyCode = $"public bool? {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.Char:
                    propertyCode = $"public char {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.DateTime:
                    propertyCode = $"public System.DateTime? {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.Double:
                    propertyCode = $"public double? {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.Int:
                    propertyCode = $"public int? {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.Long:
                    propertyCode = $"public long? {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.String:
                    propertyCode = $"public string {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.StringArray:
                    propertyCode = $"public string[] {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.IntArray:
                    propertyCode = $"public int[] {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.FloatArray:
                    propertyCode = $"public float[] {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.ObjectArray:
                    propertyCode = $"public {formattedName}[] {formattedName}" + " { get; set; }\n";
                    break;
                case DataType.Object:
                    propertyCode = $"public {formattedName} {formattedName}" + " { get; set; }\n";
                    break;
                default:
                    propertyCode = $"public object {formattedName}" + " { get; set; }\n";
                    break;
            }

            return propertyCode;
        }

        public bool CheckDateTime(string dateString)
        {
            string[] formats = { "M/dd/yy", "MM/dd/yy", "MM/dd/yyyy", "MM/d/yy", "M/d/yy" };
            return DateTime.TryParseExact(dateString, formats, null, System.Globalization.DateTimeStyles.None, out _);
        }

        public string CheckAndGetName(string name)
        {
            if (!provider.IsValidIdentifier(name))
            {
                return name + "0";
            }
            return name;
        }

        public string GenerateClassFromFieldInfo(List<FieldInfo> fieldInfoList, List<string> classes, string prefix, string suffix, Dictionary<string, bool> fieldExists)
        {
            StringBuilder codeBuilder = new StringBuilder(prefix);

            foreach (FieldInfo fieldInfo in fieldInfoList)
            {
                codeBuilder.Append(GetDataTypeString(fieldInfo.DataType, fieldInfo.Name));
                
                if (fieldInfo.DataType == DataType.Object || fieldInfo.DataType == DataType.ObjectArray)
                {
                    if (!fieldExists.ContainsKey(fieldInfo.Name))
                    {
                        fieldExists.Add(fieldInfo.Name, true);
                        GenerateClassFromFieldInfo(
                            fieldInfo.InnerFields, 
                            classes, 
                            "public partial class " + fieldInfo.Name + " {\n", 
                            "}\n", 
                            fieldExists
                        );
                    }
                }
            }
            
            codeBuilder.Append(suffix);
            classes.Add(codeBuilder.ToString());
            return "";
        }

        public string FormatColumnName(string name)
        {
            if (CheckDateTime(name))
            {
                return "day" + name.Replace("/", "_");
            }
            else
            {
                string columnName = Regex.Replace(name, @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
                columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());
                return CheckAndGetName(columnName);
            }
        }

        public CodeNamespace CreateNamespace(string namespaceName, string[] imports)
        {
            var ns = new CodeNamespace(namespaceName);
            
            foreach (var import in imports)
            {
                ns.Imports.Add(new CodeNamespaceImport(import));
            }
            
            return ns;
        }

        public void GenerateMapItMethod(CodeTypeDeclaration classDeclaration, TypeConfig typeConfig, bool hasRowid = false, int jobId = 0)
        {
            var memberMethodMap = new CodeMemberMethod
            {
                Name = "MapIt",
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };

            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                memberMethodMap.Statements.Add(
                    new CodeSnippetStatement(modelInfo.ModelName + ".ModelName = \"" + modelInfo.ModelName + "\";")
                );

                foreach (FieldInfo finfo in modelInfo.ModelFields)
                {
                    memberMethodMap.Statements.Add(
                        new CodeSnippetStatement(modelInfo.ModelName + "." + finfo.Name + " = " + finfo.Map + ";")
                    );
                }

                if (!hasRowid)
                {
                    memberMethodMap.Statements.Add(
                        new CodeSnippetStatement(modelInfo.ModelName + ".rowid = rowid++;")
                    );
                    memberMethodMap.Statements.Add(
                        new CodeSnippetStatement(modelInfo.ModelName + ".sessionid = " + jobId + ";")
                    );
                }

                memberMethodMap.Statements.Add(
                    new CodeSnippetStatement("models.Add(" + modelInfo.ModelName + ");")
                );
            }

            classDeclaration.Members.Add(memberMethodMap);
        }
    }
}
