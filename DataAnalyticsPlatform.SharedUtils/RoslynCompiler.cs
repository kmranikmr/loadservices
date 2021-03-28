
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using Microsoft.CSharp;
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
                    foreach ( var pathName in trustedAssembliesPaths)
                    {
                        if (neededAssemblies.Any(s => pathName.Contains(s)) == true)
                        {
                           DefaultReferences1.Add(MetadataReference.CreateFromFile(pathName));
                        }
                    }

                    
                    //                DefaultReferences = trustedAssembliesPaths
    //.Where(p => neededAssemblies.Contains(Path.GetFileNameWithoutExtension(p)))
    //.Select(p => MetadataReference.CreateFromFile(p))
    //.ToList();
                 //   DefaultReferences.MetadataReference.CreateFromFile(Path.Combine(projDir + @"\bin\debug", "DataAnalyticsPlatform.Common.dll")));//@"E:\source\growap\DataAnalyticsPlatform\bin\netstandard2.0\DataAnalyticsPlatform.Common.dll")
                 //   DefaultReferences.ToList().Add(MetadataReference.CreateFromFile(Path.Combine(projDir + @"\bin\debug" , "DataAnalyticsPlatform.Shared.dll")));
                    //                DefaultReferences =
                    //DependencyContext.Default.CompileLibraries
                    //.First(cl => cl.Name == "Microsoft.NETCore.App")
                    //.ResolveReferencePaths()
                    //.Select(asm => MetadataReference.CreateFromFile(asm))
                    //.ToArray();

                    foreach ( MetadataReference meta in DefaultReferences1)
                    {
                        Console.WriteLine(meta.Display);
                    }
                    //      DefaultReferences =
                    //       new[]
                    //       {


                    //  MetadataReference.CreateFromFile(Path.Combine(projDir, "mscorlib")),
                    //  MetadataReference.CreateFromFile(Path.Combine(projDir, "System")),
                    //  MetadataReference.CreateFromFile(Path.Combine(projDir, "System.Core")),
                    //  MetadataReference.CreateFromFile(Path.Combine(projDir, "System.Linq.Expressions")),
                    //  MetadataReference.CreateFromFile(Path.Combine(projDir, "System.Runtime")),
                    //   MetadataReference.CreateFromFile(Path.Combine(projDir, "System.Collections")),
                    //  MetadataReference.CreateFromFile(Path.Combine(projDir, "System.Private.CoreLib.dll")),


                    ////  MetadataReference.CreateFromFile(Path.Combine(projDir + @"\bin\Debug", "CsvHelper.dll")),//@"E:\source\growap\DataAnalyticsPlatform\packages\CsvHelper.12.1.2\lib\net45\CsvHelper.dll"),
                    // // MetadataReference.CreateFromFile(Path.Combine(projDir + @"\bin\Debug", "DataAnalyticsPlatform.Common.dll")),//@"E:\source\growap\DataAnalyticsPlatform\bin\netstandard2.0\DataAnalyticsPlatform.Common.dll")
                    // //  MetadataReference.CreateFromFile(Path.Combine(projDir + @"\bin\Debug", "DataAnalyticsPlatform.Shared.dll"))//@"E:\source\growap\DataAnalyticsPlatform\bin\netstandard2.0\DataAnalyticsPlatform.Common.dll")

                    //     MetadataReference.CreateFromFile(Path.Combine(projDir, "CsvHelper.dll")),//@"E:\source\growap\DataAnalyticsPlatform\packages\CsvHelper.12.1.2\lib\net45\CsvHelper.dll"),
                    //  MetadataReference.CreateFromFile(Path.Combine(projDir, "DataAnalyticsPlatform.Common.dll")),//@"E:\source\growap\DataAnalyticsPlatform\bin\netstandard2.0\DataAnalyticsPlatform.Common.dll")
                    //   MetadataReference.CreateFromFile(Path.Combine(projDir, "DataAnalyticsPlatform.Shared.dll"))//@"E:\source\growap\DataAnalyticsPlatform\bin\netstandard2.0\DataAnalyticsPlatform.Common.dll")

                    //       };
                }
           
            //else
            //{
            //    Console.WriteLine(" sharedutils not debug" + projDir);
            //    DefaultReferences =
            //     new[]
            //     {
            //         // MetadataReference.CreateFromFile(@"E:\source\growap\DataAnalyticsPlatform\bin\DataAnalyticsPlatform.Shared.dll"),


            //    MetadataReference.CreateFromFile(string.Format(runtimePath, "mscorlib")),
            //    MetadataReference.CreateFromFile(string.Format(runtimePath, "System")),
            //    MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Core")),
            //    MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Linq.Expressions")),
            //    MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Runtime")),
            //     MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Collections")),
            //    MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Private.CoreLib.dll")),


            //  //  MetadataReference.CreateFromFile(Path.Combine(projDir, "CsvHelper.dll")),//@"E:\source\growap\DataAnalyticsPlatform\packages\CsvHelper.12.1.2\lib\net45\CsvHelper.dll"),
            // //   MetadataReference.CreateFromFile(Path.Combine(projDir, "DataAnalyticsPlatform.Common.dll")),//@"E:\source\growap\DataAnalyticsPlatform\bin\netstandard2.0\DataAnalyticsPlatform.Common.dll")
            ////     MetadataReference.CreateFromFile(Path.Combine(projDir , "DataAnalyticsPlatform.Shared.dll"))//@"E:\source\growap\DataAnalyticsPlatform\bin\netstandard2.0\DataAnalyticsPlatform.Common.dll")

            //     };
            //}

            }
            catch (Exception ex)
            {
                int gg = 0;
            }
            //if (references != null)
            //{
            //    foreach (string reference in references)
            //    {
            //        string dir = Path.Combine(
            //             Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), reference);
            //        DefaultReferences.ToList().Add(MetadataReference.CreateFromFile(dir));
            //        // MetadataReference.CreateFromFile(@"E:\source\tester\in\Repo.dll")
            //    }
            //}
            CSharpCompilationOptions DefaultCompilationOptions;
            Console.WriteLine(" sharedutils  compile");

            DefaultCompilationOptions =
           new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                   .WithOverflowChecks(true)
                   .WithOptimizationLevel(OptimizationLevel.Release)
                   .WithUsings(DefaultNamespaces);
            Console.WriteLine(" sharedutils  compile2" );
            string dllout = @"c:\temp\" + dllName + ".dll";
            Console.WriteLine(" sharedutils  compile 3");
            var parsedSyntaxTree = Parse(source, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7));
            var compilation
                = CSharpCompilation.Create(dllName+Guid.NewGuid().ToString(), syntaxTrees : new SyntaxTree[] { parsedSyntaxTree }, references:  DefaultReferences1.ToArray() , options: DefaultCompilationOptions);//DefaultReferences, DefaultCompilationOptions);
            Console.WriteLine(" sharedutils  compile 4");
            try
            {
                var stream = new MemoryStream();
                var emitResult = compilation.Emit(stream);
                var diagnostics = emitResult.Diagnostics;
                string diagStr = string.Empty;
                foreach ( var diag in diagnostics )
                {
                    diagStr += diag.DefaultSeverity.ToString() + " " + diag.Descriptor.Description.ToString() + " " + diag.Severity.ToString()+ " " + diag.Location.GetLineSpan().StartLinePosition.Line.ToString() + " "+ diag.GetMessage() +"\n";
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
