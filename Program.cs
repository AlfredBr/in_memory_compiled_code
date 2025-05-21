using System;
using System.IO;
using System.Reflection;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class Program
{
    public static void Main()
    {
        string sourceCode = @"
            using System;
            public class Script
            {
                public static void Main()
                {
                    Console.WriteLine(""Hello from dynamically compiled code!"");
                }
            }
            ";


        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll"))
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "InMemoryAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine(diagnostic.ToString());
            }
            return;
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        // Call Script.Main() via reflection
        var type = assembly.GetType("Script");
        var method = type?.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        method?.Invoke(null, null);
    }
}
