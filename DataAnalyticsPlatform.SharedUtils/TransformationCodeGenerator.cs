
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
using CsvHelper.Configuration;
//using DataAnalyticsPlatform.Shared.DataModels;
namespace DataAnalyticsPlatform.SharedUtils
{

    public interface IModelMap
    {
        void Map();
        List<BaseModel> GetModels();
    }
    public abstract class Entity
    {

    }
    public class BaseModel 
    {
        public string ModelName { get; set; }
    }
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
        public TransformationCodeGenerator()
        {

        }
        public string getDataTypeString(DataType dataType, string Name)
        {
            string f = "";
            if (dataType == DataType.Boolean)
                f = String.Format($"public bool {Name};\n");
            else if (dataType == DataType.Char)
                f = String.Format($"public char {Name};\n");
            else if (dataType == DataType.DateTime)
                f = String.Format($"public DateTime {Name};\n");
            else if (dataType == DataType.Double)
                f = String.Format($"public double {Name};\n");
            else if (dataType == DataType.Int)
                f = String.Format($"public int {Name};\n");
            else if (dataType == DataType.Long)
                f = String.Format($"public long {Name};\n");
            else if (dataType == DataType.String)
                f = String.Format($"public string {Name};\n");
            return f;
        }
        public Type GenerateModelCode(string gen, string classname)
        {
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(gen, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.GenModel");
            if (types != null) return types[0];
            return null;
        }
        public List<Type> Code(TypeConfig typeConfig)
        {
            List<string> classNames = new List<string>();
            string code = "";
            string f = "using System.Collections.Generic; using CsvHelper.Configuration; using System.Linq; \n namespace DataAnalyticsPlatform.SharedUtils{";

            f += "public partial class OriginalRecord {\n";
            //gen base
            f += "public OriginalRecord(){ Init();}\n";
            f += "partial void Init();\n";
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
            }
            f += "}\n";
            code = f;
            foreach (ModelInfo modelInfo in typeConfig.ModelInfoList)
            {
                f = String.Format($"public partial class { modelInfo.ModelName} : BaseModel");
                f += "{\n";
                foreach (FieldInfo fieldInfo in modelInfo.ModelFields)
                {
                    f += getDataTypeString(fieldInfo.DataType, fieldInfo.Name);
                }
                f += "}\n";
                code += f;
                f = "";
            }
            code += @"public class Mappers : ClassMap<OriginalRecord>{  public Mappers(){ ";
            foreach (FieldInfo fieldInfo in typeConfig.BaseClassFields)
            {
                code += $"Map(m => m.{fieldInfo.Name}).Name(\"{fieldInfo.Name}\");\n";//  fieldInfo.DataType, fieldInfo.Name);
            }
            code += "AutoMap(); }    }";
            code += "}\n";

            // Map(m => m.policyID).Name(""policyID"");

            var ns = new CodeNamespace("DataAnalyticsPlatform.SharedUtils");//DataAnalyticsPlatform.Common
            ns.Imports.AddRange(new CodeNamespaceImport[]
                {
                    new CodeNamespaceImport("System.IO"),
                     new CodeNamespaceImport("System.Collections.Generic"),
                      new CodeNamespaceImport("System.Linq"),
                   // new CodeNamespaceImport("DataAnalyticsPlatform.Common"),
                  //  new CodeNamespaceImport("DataAnalyticsPlatform.Shared.DataModels")
                 });

            var myclass = new CodeTypeDeclaration("OriginalRecord");
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
            memberMethodMap.Name = "Map";
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
            memberMethodMap.ReturnType = new CodeTypeReference(typeof(List<BaseModel>));
            // memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;

            memberMethodMap.Statements.Add(new CodeSnippetStatement("return models; "));
            myclass.Members.Add(memberMethodMap);

            ns.Types.Add(myclass);
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter sw1 = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw1, new CodeGeneratorOptions());
            code += sw1;
            RoslynCompiler ros = new RoslynCompiler();
            Type[] types = ros.Generate(code, new string[] { "DataAnalyticsPlatform.Common.dll" }, "DataAnalyticsPlatform.ModelGen");
            if (types == null)
                return null;
            else
                return types.ToList();

        }
        /*public List<Type> Code(TypeConfig typeConfig)
        {
            string completeSourceCode = "";
            List<string> classNames = new List<string>();
            // ModelMap mapping = Helper.DeserializeFromXmlString<ModelMap>(xml);
            string originalRecordClass = typeConfig.
            var jobject = JsonConvert.DeserializeObject<TypeConfig>();
            var ns = new CodeNamespace("DataAnalyticsPlatform.Shared");
            ns.Imports.AddRange(new CodeNamespaceImport[]
                {
                    new CodeNamespaceImport("System.IO"),
                     new CodeNamespaceImport("System.Collections.Generic")
                 });

            if (originalRecordClass != "")
            {
                completeSourceCode += originalRecordClass;
                //using (StreamWriter sw = new StreamWriter(@"e:\temp\TestFile.cs", false))
                //{
                //    IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");
                //    tw.Write(originalRecordClass);
                //    tw.Close();
                //}
            }
            string file = "";
            foreach (JObject jobj in splitSchemas)
            {
                var schema4 = JsonSchema4.FromJsonAsync(jobj.ToString()).Result;
                CSharpGeneratorSettings gs = new CSharpGeneratorSettings();
                gs.Namespace = "DataAnalyticsPlatform.Shared";
                gs.ClassStyle = CSharpClassStyle.Poco;
                var generator = new CSharpGenerator(schema4, gs);
                file += generator.GenerateFile();
            }
            completeSourceCode += "\n";
            completeSourceCode += file; 

            var myclass = new CodeTypeDeclaration("OriginalRecord");
            classNames.Add("OriginalRecord");
            myclass.IsClass = true;
            myclass.IsPartial = true;
            myclass.TypeAttributes = System.Reflection.TypeAttributes.Public;


            foreach (ModelMapRecordModel mrec in mapping.record.Model)
            {
                CodeMemberField fields = new CodeMemberField(mrec.name, mrec.name);
                fields.Attributes = MemberAttributes.Public;
                myclass.Members.Add(fields);
            }
            var memberfield = new CodeMemberField("List<BaseModel>", "models");
            memberfield.Attributes = MemberAttributes.Public;
            myclass.Members.Add(memberfield);

            // memv.Statements.Add(new CodeSnippetStatement("List<BaseModel> models = new List<BaseModel>();"));
            //memberMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            //myclass.Members.Add(memberMethod);
            var memberInit = new CodeMemberMethod();
            memberInit.Name = "Init";
            memberInit.ReturnType = new CodeTypeReference("partial void");
            memberInit.Attributes = MemberAttributes.Final;

            foreach (ModelMapRecordModel mrec in mapping.record.Model)
            {
                classNames.Add(mrec.name);
                memberInit.Statements.Add(new CodeSnippetStatement(mrec.name + " = " + "new " + mrec.name + "();"));
            }
            memberInit.Statements.Add(new CodeSnippetStatement("models = new List<BaseModel>();"));
            myclass.Members.Add(memberInit);

            var memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "Map";
            CodeParameterDeclarationExpression dec = new CodeParameterDeclarationExpression("SingleRecord", "singleRecord");
            dec.Direction = FieldDirection.In;
            memberMethodMap.Parameters.Add(dec);
           //  memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;
            foreach (ModelMapRecordModel mrec in mapping.record.Model)
            {
                foreach (ModelMapRecordModelProp pro in mrec.prop)
                {
                    memberMethodMap.Statements.Add(new CodeSnippetStatement(mrec.name + "." + pro.name + "=" + pro.transform + ";"));
                }
            }
            foreach (ModelMapRecordModel mrec in mapping.record.Model)
            {
                memberMethodMap.Statements.Add(new CodeSnippetStatement("models.Add(" + mrec.name + ");"));
            }
            myclass.Members.Add(memberMethodMap);
            memberMethodMap = new CodeMemberMethod();
            memberMethodMap.Name = "GetModels";
            memberMethodMap.ReturnType = new CodeTypeReference(typeof(List<BaseModel>));
            // memberMethodMap.Attributes = MemberAttributes.Override;
            memberMethodMap.Attributes = MemberAttributes.Public;
            memberMethodMap.Attributes |= MemberAttributes.Final;

            memberMethodMap.Statements.Add(new CodeSnippetStatement("return models; "));
            myclass.Members.Add(memberMethodMap);

            ns.Types.Add(myclass);
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);

            // CodeCompilerHelper hel = new CodeCompilerHelper();
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter sw = new StringWriter();
            provider.GenerateCodeFromCompileUnit(compileUnit, sw, new CodeGeneratorOptions());

            completeSourceCode += "\n";
            completeSourceCode +=  sw;
            CodeCompilerHelper helper = new CodeCompilerHelper();
            List<Type> types = helper.generate(completeSourceCode, classNames);//0 is base
            return types;
        }*/
    }
}
