// Import necessary namespaces for reflection and Roslyn
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace InMemoryCompiledCode;

// This program demonstrates how to compile C# code at runtime using Roslyn
// and execute it using reflection.

public static class Program
{
    public static void Main()
    {
        // The C# source code to be compiled at runtime (as a string)
        string sourceCode =
            @"
                using System;
                public class Script
                {
                    public static void Main()
                    {
                        Console.WriteLine(""Hello from dynamically compiled code!"");
                    }
                }
                ";

        // Parse the source code into a Roslyn syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Get the runtime directory to locate necessary assemblies
        var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

        // Prepare metadata references required for compilation
        var references = new MetadataReference[]
        {
            // Reference to mscorlib (contains System.Object)
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            // Reference to System.Console
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            // Reference to System.Runtime (required for .NET types)
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
        };

        // Create the Roslyn compilation
        var compilation = CSharpCompilation.Create(
            assemblyName: "InMemoryAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

        // Emit the compiled assembly into a memory stream
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        // Check for compilation errors
        if (!result.Success)
        {
            // Print out each diagnostic message
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.WriteLine(diagnostic.ToString());
            }
            return;
        }

        // Load the compiled assembly from the memory stream
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        // Call Script.Main() via reflection
        var type = assembly.GetType("Script");
        var method = type?.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
        method?.Invoke(null, null);
    }
}
