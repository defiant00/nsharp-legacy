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
    public List<Token> Modifiers { get; set; }
    public string Name { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public Class(Position position, List<Token> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }
}

public partial class Comment : Statement
{
    public string Value { get; set; }
    public override bool IsCode => false;

    public Comment(Position position, string value) : base(position)
    {
        Value = value;
    }
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
    public string Value { get; set; }

    public ErrorExpression(Position position, string value) : base(position)
    {
        Value = value;
    }
}

public partial class ErrorStatement : Statement
{
    public string Value { get; set; }
    public override bool IsCode => false;

    public ErrorStatement(Position position, string value) : base(position)
    {
        Value = value;
    }
}

public partial class ExpressionStatement : Statement
{
    public Expression Expression { get; set; }

    public ExpressionStatement(Position position, Expression expression) : base(position)
    {
        Expression = expression;
    }
}

public partial class File : Statement
{
    public string Name { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public File(string name) : base(new Position())
    {
        Name = name;
    }
}

public partial class FunctionDefinition : Statement
{
    public List<Token> Modifiers { get; set; }
    public Expression ReturnType { get; set; }
    public string Name { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public FunctionDefinition(Position position, List<Token> modifiers, Expression returnType, string name) : base(position)
    {
        Modifiers = modifiers;
        ReturnType = returnType;
        Name = name;
    }
}

public partial class Identifier : Expression
{
    public List<IdentifierPart> Parts { get; set; } = new();

    public Identifier(Position position, string value) : base(position)
    {
        Parts.Add(new IdentifierPart(value));
    }
}

public partial class IdentifierPart
{
    public string Value { get; set; }

    public IdentifierPart(string value)
    {
        Value = value;
    }
}

public partial class If : Statement
{
    public Expression Condition { get; set; }
    public List<Statement> Statements { get; set; } = new();
    public List<Statement>? Else { get; set; }

    public If(Position position, Expression condition) : base(position)
    {
        Condition = condition;
    }
}

public partial class LiteralToken : Expression
{
    public Token Token { get; set; }

    public LiteralToken(Position position, Token token) : base(position)
    {
        Token = token;
    }
}

public partial class Property : Statement
{
    public List<Token> Modifiers { get; set; }
    public Expression Type { get; set; }
    public string Name { get; set; }

    public Property(Position position, List<Token> modifiers, Expression type, string name) : base(position)
    {
        Modifiers = modifiers;
        Type = type;
        Name = name;
    }
}

public partial class Space : Statement
{
    public int Size { get; set; }
    public override bool IsCode => false;

    public Space(Position position, int size) : base(position)
    {
        Size = size;
    }
}

public partial class Void : Expression
{
    public Void(Position position) : base(position) { }
}

public partial class While : Statement
{
    public Expression Condition { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public While(Position position, Expression condition) : base(position)
    {
        Condition = condition;
    }
}