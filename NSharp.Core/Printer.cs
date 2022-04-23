namespace NSharp.Core.Ast;

public abstract partial class AstItem
{
    protected static void WriteLineIndented(int indent, string line)
    {
        for (int i = 0; i < indent; i++)
            Console.Write("  ");
        Console.WriteLine(line);
    }

    public virtual void Print(int indent) => WriteLineIndented(indent, $"[{this}]");
}

public partial class Accessor
{
    public override string ToString() => $"{Expr}[{string.Join(", ", Arguments)}]";
}

public partial class Array
{
    public override string ToString() => $"[]{Type}";
}

public partial class Assignment
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Assignment: {Left} Assign.{Operator} {Right}");
}

public partial class BinaryOperator
{
    public override string ToString() => $"({Left} {Operator} {Right})";
}

public partial class Break
{
    public override void Print(int indent) => WriteLineIndented(indent, "Break");
}

public partial class Character
{
    public override string ToString() => $"'{Value}'";
}

public partial class Class
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"Class: {string.Join(" ", Modifiers)} {Name}{(Parent != null ? " inherits " + Parent : "")}{(Interfaces.Count > 0 ? " implements " : "")}{string.Join(", ", Interfaces)}");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}

public partial class Comment
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Comment{(IsDocumentation ? " (doc)" : "")}: {Value}");
}

public partial class Constant
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Constant: {string.Join(" ", Modifiers)} {Type} {Name}{(Value != null ? " = " + Value : "")}");
}

public partial class ConstructorDefinition
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"Constructor: {string.Join(" ", Modifiers)} .ctor({string.Join(", ", Parameters)})");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}

public partial class Continue
{
    public override void Print(int indent) => WriteLineIndented(indent, "Continue");
}

public partial class CurrentObjectInstance
{
    public override string ToString() => "[this]";
}

public partial class ErrorExpression
{
    public override string ToString() => $"Error: {Value}";
}

public partial class ErrorStatement
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Error: {Value}");
}

public partial class ExpressionStatement
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Expression Statement: {Expression}");
}

public partial class File
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"File: {Name}");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}

public partial class For
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, "For");
        if (Init != null)
        {
            WriteLineIndented(indent, "Init");
            Init.Print(indent + 1);
        }
        if (Condition != null)
            WriteLineIndented(indent, $"Condition: {Condition}");
        if (Post != null)
        {
            WriteLineIndented(indent, "Post");
            Post.Print(indent + 1);
        }
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}

public partial class Generic
{
    public override string ToString() => $"{Expr}{{{string.Join(", ", Arguments)}}}";
}

public partial class Identifier
{
    public override string ToString() => Value;
}

public partial class If
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"If {Condition}");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
        if (Else != null)
        {
            WriteLineIndented(indent, "Else");
            foreach (var statement in Else)
                statement.Print(indent + 1);
        }
    }
}

public partial class Import
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Import: {string.Join(".", Value)}");
}

public partial class LiteralToken
{
    public override string ToString() => Token.ToString();
}

public partial class MethodCall
{
    public override string ToString() => $"{Expr}({string.Join(", ", Arguments)})";
}

public partial class MethodDefinition
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"Method: {string.Join(" ", Modifiers)} {ReturnType?.ToString() ?? "void"} {Name}({string.Join(", ", Parameters)})");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}

public partial class Namespace
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Namespace: {string.Join(".", Value)}");
}

public partial class Number
{
    public override string ToString() => Value;
}

public partial class Parameter
{
    public override string ToString() => $"{Type} {Name}";
}

public partial class Property
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"Property: {string.Join(" ", Modifiers)} {Type} {Name}{(Get ? " get" : "")}{(Set ? " set" : "")}{(Value != null ? " = " + Value.ToString() : "")}");
        if (GetStatements.Count > 0)
        {
            WriteLineIndented(indent + 1, "get");
            foreach (var statement in GetStatements)
                statement.Print(indent + 2);
        }
        if (SetStatements.Count > 0)
        {
            WriteLineIndented(indent + 1, $"set({SetParameterName})");
            foreach (var statement in SetStatements)
                statement.Print(indent + 2);
        }
    }
}

public partial class Return
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Return: {Value}");
}

public partial class Space
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Space: {Size}");
}

public partial class String
{
    public override string ToString() => "\"" +
        string.Join(Environment.NewLine, Lines.Select(line =>
            string.Join("", line.Select(item =>
                item is StringLiteral ? item.ToString() : "{" + item + "}")))) + "\"";
}

public partial class StringLiteral
{
    public override string ToString() => Value;
}

public partial class Variable
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Variable: {string.Join(" ", Modifiers)} {Type} {Name}{(Value != null ? " = " + Value : "")}");
}
