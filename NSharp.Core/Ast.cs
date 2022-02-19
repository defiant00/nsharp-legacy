namespace NSharp.Core.Ast;

public abstract class AstItem
{
    public Position Position { get; set; }
}

public abstract class Expression : AstItem { }

public abstract class Statement : AstItem { }

public class Class : Statement
{
    public string? Name { get; set; }
    public List<Statement>? Statements { get; set; }
}

public class Comment : Statement
{
    public string? Content { get; set; }
}

public class CurrentObjectInstance : Expression { }

public class ExpressionStatement : Statement
{
    public Expression? Expression { get; set; }
}

public class File : Statement
{
    public string? Name { get; set; }
    public List<Statement>? Statements { get; set; }
}

public class FunctionDefinition : Statement
{
    public string? Name { get; set; }
    public List<Statement>? Statements { get; set; }
}

public class Property : Statement
{
    public string? Name { get; set; }
}

public class Space : Statement
{
    public int Size { get; set; }
}