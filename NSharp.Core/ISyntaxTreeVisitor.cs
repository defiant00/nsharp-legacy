using NSharp.Core.Ast;

namespace NSharp.Core;

public interface ISyntaxTreeVisitor
{
    public void Visit(Expression item);
    public void Visit(Statement item);
    public void Visit(Accessor item);
    public void Visit(Ast.Array item);
    public void Visit(Assignment item);
    public void Visit(BinaryOperator item);
    public void Visit(Break item);
    public void Visit(Character item);
    public void Visit(Class item);
    public void Visit(Comment item);
    public void Visit(Constant item);
    public void Visit(ConstructorCall item);
    public void Visit(ConstructorDefinition item);
    public void Visit(Continue item);
    public void Visit(CurrentObjectInstance item);
    public void Visit(ErrorExpression item);
    public void Visit(ErrorStatement item);
    public void Visit(ExpressionStatement item);
    public void Visit(Ast.File item);
    public void Visit(For item);
    public void Visit(Generic item);
    public void Visit(Identifier item);
    public void Visit(If item);
    public void Visit(Import item);
    public void Visit(Is item);
    public void Visit(LiteralToken item);
    public void Visit(MethodCall item);
    public void Visit(MethodDefinition item);
    public void Visit(Namespace item);
    public void Visit(Number item);
    public void Visit(Parameter item);
    public void Visit(Property item);
    public void Visit(Return item);
    public void Visit(Space item);
    public void Visit(Ast.String item);
    public void Visit(StringLiteral item);
    public void Visit(UnaryOperator item);
    public void Visit(Variable item);
}