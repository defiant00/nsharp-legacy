namespace NSharp.Core.Ast;

public abstract partial class AstItem
{
    public Position Position { get; set; }

    public AstItem(Position position) => Position = position;
}

public abstract class Expression : AstItem
{
    public Expression(Position position) : base(position) { }
}

public abstract class Statement : AstItem
{
    public virtual bool IsCode => true;

    public Statement(Position position) : base(position) { }
}

public partial class Break : Statement
{
    public Break(Position position) : base(position) { }
}

public partial class Class : Statement
{
    public List<Token> Modifiers { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public List<Statement> Statements { get; set; } = new();

    public Class(Position position) : base(position) { }
}

public partial class Comment : Statement
{
    public string Value { get; set; } = string.Empty;
    public override bool IsCode => false;

    public Comment(Position position) : base(position) { }
}

public partial class Continue : Statement
{
    public Continue(Position position) : base(position) { }
}

public partial class CurrentObjectInstance : Expression
{
    public CurrentObjectInstance(Position position) : base(position) { }
}

public partial class ErrorExpression : Expression
{
    public string Value { get; set; } = string.Empty;

    public ErrorExpression(Position position) : base(position) { }
}

public partial class ErrorStatement : Statement
{
    public string Value { get; set; } = string.Empty;
    public override bool IsCode => false;

    public ErrorStatement(Position position) : base(position) { }
}

public partial class ExpressionStatement : Statement
{
    public Expression? Expression { get; set; }

    public ExpressionStatement(Position position) : base(position) { }
}

public partial class File : Statement
{
    public string Name { get; set; } = string.Empty;
    public List<Statement> Statements { get; set; } = new();

    public File() : base(new Position()) { }
}

public partial class FunctionDefinition : Statement
{
    public List<Token> Modifiers { get; set; } = new();
    public Expression? ReturnType { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Statement> Statements { get; set; } = new();

    public FunctionDefinition(Position position) : base(position) { }
}

public partial class Identifier : Expression
{
    public string Value { get; set; } = string.Empty;

    public Identifier(Position position) : base(position) { }
}

public partial class If : Statement
{
    public Expression? Condition { get; set; }
    public List<Statement> Statements { get; set; } = new();
    public List<Statement>? Else { get; set; }

    public If(Position position) : base(position) { }
}

public partial class LiteralToken : Expression
{
    public Token Token { get; set; }

    public LiteralToken(Position position) : base(position) { }
}

public partial class Loop : Statement
{
    public List<Statement> Statements { get; set; } = new();

    public Loop(Position position) : base(position) { }
}

public partial class Property : Statement
{
    public string Name { get; set; } = string.Empty;

    public Property(Position position) : base(position) { }
}

public partial class Space : Statement
{
    public int Size { get; set; }
    public override bool IsCode => false;

    public Space(Position position) : base(position) { }
}

public partial class Void : Expression
{
    public Void(Position position) : base(position) { }
}