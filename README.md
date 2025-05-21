# In-Memory Compiled Code

This project demonstrates how to dynamically compile and execute C# code in memory using [Roslyn](https://github.com/dotnet/roslyn) (`Microsoft.CodeAnalysis.CSharp`). It targets .NET 9.0.

## Features

- Compiles C# source code from a string at runtime
- Loads the compiled assembly directly from memory
- Invokes the entry point (`Main` method) of the dynamically compiled code via reflection

## Usage

1. **Build**
   ```sh
   dotnet build
   ```

2. **Run**
    ```sh
    dotnet run
    ```