using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace DataAnalyticsPlatform.Shared
{
    public static class CompileHelper
    {
        public static Assembly CompileCode(string code)
        {
            var csc = new CSharpCodeProvider();
            var parameters = new CompilerParameters( new[] { "mscorlib.dll", "System.Core.dll", "netstandard.dll" }, "foo.dll", true);            
            parameters.ReferencedAssemblies.Add("CsvHelper.dll");
            parameters.ReferencedAssemblies.Add("DataAnalyticsPlatform.Shared.dll");
            parameters.GenerateExecutable = false;
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, code);
            return results.CompiledAssembly;
        }

        
    }
}
