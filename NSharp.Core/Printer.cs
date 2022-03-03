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

public partial class Break
{
    public override void Print(int indent) => WriteLineIndented(indent, "Break");
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
    public override void Print(int indent) => WriteLineIndented(indent, $"Comment: {Value}");
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

public partial class FunctionDefinition
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"Function: {string.Join(" ", Modifiers)} {ReturnType} {Name}()");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
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

public partial class LiteralToken
{
    public override string ToString() => Token.ToString();
}

public partial class Property
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Property: {Name}");
}

public partial class Space
{
    public override void Print(int indent) => WriteLineIndented(indent, $"Space: {Size}");
}

public partial class Void
{
    public override string ToString() => "Void";
}

public partial class While
{
    public override void Print(int indent)
    {
        WriteLineIndented(indent, $"While {Condition}");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}