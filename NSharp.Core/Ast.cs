namespace NSharp.Core.Ast;

public interface IAstItem
{
    public Position Position { get; set; }
}

public interface IExpression : IAstItem { }

public interface IStatement : IAstItem { }

public class AstItem : IAstItem
{
    public Position Position { get; set; }
}

public class Class : AstItem, IStatement
{
    public string? Name { get; set; }
    public List<IStatement>? Statements { get; set; }
}

public class Comment : AstItem, IStatement
{
    public string? Content { get; set; }
}

public class CurrentObjectInstance : AstItem, IExpression { }

public class ExpressionStatement : AstItem, IStatement
{
    public IExpression? Expression { get; set; }
}

public class File : AstItem, IStatement
{
    public string? Name { get; set; }
}

public class FunctionDefinition : AstItem, IStatement
{
    public string? Name { get; set; }
    public List<IStatement>? Statements { get; set; }
}

public class Property : AstItem, IStatement
{
    public string? Name { get; set; }
}

public class Space : AstItem, IStatement
{
    public int Size { get; set; }
}