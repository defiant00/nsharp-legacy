namespace NSharp.Core.Ast;

public abstract class AstItem
{
    public Position Position { get; set; }
}

public abstract class Expression : AstItem { }

public abstract class Statement : AstItem { }

public class Block : Statement
{
    public List<Statement> Statements { get; set; } = new();
}

public class Break : Statement { }

public class Class : Statement
{
    public string Name { get; set; } = string.Empty;
    public Block Block { get; set; } = new();
}

public class Comment : Statement
{
    public string Content { get; set; } = string.Empty;
}

public class CurrentObjectInstance : Expression { }

public class ExpressionStatement : Statement
{
    public Expression? Expression { get; set; }
}

public class File : Statement
{
    public string Name { get; set; } = string.Empty;
    public List<Statement> Statements { get; set; } = new();
}

public class FunctionDefinition : Statement
{
    public string Name { get; set; } = string.Empty;
    public Block Block { get; set; } = new();
}

public class If : Statement
{
    public Expression? Condition { get; set; }
    public Block Block { get; set; } = new();
    public Block? Else { get; set; }
}

public class LiteralToken : Expression
{
    public Token Token { get; set; }
}

public class Loop : Statement { }

public class Property : Statement
{
    public string Name { get; set; } = string.Empty;
}

public class Space : Statement
{
    public int Size { get; set; }
}