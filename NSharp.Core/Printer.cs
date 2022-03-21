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
        WriteLineIndented(indent, $"Class: {string.Join(" ", Modifiers)} {Name}");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}

public partial class Comment
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Comment{(IsDocumentation ? " (doc)" : "")}: {Value}");
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

public partial class FunctionDefinition
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"Function: {string.Join(" ", Modifiers)} {ReturnType?.ToString() ?? "void"} {Name}({string.Join(", ", Parameters)})");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}

public partial class Identifier
{
    public override string ToString() => string.Join(".", Parts);
}

public partial class IdentifierPart
{
    public override string ToString() => (Types != null && Types.Count > 0) ? $"{Value}<{string.Join(", ", Types)}>" : Value;
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
    public override void Print(int indent) => WriteLineIndented(indent, $"Import: {Value}");
}

public partial class LiteralToken
{
    public override string ToString() => Token.ToString();
}

public partial class Namespace
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Namespace: {Name}");
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
    public override void Print(int indent) => WriteLineIndented(indent, $"Property: {string.Join(" ", Modifiers)} {Type} {Name}");
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
