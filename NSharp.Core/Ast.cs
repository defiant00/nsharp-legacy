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

public partial class BinaryOperator : Expression
{
    public OperatorType Operator { get; set; }
    public Expression Left { get; set; }
    public Expression Right { get; set; }

    public BinaryOperator(Position position, OperatorType op, Expression left, Expression right) : base(position)
    {
        Operator = op;
        Left = left;
        Right = right;
    }
}

public partial class Break : Statement
{
    public Break(Position position) : base(position) { }
}

public partial class Character : Expression
{
    public string Value { get; set; }

    public Character(Position position, string value) : base(position)
    {
        Value = value;
    }
}

public partial class Class : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public Class(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }
}

public partial class Comment : Statement
{
    public string Value { get; set; }
    public bool IsDocumentation { get; set; }

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

public partial class For : Statement
{
    public Statement? Init { get; set; }
    public Expression? Condition { get; set; }
    public Statement? Post { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public For(Position position) : base(position) { }
}

public partial class FunctionDefinition : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public Expression? ReturnType { get; set; }
    public string Name { get; set; }
    public List<Parameter> Parameters { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public FunctionDefinition(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }
}

public partial class Identifier : Expression
{
    public List<IdentifierPart> Parts { get; set; } = new();

    public Identifier(Position position, IdentifierPart firstPart) : base(position)
    {
        Parts.Add(firstPart);
    }
}

public partial class IdentifierPart : Expression
{
    public string Value { get; set; }
    public List<Identifier>? Types { get; set; }

    public IdentifierPart(Position position, string value) : base(position)
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

public partial class Import : Statement
{
    public Identifier Value { get; set; }

    public Import(Position position, Identifier value) : base(position)
    {
        Value = value;
    }
}

public partial class LiteralToken : Expression
{
    public Literal Token { get; set; }

    public LiteralToken(Position position, Literal token) : base(position)
    {
        Token = token;
    }
}

public partial class Namespace : Statement
{
    public Identifier Name { get; set; }

    public Namespace(Position position, Identifier name) : base(position)
    {
        Name = name;
    }
}

public partial class Number : Expression
{
    public string Value { get; set; }

    public Number(Position position, string value) : base(position)
    {
        Value = value;
    }
}

public partial class Parameter : Expression
{
    public Identifier Type { get; set; }
    public string Name { get; set; }

    public Parameter(Position position, Identifier type, string name) : base(position)
    {
        Type = type;
        Name = name;
    }
}

public partial class Property : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public Identifier Type { get; set; }
    public string Name { get; set; }

    public Property(Position position, List<Modifier> modifiers, Identifier type, string name) : base(position)
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

public partial class String : Expression
{
    public List<List<Expression>> Lines { get; set; } = new();

    public String(Position position) : base(position) { }
}

public partial class StringLiteral : Expression
{
    public string Value { get; set; }

    public StringLiteral(Position position, string value) : base(position)
    {
        Value = value;
    }
}