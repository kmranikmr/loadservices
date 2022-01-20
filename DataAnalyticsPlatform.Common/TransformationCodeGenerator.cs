
using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.SharedUtils;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using CsvHelper;
using System.Text.RegularExpressions;
using CsvHelper.Configuration;
using System.Globalization;
using DataAnalyticsPlatform.Shared.DataAccess;
//using DataAnalyticsPlatform.Shared.DataModels;
namespace DataAnalyticsPlatform.Common
{
    public interface IBaseModel
    {
        string ModelName { get; set; }

        string[] Props { get; set; }

        object[] Values { get; set; }
    }
    public interface IModelMap
    {
        void MapIt();
        List<BaseModel> GetModels();
    }
    public abstract class Entity
    {

    }
    //public class BaseModel //: IBaseModel
    //{
    //    public string ModelName { get; set; }
    //    public long RecordId1 { get; set; }
    //    public long FileId1 { get; set; }
    //    public string[] Props { get; set; }
    //    public object[] Values { get; set; }

    //}
    //public partial class OriginalRecord
    //{

    //}
    //public class Mappers : ClassMap<OriginalRecord>
    //{
    //    public Mappers() { AutoMap(); }
    //}

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ModelMap
    {

        private ModelMapRecord recordField;

        private byte versionField;

        /// <remarks/>
        public ModelMapRecord record
        {
            get
            {
                return this.recordField;
            }
            set
            {
                this.recordField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ModelMapRecord
    {

        private ModelMapRecordModel[] modelField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Model")]
        public ModelMapRecordModel[] Model
        {
            get
            {
                return this.modelField;
            }
            set
            {
                this.modelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ModelMapRecordModel
    {

        private ModelMapRecordModelProp[] propField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("prop")]
        public ModelMapRecordModelProp[] prop
        {
            get
            {
                return this.propField;
            }
            set
            {
                this.propField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ModelMapRecordModelProp
    {

        private string transformField;

        private string nameField;

        /// <remarks/>
        public string transform
        {
            get
            {
                return this.transformField;
            }
            set
            {
                this.transformField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }


    public class TransformationCodeGenerator
    {
        private CodeDomProvider provider;
        public TransformationCodeGenerator()
        {
            provider = CodeDomProvider.CreateProvider("C#");
        }
        public string getDataTypeString(DataType dataType, string name)
        {
            string f = "";
            string Name = name.Replace("-", "");
            if (dataType == DataType.Boolean)
                f = $"public bool? {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Char)
                f = $"public char {Name}" + "{get;set;}\n";
            else if (dataType == DataType.DateTime)
                f = $"public System.DateTime? {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Double)
                f = $"public double? {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Int)
                f = $"public int? {Name}" + "{get;set;}\n";
            else if (dataType == DataType.Long)
                f = $"public long? {Name}" + "{get;set;}\n";
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
        public Type GenerateModelCode(string gen, string classname)
        {
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(gen, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.GenModel");
            if (types != null) return types[0];
            return null;
        }

        public bool CheckDateTime(string dateString)
        {
            string[] formats = { "M/dd/yy", "MM/dd/yy", "MM/dd/yyyy", "MM/d/yy", "M/d/yy" };
            DateTime parsedDateTime;

            return DateTime.TryParseExact(dateString, formats, null,
                                           DateTimeStyles.None, out parsedDateTime);
        }
        public string CheckandGetName(string name)
        {
            if (!provider.IsValidIdentifier(name))
            {
                return name + "0";
            }
            return name;
        }
        public List<Type> Code(TypeConfig typeConfig, int jobid = 0,  string Filename = "")
        {
            List<string> classNames = new List<string>();
            string code = "";
            string f = "using System; using System.Collections.Generic; using CsvHelper.Configuration; using System.Linq; using System.Text.RegularExpressions; using DataAnalyticsPlatform.Shared.DataAccess;\n namespace DataAnalyticsPlatform.Common{";
          //  f += "namespace " + typeConfig.SchemaName + "{";
            f += "public partial class OriginalRecord{\n";//+jobid+ "
            //gen base
            f += "static int rows = 1;";
            f += "public OriginalRecord(){ Init(); rowid = rows++;}\n";//jobid+"


            f += "partial void Init();\n";
            bool hasRowid = false;
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
                if ( fieldInfo.Name.Contains("rowid"))
                {
                    hasRowid = true;
                }
            }
            if (!hasRowid)
            {
                f+= "public int rowid{get;set;}\n";
            }
            f += "public int fileid{get;set;}\n";
            f += "public long sessionid{get;set;}\n";
             f += "public string FileName{get;set;}\n";
            f += "}\n";
            code = f;
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelName = modelInfo.ModelName.Replace("-", "");
                f = String.Format($"public partial class { modelName } : BaseModel");
                f += "{\n";
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    bool dateBool = CheckDateTime(fieldInfo.Name);
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

                    columnName = CheckandGetName(columnName);

                   // var columnName = Regex.Replace(fieldInfo.DisplayName, @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);

                    //columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());

                    string FieldNameTrim = columnName;//fieldInfo.Name.Replace("-", "");
                    f += getDataTypeString(fieldInfo.DataType, FieldNameTrim);
                }
                if (!hasRowid)
                {
                    f += "public int rowid{get;set;}\n";
                }
                f += "public int fileid{get;set;}\n";
                f += "public long sessionid{get;set;}\n";
                  f += "public string FileName{get;set;}\n";
                f += "}\n";
                //f += "}\n";
                code += f;
                f = "";
            }
            code += @"public class Mappers : ClassMap<OriginalRecord>{  public Mappers(){ ";
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                string ClassNameField = fieldInfo.Name.Replace("-", "");
                string FieldNameTrim = fieldInfo.DisplayName;//fieldInfo.Name.Replace("-", "");
                FieldNameTrim = FieldNameTrim.Replace("\"", "");
                code += $"Map(m => m.{ClassNameField}).Name(\"{FieldNameTrim}\");\n";//  fieldInfo.DataType, fieldInfo.Name);
            }
            code += "AutoMap(); }    }";
            code += "}\n";

            // Map(m => m.policyID).Name(""policyID"");

            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");//DataAnalyticsPlatform.Shared
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

            var myclass = new CodeTypeDeclaration("OriginalRecord");//+jobid
            myclass.IsClass = true;
            myclass.IsPartial = true;
            
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelNametrim = modelInfo.ModelName.Replace("-", "");
                CodeMemberField fields = new CodeMemberField(modelNametrim, modelNametrim);
                fields.Attributes = MemberAttributes.Public;
                myclass.Members.Add(fields);
            }
            var memberfield = new CodeMemberField("List<BaseModel>", "models");
            memberfield.Attributes = MemberAttributes.Public;
            myclass.Members.Add(memberfield);


            var memberInit = new CodeMemberMethod();
            memberInit.Name = "Init";
            memberInit.ReturnType = new CodeTypeReference("partial void");
            memberInit.Attributes = MemberAttributes.Final;

            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string modelNameTrim = modelInfo.ModelName.Replace("-", "");
                classNames.Add(modelNameTrim);
                memberInit.Statements.Add(new CodeSnippetStatement(modelNameTrim + " = " + "new " + modelNameTrim + "();"));
            }
            memberInit.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            //foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            //{
            //    memberInit.Statements.Add(new CodeSnippetStatement($"models.Add({modelInfo.ModelName});"));
            //}
            myclass.Members.Add(memberInit);

            var memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "MapIt";
            // CodeParameterDeclarationExpression dec = new CodeParameterDeclarationExpression("OriginalRecord", "originalRecord");
            //dec.Direction = FieldDirection.In;
            // memberMethodMap.Parameters.Add(dec);
            //  memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;
            var memberMethodValues = new CodeMemberMethod();
            memberMethodValues.Name = "SetValues";
            memberMethodValues.Attributes = MemberAttributes.Public;
            memberMethodValues.Attributes |= MemberAttributes.Final;

            var memberMethodProps = new CodeMemberMethod();
            memberMethodProps.Name = "SetProps";
            memberMethodProps.Attributes = MemberAttributes.Public;
            memberMethodProps.Attributes |= MemberAttributes.Final;

            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string ModelNameTrim = modelInfo.ModelName.Replace("-","");
                memberMethodMap.Statements.Add(new CodeSnippetStatement(ModelNameTrim + ".ModelName" + "=" + "\"" + ModelNameTrim + "\"" + ";"));
                memberMethodProps.Statements.Add(new CodeSnippetStatement($"{ModelNameTrim}.Props = new string[] {{"));
                memberMethodValues.Statements.Add(new CodeSnippetStatement($"{ModelNameTrim}.Values = new List<object>() {{"));
                int commaStart = 0;
                foreach (FieldInfo finfo in modelInfo.ModelFields)
                {
                    bool dateBool = CheckDateTime(finfo.Name);
                    string columnName = "";
                    if (dateBool)
                    {
                        columnName = "day" + finfo.Name.Replace("/", "_");
                    }
                    else
                    {
                        columnName = Regex.Replace(finfo.Name, @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
                        columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());

                    }

                    columnName = CheckandGetName(columnName);
                  //  var columnName = Regex.Replace(finfo.DisplayName ,@"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);//finfo.Name
                   // columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());

                    memberMethodMap.Statements.Add(new CodeSnippetStatement(ModelNameTrim + "." + columnName + "=" + finfo.Map + ";"));
                    if ( commaStart > 0 )
                    {
                        memberMethodValues.Statements.Add(new CodeSnippetStatement(","));
                        memberMethodProps.Statements.Add(new CodeSnippetStatement(","));
                    }
                    memberMethodValues.Statements.Add(new CodeSnippetStatement($"{ModelNameTrim}.{columnName }"));
                    memberMethodProps.Statements.Add(new CodeSnippetStatement($"\"{columnName}\""));
                    commaStart++;
                }
                if ( !hasRowid )
                {
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(ModelNameTrim + "." + "rowid" + "=" + "rowid" + ";"));
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(ModelNameTrim + "." + "sessionid" + "=" + jobid + ";"));
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(ModelNameTrim + "." + "FileName" + "=" + "\""+ Filename + "\""+ ";"));
                    memberMethodValues.Statements.Add(new CodeSnippetStatement(","));
                    memberMethodProps.Statements.Add(new CodeSnippetStatement(","));
                    memberMethodValues.Statements.Add(new CodeSnippetStatement($"{ModelNameTrim}.rowid "));
                    memberMethodProps.Statements.Add(new CodeSnippetStatement($"\"rowid\""));
                    memberMethodValues.Statements.Add(new CodeSnippetStatement(","));
                    memberMethodProps.Statements.Add(new CodeSnippetStatement(","));
                    memberMethodValues.Statements.Add(new CodeSnippetStatement($"{ModelNameTrim}.sessionid "));
                    memberMethodProps.Statements.Add(new CodeSnippetStatement($"\"sessionid\""));
                    memberMethodValues.Statements.Add(new CodeSnippetStatement(","));
                    memberMethodProps.Statements.Add(new CodeSnippetStatement(","));
                    memberMethodValues.Statements.Add(new CodeSnippetStatement($"{ModelNameTrim}.FileName "));
                    memberMethodProps.Statements.Add(new CodeSnippetStatement($"\"FileName\""));               

                }
                memberMethodValues.Statements.Add(new CodeSnippetStatement("}.ToArray();"));
                memberMethodProps.Statements.Add(new CodeSnippetStatement("};"));
            }
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string ModelNameTrim = modelInfo.ModelName.Replace("-", "");
                memberMethodMap.Statements.Add(new CodeSnippetStatement("models.Add(" + ModelNameTrim + ")" + ";"));
            }
            memberMethodMap.Statements.Add(new CodeSnippetStatement("SetProps();"));
            memberMethodMap.Statements.Add(new CodeSnippetStatement("SetValues();"));
            myclass.Members.Add(memberMethodMap);
            myclass.Members.Add(memberMethodProps);
            myclass.Members.Add(memberMethodValues);
            memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "GetModels";
            memberMethodMap.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "file_id"));
            memberMethodMap.ReturnType = new CodeTypeReference(typeof(List<BaseModel>));
            // memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;
            memberMethodMap.Statements.Add(new CodeSnippetStatement("fileid  = file_id; "));
            memberMethodMap.Statements.Add(new CodeSnippetStatement("return models; "));
            myclass.Members.Add(memberMethodMap);

            ns.Types.Add(myclass);
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter sw1 = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw1, new CodeGeneratorOptions());
            code += sw1;
            Console.WriteLine(code);
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(code, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.ModelGen");
            if (types == null)
                return null;
            else
                return types.ToList();

        }

