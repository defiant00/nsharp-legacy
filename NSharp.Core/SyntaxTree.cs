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

public class AnonymousFunction : Expression
{
    public Expression? ReturnType { get; set; }
    public List<Parameter> Parameters { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public AnonymousFunction(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Argument : Expression
{
    public string? Name { get; set; }
    public List<ArgumentModifierType> Modifiers { get; set; }
    public Expression Expr { get; set; }

    public Argument(Position position, string? name, List<ArgumentModifierType> modifiers, Expression expr) : base(position)
    {
        Name = name;
        Modifiers = modifiers;
        Expr = expr;
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

public class ArrayLiteral : Expression
{
    public List<Expression> Values { get; set; } = new();

    public ArrayLiteral(Position position) : base(position) { }

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

public class Case : Statement
{
    public Expression Expr { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public Case(Position position, Expression expr) : base(position)
    {
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Catch : Statement
{
    public Expression? Type { get; set; }
    public string? Name { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public Catch(Position position) : base(position) { }

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
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public List<string> GenericNames { get; set; } = new();
    public Expression? Parent { get; set; }
    public List<Expression> Interfaces { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public Class(Position position, List<Expression> modifiers, string name) : base(position)
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

public class Condition : Expression
{
    public Expression Value { get; set; }
    public Expression Result { get; set; }

    public Condition(Position position, Expression value, Expression result) : base(position)
    {
        Value = value;
        Result = result;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Conditional : Expression
{
    public Expression Expr { get; set; }
    public List<Expression> Conditions { get; set; } = new();

    public Conditional(Position position, Expression expr) : base(position)
    {
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Constant : Statement
{
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public Expression? Value { get; set; }

    public Constant(Position position, List<Expression> modifiers, string name, Expression type) : base(position)
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
    public List<string> InitProperties { get; set; } = new();
    public List<Expression> InitValues { get; set; } = new();

    public ConstructorCall(Position position, Expression expr) : base(position)
    {
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ConstructorDefinition : Statement
{
    public List<Expression> Modifiers { get; set; }
    public List<Parameter> Parameters { get; set; } = new();
    public List<Expression> BaseArguments { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public ConstructorDefinition(Position position, List<Expression> modifiers) : base(position)
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
    public List<Expression> Modifiers { get; set; }
    public Expression? ReturnType { get; set; }
    public string Name { get; set; }
    public List<string> GenericNames { get; set; } = new();
    public List<Parameter> Parameters { get; set; } = new();

    public DelegateDefinition(Position position, List<Expression> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Discard : Expression
{
    public Discard(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Enumeration : Statement
{
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public List<EnumerationItem> Values { get; set; } = new();

    public Enumeration(Position position, List<Expression> modifiers, string name) : base(position)
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

public class Field : Statement
{
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public Expression? Value { get; set; }

    public Field(Position position, List<Expression> modifiers, string name, Expression type) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
        Type = type;
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
    public string? Name { get; set; }
    public Expression? Init { get; set; }
    public Expression? Condition { get; set; }
    public Statement? Post { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public For(Position position) : base(position) { }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ForEach : Statement
{
    public string Name { get; set; }
    public Expression Expr { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public ForEach(Position position, string name, Expression expr) : base(position)
    {
        Name = name;
        Expr = expr;
    }

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
    public string? Name { get; set; }
    public Expression? AssignExpr { get; set; }
    public Expression Condition { get; set; }
    public List<Statement> Statements { get; set; } = new();
    public List<Statement> Else { get; set; } = new();

    public If(Position position, Expression condition) : base(position)
    {
        Condition = condition;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class ImplicitConstructorCall : Expression
{
    public List<Expression> Arguments { get; set; } = new();
    public List<string> InitProperties { get; set; } = new();
    public List<Expression> InitValues { get; set; } = new();

    public ImplicitConstructorCall(Position position) : base(position) { }

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

public class Indexer : Expression
{
    public Expression Expr { get; set; }
    public List<Expression> Arguments { get; set; } = new();

    public Indexer(Position position, Expression expr, Expression arg) : base(position)
    {
        Expr = expr;
        Arguments.Add(arg);
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Interface : Statement
{
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public List<string> GenericNames { get; set; } = new();
    public List<Expression> Interfaces { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public Interface(Position position, List<Expression> modifiers, string name) : base(position)
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

public class LocalConstant : Statement
{
    public string Name { get; set; }
    public Expression Type { get; set; }
    public Expression Value { get; set; }

    public LocalConstant(Position position, string name, Expression type, Expression value) : base(position)
    {
        Name = name;
        Type = type;
        Value = value;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class LocalVariable : Statement
{
    public string Name { get; set; }
    public Expression? Type { get; set; }
    public Expression? Value { get; set; }

    public LocalVariable(Position position, string name) : base(position)
    {
        Name = name;
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
    public List<Expression> Modifiers { get; set; }
    public Expression? ReturnType { get; set; }
    public string Name { get; set; }
    public List<string> GenericNames { get; set; } = new();
    public List<Parameter> Parameters { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public MethodDefinition(Position position, List<Expression> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class MethodSignature : Statement
{
    public List<Expression> Modifiers { get; set; }
    public Expression? ReturnType { get; set; }
    public string Name { get; set; }
    public List<string> GenericNames { get; set; } = new();
    public List<Parameter> Parameters { get; set; } = new();

    public MethodSignature(Position position, List<Expression> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Modifier : Expression
{
    public ModifierType Type { get; set; }
    public Expression? Arg { get; set; }

    public Modifier(Position position, ModifierType type) : base(position)
    {
        Type = type;
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

public class Nullable : Expression
{
    public Expression Type { get; set; }

    public Nullable(Position position, Expression type) : base(position)
    {
        Type = type;
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
    public Expression? Type { get; set; }
    public string Name { get; set; }
    public Expression? Value { get; set; }

    public Parameter(Position position, string name) : base(position)
    {
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Property : Statement
{
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public Expression? Value { get; set; }
    public bool Get { get; set; } = true;
    public bool Set { get; set; } = true;
    public string SetParameterName { get; set; } = string.Empty;
    public List<Statement> GetStatements { get; set; } = new();
    public List<Statement> SetStatements { get; set; } = new();

    public Property(Position position, List<Expression> modifiers, string name, Expression type) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
        Type = type;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class PropertySignature : Statement
{
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression Type { get; set; }
    public bool Get { get; set; } = true;
    public bool Set { get; set; } = true;

    public PropertySignature(Position position, List<Expression> modifiers, string name, Expression type) : base(position)
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

public class Struct : Statement
{
    public List<Expression> Modifiers { get; set; }
    public string Name { get; set; }
    public List<string> GenericNames { get; set; } = new();
    public List<Expression> Interfaces { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();

    public Struct(Position position, List<Expression> modifiers, string name) : base(position)
    {
        Modifiers = modifiers;
        Name = name;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Switch : Statement
{
    public string? Name { get; set; }
    public Expression? AssignExpr { get; set; }
    public Expression Expr { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public Switch(Position position, Expression expr) : base(position)
    {
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Throw : Statement
{
    public Expression Expr { get; set; }

    public Throw(Position position, Expression expr) : base(position)
    {
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}

public class Try : Statement
{
    public List<Statement> Statements { get; set; } = new();
    public List<Statement> Catches { get; set; } = new();
    public List<Statement> FinallyStatements { get; set; } = new();

    public Try(Position position) : base(position) { }

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

public class Using : Statement
{
    public string Name { get; set; }
    public Expression Expr { get; set; }
    public List<Statement> Statements { get; set; } = new();

    public Using(Position position, string name, Expression expr) : base(position)
    {
        Name = name;
        Expr = expr;
    }

    public override void Accept(ISyntaxTreeVisitor visitor) => visitor.Visit(this);
}
