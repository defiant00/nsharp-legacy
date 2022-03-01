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

public class Break : Statement { }

public class Class : Statement
{
    public Identifier Name { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();
}

public partial class Comment : Statement
{
    public string Value { get; set; } = string.Empty;
    public override bool IsCode => false;
}

public class Continue : Statement { }

public class CurrentObjectInstance : Expression { }

public class ErrorExpression : Expression
{
    public string Value { get; set; } = string.Empty;
}

public class ErrorStatement : Statement
{
    public string Value { get; set; } = string.Empty;
    public override bool IsCode => false;
}

public class ExpressionStatement : Statement
{
    public Expression? Expression { get; set; }
}

public partial class File : Statement
{
    public string Name { get; set; } = string.Empty;
    public List<Statement> Statements { get; set; } = new();
}

public class FunctionDefinition : Statement
{
    public Identifier Name { get; set; } = new();
    public List<Statement> Statements { get; set; } = new();
}

public class Identifier : Expression
{
    public string Value { get; set; } = string.Empty;
}

public class If : Statement
{
    public Expression? Condition { get; set; }
    public List<Statement> Statements { get; set; } = new();
    public List<Statement>? Else { get; set; }
}

public class LiteralToken : Expression
{
    public Token Token { get; set; }
}

public class Loop : Statement
{
    public Expression? Condition { get; set; }
    public bool ConditionAtEnd { get; set; }
    public List<Statement> Statements { get; set; } = new();
}

public class Property : Statement
{
    public string Name { get; set; } = string.Empty;
}

public class Space : Statement
{
    public int Size { get; set; }
    public override bool IsCode => false;
}