        public string GenerateClassfromFieldInfo(List<FieldInfo> FieldinfoList, List<string> classes, string prefix, string suffix, Dictionary<string, bool> fieldExists)
        {
            string f = prefix;

            foreach (FieldInfo fieldInfo in FieldinfoList)
            {
                f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
                if (fieldInfo.DataType == DataType.Object || fieldInfo.DataType == DataType.ObjectArray)
                {
                    if (!fieldExists.ContainsKey(fieldInfo.Name))
                    {
                        fieldExists.Add(fieldInfo.Name, true);
                        GenerateClassfromFieldInfo(fieldInfo.InnerFields, classes, "public partial class " + fieldInfo.Name + "{\n", "}\n", fieldExists);
                    }
                }
            }
            f += suffix;
            classes.Add(f);
            return "";
        }

        public string GenerateModelforJson(string ModelName, List<FieldInfo> modelInfo, List<string>classes, CodeMemberMethod memberMethodMap)
        {
            foreach (FieldInfo finfo in modelInfo)
            {
                memberMethodMap.Statements.Add(new CodeSnippetStatement(ModelName + "." + finfo.Name + "=" + finfo.Map + ";"));

                //if ( finfo.Map.Contains("."))
                //{
                //    CodeIterationStatement codeIterationstmt = new CodeIterationStatement();
                //    codeIterationstmt. = new CodeSnippetExpression()
                //}
            }
            return "";
        }

