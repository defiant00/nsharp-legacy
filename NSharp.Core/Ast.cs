namespace NSharp.Core.Ast;

public abstract partial class AstItem
{
    public Position Position { get; set; }
}

public abstract class Expression : AstItem { }

public abstract class Statement : AstItem
{
    public virtual bool IsCode => true;
}

public partial class Break : Statement { }

public partial class Class : Statement
{
    public List<Token> Modifiers { get; set; } = new();
    public Identifier Name { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();
}

public partial class Comment : Statement
{
    public string Value { get; set; } = string.Empty;
    public override bool IsCode => false;
}

public partial class Continue : Statement { }

public partial class CurrentObjectInstance : Expression { }

public partial class ErrorExpression : Expression
{
    public string Value { get; set; } = string.Empty;
}

public partial class ErrorStatement : Statement
{
    public string Value { get; set; } = string.Empty;
    public override bool IsCode => false;
}

public partial class ExpressionStatement : Statement
{
    public Expression? Expression { get; set; }
}

public partial class File : Statement
{
    public string Name { get; set; } = string.Empty;
    public List<Statement> Statements { get; set; } = new();
}

public partial class FunctionDefinition : Statement
{
    public Identifier Name { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();
}

public partial class Identifier : Expression
{
    public string Value { get; set; } = string.Empty;
}

public partial class If : Statement
{
    public Expression? Condition { get; set; }
    public List<Statement> Statements { get; set; } = new();
    public List<Statement>? Else { get; set; }
}

public partial class LiteralToken : Expression
{
    public Token Token { get; set; }
}

public partial class Loop : Statement
{
    public List<Statement> Statements { get; set; } = new();
}

public partial class Property : Statement
{
    public string Name { get; set; } = string.Empty;
}

public partial class Space : Statement
{
    public int Size { get; set; }
    public override bool IsCode => false;
}