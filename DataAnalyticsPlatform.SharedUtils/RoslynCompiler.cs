/*
    RoslynCompiler class provides functionality for dynamically compiling C# code using Roslyn compiler.
    It allows parsing C# source code into syntax trees and generating dynamic assemblies at runtime.

    Features:
    - Constructor initializes necessary properties and settings for compilation.
    - Provides methods for parsing C# source code into SyntaxTree objects.
    - Generates dynamic assemblies from parsed syntax trees with specified compilation options and references.
    - Handles compilation errors and diagnostics, logging them to the console.
    - Uses AssemblyLoadContext for loading assemblies from memory streams and retrieving generated types.

    Note:
    - Dependencies include Microsoft.CodeAnalysis and related namespaces for Roslyn compiler services.
    - Uses CSharpCompilationOptions for configuring compilation settings (e.g., output kind, optimization).
    - Supports loading assemblies from trusted platform assemblies based on needed assemblies list.

    Usage:
    - Instantiate RoslynCompiler to compile C# code dynamically.
    - Use Parse method to parse C# source text into SyntaxTree.
    - Use Generate method to compile the parsed syntax tree into a dynamic assembly and retrieve generated types.
*/


using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
namespace DataAnalyticsPlatform.SharedUtils
{
    public class RoslynCompiler
    {
        public Type myType { get; set; }
        public object AssemblyLoadContext { get; private set; }

        public RoslynCompiler()
        {

        }

        public void init()
        {

        }



        public SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }
        public Type[] Generate(string source, string[] references = null, string dllName = "")
        {

            IEnumerable<string> DefaultNamespaces = new[]
          {
                "System",
                "System.IO",
                "System.Net",
                "System.Linq",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Collections.Generic",
                "System.Collections",
               "DataAnalyticsPlatform.Common",
                "DataAnalyticsPlatform.Shared",
               "CsvHelper.Configuration"
           };

            string runtimePath = RuntimeEnvironment.GetRuntimeDirectory() + @"{0}.dll";




            Console.WriteLine(" sharedutils " + runtimePath);
            var coreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            Console.WriteLine(" sharedutils coredir" + coreDir);
            string projDir = System.Environment.CurrentDirectory;
            IEnumerable<MetadataReference> DefaultReferences = null;
            List<MetadataReference> DefaultReferences1 = new List<MetadataReference>();
            try
            {
                // if (projDir.Contains("Debug") == false)//lot o fhack will fix
                {
                    var neededAssemblies = new[]
                    {
                        "mscorlib.dll",
                        "System.dll",
                        "System.Runtime.dll",
                        "System.Core.dll",
                        "System.Linq.Expressions.dll",
                         "System.Collections.dll",
                         "CsvHelper.dll",
                         "DataAnalyticsPlatform.Common.dll",
                         "DataAnalyticsPlatform.Shared.dll",
                         "netstandard.dll",
                         "System.Private.CoreLib.dll",
                         "System.Text.RegularExpressions.dll"

                    };

                    Console.WriteLine(" sharedutils debug" + projDir);

                    var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
                    foreach (var pathName in trustedAssembliesPaths)
                    {
                        if (neededAssemblies.Any(s => pathName.Contains(s)) == true)
                        {
                            DefaultReferences1.Add(MetadataReference.CreateFromFile(pathName));
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in compile");
            }

            CSharpCompilationOptions DefaultCompilationOptions;
            Console.WriteLine(" sharedutils  compile");

            DefaultCompilationOptions =
           new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                   .WithOverflowChecks(true)
                   .WithOptimizationLevel(OptimizationLevel.Release)
                   .WithUsings(DefaultNamespaces);
            Console.WriteLine(" sharedutils  compile2");
            string dllout = @"c:\temp\" + dllName + ".dll";
            Console.WriteLine(" sharedutils  compile 3");
            var parsedSyntaxTree = Parse(source, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7));
            var compilation
                = CSharpCompilation.Create(dllName + Guid.NewGuid().ToString(), syntaxTrees: new SyntaxTree[] { parsedSyntaxTree }, references: DefaultReferences1.ToArray(), options: DefaultCompilationOptions);//DefaultReferences, DefaultCompilationOptions);
            Console.WriteLine(" sharedutils  compile 4");
            try
            {
                var stream = new MemoryStream();
                var emitResult = compilation.Emit(stream);
                var diagnostics = emitResult.Diagnostics;
                string diagStr = string.Empty;
                foreach (var diag in diagnostics)
                {
                    diagStr += diag.DefaultSeverity.ToString() + " " + diag.Descriptor.Description.ToString() + " " + diag.Severity.ToString() + " " + diag.Location.GetLineSpan().StartLinePosition.Line.ToString() + " " + diag.GetMessage() + "\n";
                }
                Console.WriteLine(" sharedutils  compile 5 " + diagStr);
                if (emitResult.Success)
                {
                    Console.WriteLine(" sharedutils  compile 6");
                    // var assembly = Assembly.LoadFile(@"c:\temp\DataAnalyticsPlatform.ModelGen.dll");// stream.ToArray());
                    stream.Seek(0, SeekOrigin.Begin);
                    AssemblyLoadContext context = System.Runtime.Loader.AssemblyLoadContext.Default;
                    Assembly assembly = context.LoadFromStream(stream);
                    Type[] types = assembly.GetTypes();
                    Console.WriteLine(" sharedutils  compile 7");
                    return types;
                    // Assembly assembly = Assembly.Load(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

    }
}
