using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace BrightChain.Engine.Roslyn;

public class Compiler
{
    private static readonly IEnumerable<string> DefaultNamespaces =
        new[]
        {
            "System", "System.IO", "System.Net", "System.Linq", "System.Text", "System.Text.RegularExpressions",
            "System.Collections.Generic",
        };

    private static readonly string runtimePath =
        @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1\{0}.dll";

    private static readonly IEnumerable<MetadataReference> DefaultReferences =
        new[]
        {
            MetadataReference.CreateFromFile(path: string.Format(format: runtimePath,
                arg0: "mscorlib")),
            MetadataReference.CreateFromFile(path: string.Format(format: runtimePath,
                arg0: "System")),
            MetadataReference.CreateFromFile(path: string.Format(format: runtimePath,
                arg0: "System.Core")),
        };

    private static readonly CSharpCompilationOptions DefaultCompilationOptions =
        new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary)
            .WithOverflowChecks(enabled: true).WithOptimizationLevel(value: OptimizationLevel.Release)
            .WithUsings(usings: DefaultNamespaces);

    public static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
    {
        var stringText = SourceText.From(text: text,
            encoding: Encoding.UTF8);
        return SyntaxFactory.ParseSyntaxTree(text: stringText,
            options: options,
            path: filename);
    }

    private static void Main(string[] args)
    {
        var fileToCompile =
            @"C:\Users\DesktopHome\Documents\Visual Studio 2013\Projects\ConsoleForEverything\SignalR_Everything\Program.cs";
        var source = File.ReadAllText(path: fileToCompile);
        var parsedSyntaxTree = Parse(text: source,
            filename: "",
            options: CSharpParseOptions.Default.WithLanguageVersion(version: LanguageVersion.CSharp5));

        //var syntaxTree = CSharpSyntaxTree.ParseText(source);

        //CSharpCompilation compilation = CSharpCompilation.Create(
        //    "assemblyName",
        //    new[] { syntaxTree },
        //    new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
        //    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


        var compilation
            = CSharpCompilation.Create(
                assemblyName: "Test.dll",
                syntaxTrees: new[] {parsedSyntaxTree},
                references: DefaultReferences,
                options: DefaultCompilationOptions);
        try
        {
            using (var dllStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(peStream: dllStream,
                    pdbStream: pdbStream);
                Console.WriteLine(value: emitResult.Success ? "Sucess!!" : "Failed");
                if (!emitResult.Success)
                {
                    // emitResult.Diagnostics
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(value: ex);
        }

        Console.Read();
    }
}
