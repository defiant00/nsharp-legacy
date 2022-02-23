namespace NSharp.Compiler;

// using System.Text.Json;
using NSharp.Core;
using NSharp.Core.Ast;
using NSharp.Language.CLang;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("N# Compiler v0.1.0");

        string file = "test.ns";
        var ast = new File
        {
            Name = file,
            Statements = new List<Statement>
            {
                new Class
                {
                    Name = new Identifier { Value = "TestClass" },
                    Statements = new List<Statement>
                    {
                        new FunctionDefinition
                        {
                            Name = new Identifier { Value = "TestFunc" },
                            Statements = new List<Statement>
                            {
                                new ExpressionStatement
                                {
                                    Expression = new CurrentObjectInstance{},
                                },
                                new Space {Size = 1},
                                new Loop {
                                    Condition = new LiteralToken { Token = Token.True },
                                    Statements = new List<Statement>
                                    {
                                        new Comment {Content = "While loop!"},
                                        new Break{},
                                    },
                                },
                                new Space {Size = 1},
                                new Loop {
                                    Condition = new LiteralToken { Token = Token.True },
                                    ConditionAtEnd = true,
                                    Statements = new List<Statement>
                                    {
                                        new Comment {Content = "Do While loop!"},
                                        new If
                                        {
                                            Condition = new LiteralToken { Token = Token.False },
                                            Statements = new List<Statement>
                                            {
                                                new Comment { Content = "Hi!" },
                                            },
                                        },
                                        new If
                                        {
                                            Condition = new LiteralToken { Token = Token.False },
                                            Statements = new List<Statement>
                                            {
                                                new Continue{},
                                            }
                                        },
                                    },
                                },
                            },
                        },
                        new FunctionDefinition
                        {
                            Name = new Identifier { Value = "true" }
                        },
                    },
                },
            },
        };

        System.IO.File.WriteAllText("test.txt", CLang.Process(ast));

        // System.IO.File.WriteAllText(file, JsonSerializer.Serialize(ast, new JsonSerializerOptions { WriteIndented = true }));
    }
}