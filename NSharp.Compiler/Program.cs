namespace NSharp.Compiler;

// using System.Text.Json;
using NSharp.Core;
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
                    Block = new Block
                    {
                        Statements = new List<Statement>
                        {
                            new FunctionDefinition
                            {
                                Name = "TestFunc",
                                Block = new Block
                                {
                                    Statements = new List<Statement>
                                    {
                                        new ExpressionStatement
                                        {
                                            Expression = new CurrentObjectInstance{},
                                        },
                                        new Space {Size = 1},
                                        new Block
                                        {
                                            Statements = new List<Statement>
                                            {
                                                new If
                                                {
                                                    Condition = new LiteralToken { Token = Token.False },
                                                    Block = new Block
                                                    {
                                                        Statements = new List<Statement>
                                                        {
                                                            new Break{},
                                                        }
                                                    },
                                                },
                                                new Comment {Content = "While loop!"},
                                                new Loop{},
                                            },
                                        },
                                        new Space {Size = 1},
                                        new Block
                                        {
                                            Statements = new List<Statement>
                                            {
                                                new Comment {Content = "Do While loop!"},
                                                new If
                                                {
                                                    Condition = new LiteralToken { Token = Token.True },
                                                    Block = new Block
                                                    {
                                                        Statements = new List<Statement>
                                                        {
                                                            new Loop{},
                                                        }
                                                    },
                                                },
                                            },
                                        },
                                    },
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