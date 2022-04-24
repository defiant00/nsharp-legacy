namespace NSharp.Core.Ast;

public interface ISyntaxTreeItem
{
    public Position Position { get; set; }
}

public abstract class Expression : ISyntaxTreeItem
{
    public Position Position { get; set; }

    public Expression(Position position) => Position = position;

    public virtual void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public abstract class Statement : ISyntaxTreeItem
{
    public Position Position { get; set; }

    public Statement(Position position) => Position = position;

    public virtual void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Accessor : Expression
{
    public Expression Expr { get; set; }
    public List<Expression> Arguments { get; set; } = new();

    public Accessor(Position position, Expression expr, Expression arg) : base(position)
    {
        Expr = expr;
        Arguments.Add(arg);
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Array : Expression
{
    public Expression Type { get; set; }

    public Array(Position position, Expression type) : base(position)
    {
        Type = type;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Assignment : Statement
{
    public AssignmentOperator Operator { get; set; }
    public Expression Left { get; set; }
    public Expression Right { get; set; }

    public Assignment(Position position, AssignmentOperator op, Expression left, Expression right) : base(position)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class BinaryOperator : Expression
{
    public BinaryOperatorType Operator { get; set; }
    public Expression Left { get; set; }
    public Expression Right { get; set; }

    public BinaryOperator(Position position, BinaryOperatorType op, Expression left, Expression right) : base(position)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Break : Statement
{
    public Break(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Character : Expression
{
    public string Value { get; set; }

    public Character(Position position, string value) : base(position)
    {
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Class : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression? Parent { get; set; }
    public List<Expression> Interfaces { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public Class(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Comment : Statement
{
    public string Value { get; set; }
    public bool IsDocumentation { get; set; }

    public Comment(Position position, string value) : base(position)
    {
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Constant : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public Expression? Value { get; set; }

    public Constant(Position position, List<Modifier> modifiers, string name, Expression type) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
        Type = type;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ConstructorCall : Expression
{
    public Expression Expr { get; set; }
    public List<Expression> Arguments { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public ConstructorCall(Position position, Expression expr) : base(position)
    {
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ConstructorDefinition : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public List<Parameter> Parameters { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public ConstructorDefinition(Position position, List<Modifier> modifiers) : base(position)
    {
        Modifiers = modifiers;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Continue : Statement
{
    public Continue(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class CurrentObjectInstance : Expression
{
    public CurrentObjectInstance(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class DelegateDefinition : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public Expression? ReturnType { get; set; }
    public string Name { get; set; }
    public List<Parameter> Parameters { get; set; } = new();

    public DelegateDefinition(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Enumeration : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public List<EnumerationItem> Values { get; set; } = new();

    public Enumeration(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class EnumerationItem : Statement
{
    public string Name { get; set; }
    public int? Value { get; set; }
    public Comment? Comment { get; set; }

    public EnumerationItem(Position position, string name) : base(position)
    {
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ErrorExpression : Expression
{
    public string Value { get; set; }

    public ErrorExpression(Position position, string value) : base(position)
    {
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ErrorStatement : Statement
{
    public string Value { get; set; }

    public ErrorStatement(Position position, string value) : base(position)
    {
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ExpressionStatement : Statement
{
    public Expression Expression { get; set; }

    public ExpressionStatement(Position position, Expression expression) : base(position)
    {
        Expression = expression;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class File : Statement
{
    public string Name { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public File(string name) : base(new Position())
    {
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class For : Statement
{
    public Statement? Init { get; set; }
    public Expression? Condition { get; set; }
    public Statement? Post { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public For(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Generic : Expression
{
    public Expression Expr { get; set; }
    public List<Expression> Arguments { get; set; } = new();

    public Generic(Position position, Expression expr, Expression arg) : base(position)
    {
        Expr = expr;
        Arguments.Add(arg);
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Identifier : Expression
{
    public string Value { get; set; }

    public Identifier(Position position, string value) : base(position)
    {
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class If : Statement
{
    public Expression Condition { get; set; }
    public List<Statement> Statements { get; set; } = new();
    public List<Statement>? Else { get; set; }

    public If(Position position, Expression condition) : base(position)
    {
        Condition = condition;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Import : Statement
{
    public List<string> Value { get; set; } = new();

    public Import(Position position, string value) : base(position)
    {
        Value.Add(value);
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Interface : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public List<Expression> Interfaces { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public Interface(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Is : Expression
{
    public Expression Expr { get; set; }
    public Expression Type { get; set; }
    public string? Name { get; set; }

    public Is(Position position, Expression expr, Expression type) : base(position)
    {
        Expr = expr;
        Type = type;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class LiteralToken : Expression
{
    public Literal Token { get; set; }

    public LiteralToken(Position position, Literal token) : base(position)
    {
        Token = token;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class MethodCall : Expression
{
    public Expression Expr { get; set; }
    public List<Expression> Arguments { get; set; } = new();

    public MethodCall(Position position, Expression expr) : base(position)
    {
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class MethodDefinition : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public Expression? ReturnType { get; set; }
    public string Name { get; set; }
    public List<Parameter> Parameters { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public MethodDefinition(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class MethodSignature : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public Expression? ReturnType { get; set; }
    public string Name { get; set; }
    public List<Parameter> Parameters { get; set; } = new();

    public MethodSignature(Position position, List<Modifier> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Namespace : Statement
{
    public List<string> Value { get; set; } = new();

    public Namespace(Position position, string value) : base(position)
    {
        Value.Add(value);
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Number : Expression
{
    public string Value { get; set; }

    public Number(Position position, string value) : base(position)
    {
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Parameter : Expression
{
    public Expression Type { get; set; }
    public string Name { get; set; }

    public Parameter(Position position, Expression type, string name) : base(position)
    {
        Type = type;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Property : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public Expression? Value { get; set; }
    public bool Get { get; set; } = true;
    public bool Set { get; set; } = true;
    public string SetParameterName { get; set; } = string.Empty;
    public List<Statement> GetStatements { get; set; } = new();
    public List<Statement> SetStatements { get; set; } = new();

    public Property(Position position, List<Modifier> modifiers, string name, Expression type) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
        Type = type;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class PropertySignature : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public bool Get { get; set; } = true;
    public bool Set { get; set; } = true;

    public PropertySignature(Position position, List<Modifier> modifiers, string name, Expression type) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
        Type = type;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Return : Statement
{
    public Expression? Value;

    public Return(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Space : Statement
{
    public int Size { get; set; }

    public Space(Position position, int size) : base(position)
    {
        Size = size;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class String : Expression
{
    public List<List<Expression>> Lines { get; set; } = new();

    public String(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class StringLiteral : Expression
{
    public string Value { get; set; }

    public StringLiteral(Position position, string value) : base(position)
    {
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class UnaryOperator : Expression
{
    public UnaryOperatorType Operator { get; set; }
    public Expression Expr { get; set; }

    public UnaryOperator(Position position, UnaryOperatorType op, Expression expr) : base(position)
    {
        Operator = op;
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Variable : Statement
{
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public Expression? Value { get; set; }

    public Variable(Position position, List<Modifier> modifiers, string name, Expression type) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
        Type = type;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}
