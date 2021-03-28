using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
namespace DataAnalyticsPlatform.Common
{
    public class CodeCompilerHelper
    {

        public Type myType { get; set; }
        public CompilerParameters param  = null;
        public Assembly assem;
        public CodeCompilerHelper()
        {
           
        
        }
        private bool AddLoadedAssembly(string name)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.FullName.StartsWith(name))
                {
                    param.ReferencedAssemblies.Add(a.Location);
                    return true;
                }
            }
            return false;
        }
        public List<Type> generate(string source, List<string> className)
        {
            List<Type> types = new List<Type>();
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            param = new CompilerParameters {
                GenerateExecutable = false,
                IncludeDebugInformation = true,
                 GenerateInMemory = true ,
              
            };
            param.TempFiles.KeepFiles = true;
            param.ReferencedAssemblies.Add("System.dll");
            param.ReferencedAssemblies.Add("System.Xml.dll");
            param.ReferencedAssemblies.Add("System.Xml.Linq.dll");
           // param.ReferencedAssemblies.Add("System.Core.dll");

            var results = codeProvider.CompileAssemblyFromSource(param, source);
            Type myType = null;
            if (results.Errors.HasErrors)
            {

                foreach (var error in results.Errors)
                {
                    // Console.WriteLine(error);

                }
            }
            else
            {
                assem = results.CompiledAssembly;
                for (int i = 0; i < className.Count; i++)
                {
                    types.Add(results.CompiledAssembly.GetType("DataAnalyticsPlatform.Common." + className[i]));

                }
               
            }
            return types;
        }
    }
}