        public Type[] GenerateModelJSONCode(string gen, string classname)
        {
            RoslynCompiler ros = new RoslynCompiler();
            Console.WriteLine(gen);
            Type[] types = ros.Generate(gen, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.GenModel");
            if (types != null) return types;
            return null;
        }

        public string AddModelPartials(ref CodeTypeDeclaration myclass, TypeConfig typeConfig, bool hasRowid = false, int jobId = 0)
        {
            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");
            //var myclass = new CodeTypeDeclaration("OriginalRecord");
            myclass.IsClass = true;
            myclass.IsPartial = true;

            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                CodeMemberField fields = new CodeMemberField(modelInfo.ModelName, modelInfo.ModelName);
                fields.Attributes = MemberAttributes.Public;
                myclass.Members.Add(fields);
            }
            var memberfield = new CodeMemberField("List<BaseModel>", "models");
            memberfield.Attributes = MemberAttributes.Public;
            myclass.Members.Add(memberfield);


            var memberInit = new CodeMemberMethod();
            memberInit.Name = "Init";
            memberInit.ReturnType = new CodeTypeReference("partial void");
            memberInit.Attributes = MemberAttributes.Final;

           // foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
           // {
              //  classNames.Add(modelInfo.ModelName);
               // memberInit.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + " = " + "new " + modelInfo.ModelName + "();"));
          //  }
            memberInit.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            //foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            //{
            //    memberInit.Statements.Add(new CodeSnippetStatement($"models.Add({modelInfo.ModelName});"));
            //}
            myclass.Members.Add(memberInit);

            var memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "MapIt";
            // CodeParameterDeclarationExpression dec = new CodeParameterDeclarationExpression("OriginalRecord", "originalRecord");
            //dec.Direction = FieldDirection.In;
            // memberMethodMap.Parameters.Add(dec);
            //  memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                List<FieldInfo> fieldInfoList = modelInfo.ModelFields.ToList();
                
                fieldInfoList = fieldInfoList.Where(x => x.Map.Contains("[].") ).Select(x => x).ToList();
                if (fieldInfoList.Count > 0)
                {
                    fieldInfoList = fieldInfoList.OrderBy(q => q.Map.Split('.').Length).ToList();
                    fieldInfoList = fieldInfoList.Select(c => { c.Map = c.Map.Replace("[]", ""); return c; }).ToList();
                }

                HashSet<string> uniqueInnerClasss = new HashSet<string>();
                int count = 0;
                if (fieldInfoList.Count > 0 )
                {
                    foreach (FieldInfo info in fieldInfoList)
                    {
                        string[] classtypes = info.Map.Split('.');
                        string myclassType = "";
                        if (classtypes.Length > 0)
                        {
                            if (classtypes.Length == 1)
                                myclassType = classtypes[classtypes.Length - 1];
                            else
                                myclassType = string.Join(".", classtypes.Take(classtypes.Length - 1));
                        }
                        CodeIterationStatement codefor = null;
                        string MappingClass = "";
                        foreach (FieldInfo finfo in modelInfo.ModelFields)
                        {
                            
                            if (!uniqueInnerClasss.Contains(myclassType))
                            {
                                codefor = null;
                                count++;
                                string myclassName = classtypes.Length >= 2 ? classtypes[classtypes.Length - 2] : classtypes[classtypes.Length - 1];
                                uniqueInnerClasss.Add(myclassType);


                                //string myclassName
                                codefor = new CodeIterationStatement();

                                codefor.TestExpression = new CodeSnippetExpression("e" + count + ".MoveNext()");
                                codefor.IncrementStatement = new CodeSnippetStatement();
                                codefor.InitStatement = new CodeSnippetStatement("IEnumerator e" + count + " =  " + myclassType + " .GetEnumerator()");

                                codefor.Statements.Add(new CodeSnippetStatement($"{myclassName} {myclassName} = ({myclassName})e" + count + ".Current;"));
                                codefor.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + " = " + "new " + modelInfo.ModelName + "();"));
                                codefor.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + ".ModelName" + "=" + "\"" + modelInfo.ModelName + "\"" + ";"));
                                MappingClass = myclassName;
                                // memberMethodMap.Statements.Add(codefor);
                                //  memberMethodMap.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + finfo.Name + "=" + finfo.Map + ";"));
                            }
                            if (codefor != null)//
                            {
                                string formattedPath = finfo.Map.Replace("[]", "");
                                Console.WriteLine(formattedPath);
                                if (finfo.Map.Contains(myclassType))
                                {
                                    string MappingField = "";
                                    int indexEnd = formattedPath.LastIndexOf(".");
                                    Console.WriteLine(indexEnd);
                                    if (indexEnd > -1)
                                    {
                                        MappingField = finfo.Map.Substring(indexEnd + 1);
                                        Console.WriteLine(MappingField);
                                    }
                                    codefor.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + finfo.Name + "=" + MappingClass + "." + MappingField + ";"));
                                }
                                else {
                                    codefor.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + finfo.Name + "=" + formattedPath + ";"));
                                }
                               
                                
                            }
                            
                         }
                        
                        if (codefor != null)
                        {
                            if (!hasRowid)
                            {
                                codefor.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + "rowid" + "=" + "++rowid" + ";"));
                                codefor.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + "sessionid" + "=" + jobId + ";"));
                            }
                            codefor.Statements.Add(new CodeSnippetStatement("models.Add(" + modelInfo.ModelName + ")" + ";"));
                            memberMethodMap.Statements.Add(codefor);
                          
                        }
                    }
                    fieldInfoList = fieldInfoList.Select(c => { c.Map = c.Map.Replace(".", "[]."); return c; }).ToList();
                }
                else
                {
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + " = " + "new " + modelInfo.ModelName + "();"));
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + ".ModelName" + "=" + "\"" + modelInfo.ModelName + "\"" + ";"));

                    foreach (FieldInfo finfo in modelInfo.ModelFields)
                    {
                        memberMethodMap.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + finfo.Name + "=" + finfo.Map + ";"));

                    }
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + "rowid" + "=" + "++rowid" + ";"));
                    memberMethodMap.Statements.Add(new CodeSnippetStatement("models.Add(" + modelInfo.ModelName + ")" + ";"));
                }
            }
            


            //foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            //{
            //    if ( )
            //    memberMethodMap.Statements.Add(new CodeSnippetStatement("models.Add(" + modelInfo.ModelName + ")" + ";"));
            //}
            myclass.Members.Add(memberMethodMap);
            memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "GetModels";
            memberMethodMap.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "file_id"));
            memberMethodMap.ReturnType = new CodeTypeReference(typeof(List<BaseModel>));
            // memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;
            memberMethodMap.Statements.Add(new CodeSnippetStatement("fileid  = file_id; "));
            memberMethodMap.Statements.Add(new CodeSnippetStatement("return models; "));
            myclass.Members.Add(memberMethodMap);
            
            ns.Types.Add(myclass);
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter sw1 = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw1, new CodeGeneratorOptions());
            return sw1.ToString() ;

        }
        public List<Type> CodeTwitter(TypeConfig typeConfig, int jobid = 0)
        {
            string code = File.ReadAllText("TwitterOriginal.cs");
            Type[] types = GenerateModelJSONCode(code, "");
            return types.ToList();
        }
        public List<Type> CodeJSON(TypeConfig typeConfig, int jobid = 0)
        {
            List<string> classNames = new List<string>();
            string code = "";
            string f = "using System.Collections.Generic; using System.Linq; using System.Collections; \n namespace DataAnalyticsPlatform.Common{";

            string Pf = "public partial class OriginalRecord{\n";
            //+jobid+
            //gen base
            Pf += "static int rows = 1;";
            Pf += "public OriginalRecord(){ Init(); rowid = rows++;}\n";//"+jobid+"
            Pf += "partial void Init();\n";
            List<string> classes = new List<string>();
            Console.WriteLine(" common 1");
            bool hasRowid = typeConfig.BaseClassFields.Any(x => x.Name == "rowid");
            
            if (!hasRowid)
            {
                Pf += "public int rowid{get;set;}\n";
                Pf += "public long sessionid{get;set;}\n";
            }
            bool hasFileid = typeConfig.BaseClassFields.Any(x => x.Name == "fileid");
            if (!hasFileid)
            {
                Pf += "public int fileid{get;set;}\n";
            }

            Dictionary<string, bool> FieldNameExists = new Dictionary<string, bool>();
            GenerateClassfromFieldInfo(typeConfig.BaseClassFields, classes, Pf, "}\n", FieldNameExists);
            Console.WriteLine(" common 2");
            FieldNameExists.Clear();
            //foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            //{
            //    f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
            //}
            //f += "}\n";
            f += classes.Aggregate((i, j) => i +  "\n" + j);

            code = f;
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                string mf = String.Format($"public partial class { modelInfo.ModelName} : BaseModel");
                mf += "{\n";
                //foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                //{
                //    f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
                //}
                //f += "}\n";
                if (!hasRowid)
                {
                    mf += "public int rowid{get;set;}\n";
                    mf += "public long sessionid{get;set;}\n";
                }
                bool hasModelFileid = modelInfo.ModelFields.Any(x => x.Name == "fileid");
                if (!hasModelFileid)
                {
                    mf += "public int fileid{get;set;}\n";
                }
                List<string> Mclasses = new List<string>();
                GenerateClassfromFieldInfo(modelInfo.ModelFields, Mclasses, mf, "}\n", FieldNameExists);
                f = Mclasses.Aggregate((i, j) => i + "\n" + j);
              
                code += f;
                f = "";
            }
            code += "}\n";

            var ns = new CodeNamespace("DataAnalyticsPlatform.Common");//DataAnalyticsPlatform.Shared
            ns.Imports.AddRange(new CodeNamespaceImport[]
                {
                    new CodeNamespaceImport("System.IO"),
                     new CodeNamespaceImport("System.Collections.Generic"),
                     new CodeNamespaceImport("System.Collections"),
                      new CodeNamespaceImport("System.Linq"),
                    new CodeNamespaceImport("DataAnalyticsPlatform.Common"),
                    new CodeNamespaceImport("DataAnalyticsPlatform.Shared.DataModels")
                 });

            
            var myclass = new CodeTypeDeclaration("OriginalRecord");//+jobid
            string sw2 = AddModelPartials(ref myclass, typeConfig, hasRowid, jobid);
            code += sw2;
            Console.WriteLine(code);
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(code, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.ModelGen");
            if (types == null)
                return null;
            else
                return types.ToList();

            myclass.IsClass = true;
            myclass.IsPartial = true;

            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                CodeMemberField fields = new CodeMemberField(modelInfo.ModelName, modelInfo.ModelName);
                fields.Attributes = MemberAttributes.Public;
                myclass.Members.Add(fields);
            }
            var memberfield = new CodeMemberField("List<BaseModel>", "models");
            memberfield.Attributes = MemberAttributes.Public;
            myclass.Members.Add(memberfield);


            var memberInit = new CodeMemberMethod();
            memberInit.Name = "Init";
            memberInit.ReturnType = new CodeTypeReference("partial void");
            memberInit.Attributes = MemberAttributes.Final;

            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                classNames.Add(modelInfo.ModelName);
                memberInit.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + " = " + "new " + modelInfo.ModelName + "();"));
            }
            memberInit.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            //foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            //{
            //    memberInit.Statements.Add(new CodeSnippetStatement($"models.Add({modelInfo.ModelName});"));
            //}
            myclass.Members.Add(memberInit);

            var memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "MapIt";
            // CodeParameterDeclarationExpression dec = new CodeParameterDeclarationExpression("OriginalRecord", "originalRecord");
            //dec.Direction = FieldDirection.In;
            // memberMethodMap.Parameters.Add(dec);
            //  memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                memberMethodMap.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + ".ModelName" + "=" + "\"" + modelInfo.ModelName + "\"" + ";"));

                foreach (FieldInfo finfo in modelInfo.ModelFields)
                {
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(modelInfo.ModelName + "." + finfo.Name + "=" + finfo.Map + ";"));
                }
            }
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                memberMethodMap.Statements.Add(new CodeSnippetStatement("models.Add(" + modelInfo.ModelName + ")" + ";"));
            }
            myclass.Members.Add(memberMethodMap);
            memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "GetModels";
            memberMethodMap.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "file_id"));
            memberMethodMap.ReturnType = new CodeTypeReference(typeof(List<BaseModel>));
            // memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;
            memberMethodMap.Statements.Add(new CodeSnippetStatement("fileid  = file_id; "));
            memberMethodMap.Statements.Add(new CodeSnippetStatement("return models; "));
            myclass.Members.Add(memberMethodMap);

            ns.Types.Add(myclass);
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter sw1 = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw1, new CodeGeneratorOptions());
            code += sw1;
            RoslynCompiler ros1 = new RoslynCompiler();
            Type[] typesll = ros1.Generate(code, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.ModelGen");
            if (typesll == null)
                return null;
            else
                return typesll.ToList();

        }
    }
}
