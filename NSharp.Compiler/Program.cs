using System.Text.Json;

namespace NSharp.Compiler;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("N# Compiler v0.1.0");

        string file = "test.ns.json";
        var ast = new Core.Ast.File
        {
            Name = file,
            Statements = new List<Core.Ast.Statement>
            {
                new Core.Ast.Class
                {
                    Name = "TestClass",
                    Statements = new List<Core.Ast.Statement>
                    {
                        new Core.Ast.FunctionDefinition
                        {
                            Name = "TestFunc",
                        },
                    },
                },
            },
        };
        
        File.WriteAllText(file, JsonSerializer.Serialize(ast, new JsonSerializerOptions { WriteIndented = true }));
    }
}