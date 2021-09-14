using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace BrightChain.Engine.Roslyn
{
    public class Compiler
    {
        private static readonly IEnumerable<string> DefaultNamespaces =
            new[]
            {
                "System",
                "System.IO",
                "System.Net",
                "System.Linq",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Collections.Generic",
            };

        private static readonly string runtimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1\{0}.dll";

        private static readonly IEnumerable<MetadataReference> DefaultReferences =
            new[]
            {
                MetadataReference.CreateFromFile(string.Format(runtimePath, "mscorlib")),
                MetadataReference.CreateFromFile(string.Format(runtimePath, "System")),
                MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Core"))
            };

        private static readonly CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release)
                    .WithUsings(DefaultNamespaces);

        public static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }

        static void Main(string[] args)
        {
            var fileToCompile = @"C:\Users\DesktopHome\Documents\Visual Studio 2013\Projects\ConsoleForEverything\SignalR_Everything\Program.cs";
            var source = File.ReadAllText(fileToCompile);
            var parsedSyntaxTree = Parse(source, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));

            //var syntaxTree = CSharpSyntaxTree.ParseText(source);

            //CSharpCompilation compilation = CSharpCompilation.Create(
            //    "assemblyName",
            //    new[] { syntaxTree },
            //    new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            //    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            var compilation
                = CSharpCompilation.Create(
                    "Test.dll",
                    new SyntaxTree[] { parsedSyntaxTree },
                    DefaultReferences,
                    DefaultCompilationOptions);
            try
            {
                using (var dllStream = new MemoryStream())
                using (var pdbStream = new MemoryStream())
                {
                    var emitResult = compilation.Emit(dllStream, pdbStream);
                    Console.WriteLine(emitResult.Success ? "Sucess!!" : "Failed");
                    if (!emitResult.Success)
                    {
                        // emitResult.Diagnostics
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.Read();
        }
    }
}
