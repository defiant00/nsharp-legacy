namespace NSharp.Core.Ast;

public abstract partial class AstItem
{
    protected static void PrintIndent(int indent)
    {
        for (int i = 0; i < indent; i++)
            Console.Write("  ");
    }

    public virtual void Print(int indent)
    {
        PrintIndent(indent);
        Console.WriteLine($"[{this}]");
    }
}

public partial class Comment
{
    public override void Print(int indent)
    {
        PrintIndent(indent);
        Console.WriteLine($"Comment: {Value}");
    }
}

public partial class File
{
    public override void Print(int indent)
    {
        PrintIndent(indent);
        Console.WriteLine($"File: {Name}");
        foreach (var statement in Statements)
            statement.Print(indent + 1);
    }
}