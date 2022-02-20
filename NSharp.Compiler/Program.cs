namespace NSharp.Compiler;

// using System.Text.Json;
using NSharp.Core.Ast;
using NSharp.Language;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("N# Compiler v0.1.0");

        string file = "test.ns.json";
        var ast = new File
        {
            Name = file,
            Statements = new List<Statement>
            {
                new Class
                {
                    Name = "TestClass",
                    Statements = new List<Statement>
                    {
                        new FunctionDefinition
                        {
                            Name = "TestFunc",
                            Statements = new List<Statement>
                            {
                                new ExpressionStatement
                                {
                                    Expression = new CurrentObjectInstance{},
                                },
                                new Space {Size = 3},
                                new Comment {Content = "This is a comment!"},
                                new ExpressionStatement
                                {
                                    Expression = new CurrentObjectInstance{},
                                },
                            },
                        },
                    },
                },
            },
        };

        System.IO.File.WriteAllText("test.txt", CLang.Process(ast));

        // System.IO.File.WriteAllText(file, JsonSerializer.Serialize(ast, new JsonSerializerOptions { WriteIndented = true }));
    }
}