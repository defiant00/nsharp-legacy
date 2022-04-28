using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Compiler;

public class ToCsVisitor : ISyntaxTreeVisitor, IDisposable
{
    private StreamWriter Buffer { get; set; }
    private int Indent { get; set; } = 0;
    private string CurrentClass { get; set; } = string.Empty;
    private Stack<int> BinopParentPrecedence { get; set; } = new();

    private void Write(string line) => Buffer.Write(line);
    private void WriteLine() => Buffer.WriteLine();
    private void WriteLine(string line) => Buffer.WriteLine(line);

    private void WriteIndent()
    {
        for (int i = 0; i < Indent; i++)
            Write("    ");
    }

    private void WriteIndented(string line)
    {
        WriteIndent();
        Write(line);
    }

    private void WriteLineIndented(string line)
    {
        WriteIndent();
        WriteLine(line);
    }

    private void WriteModifiersIndented(List<Modifier> modifiers)
    {
        WriteIndented(string.Join(" ", modifiers.Select(m => m switch
        {
            Modifier.Internal => "internal",
            Modifier.Private => "private",
            Modifier.Protected => "protected",
            Modifier.Public => "public",
            Modifier.Static => "static",
            _ => " [modifier] ",
        })));
    }

    public ToCsVisitor(string filename)
    {
        Buffer = new StreamWriter(filename);
        BinopParentPrecedence.Push(-1);
    }

    public void Dispose() => Buffer?.Dispose();

    public void Visit(Expression item) => Write("/* expression */");

    public void Visit(Statement item) => WriteLine("/* statement */");

    public void Visit(AnonymousFunction item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Argument item)
    {
        if (item.Name != null)
            Write($"{item.Name}: ");
        item.Expr.Accept(this);
    }

    public void Visit(Core.Ast.Array item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Assignment item)
    {
        WriteIndent();
        item.Left.Accept(this);
        Write(item.Operator switch
        {
            AssignmentOperator.Add => " += ",
            AssignmentOperator.Assign => " = ",
            AssignmentOperator.BitwiseAnd => " &= ",
            AssignmentOperator.BitwiseOr => " |= ",
            AssignmentOperator.BitwiseXor => " ^= ",
            AssignmentOperator.Divide => " /= ",
            AssignmentOperator.LeftShift => " <<= ",
            AssignmentOperator.Modulus => " %= ",
            AssignmentOperator.Multiply => " *= ",
            AssignmentOperator.NullCoalesce => " ??= ",
            AssignmentOperator.RightShift => " >>= ",
            AssignmentOperator.Subtract => " -= ",
            _ => " [none] ",
        });
        item.Right.Accept(this);
        WriteLine(";");
    }

    public void Visit(BinaryOperator item)
    {
        bool parens = item.Operator.Precedence() < BinopParentPrecedence.Peek();
        if (parens)
            Write("(");
        BinopParentPrecedence.Push(item.Operator.Precedence());
        item.Left.Accept(this);
        BinopParentPrecedence.Pop();
        Write(item.Operator switch
        {
            BinaryOperatorType.Add => " + ",
            BinaryOperatorType.And => " && ",
            BinaryOperatorType.As => " as ",
            BinaryOperatorType.BitwiseAnd => " & ",
            BinaryOperatorType.BitwiseOr => " | ",
            BinaryOperatorType.BitwiseXor => " ^ ",
            BinaryOperatorType.Divide => " / ",
            BinaryOperatorType.Dot => ".",
            BinaryOperatorType.Equal => " == ",
            BinaryOperatorType.GreaterThan => " > ",
            BinaryOperatorType.GreaterThanOrEqual => " >= ",
            BinaryOperatorType.LeftShift => " << ",
            BinaryOperatorType.LessThan => " < ",
            BinaryOperatorType.LessThanOrEqual => " <= ",
            BinaryOperatorType.Modulus => " % ",
            BinaryOperatorType.Multiply => " * ",
            BinaryOperatorType.NotEqual => " != ",
            BinaryOperatorType.NullCoalesce => " ?? ",
            BinaryOperatorType.NullDot => "?.",
            BinaryOperatorType.Or => " || ",
            BinaryOperatorType.RightShift => " >> ",
            BinaryOperatorType.Subtract => " - ",
            _ => " [none] ",
        });
        BinopParentPrecedence.Push(item.Operator.Precedence() + 1);
        item.Right.Accept(this);
        BinopParentPrecedence.Pop();
        if (parens)
            Write(")");
    }

    public void Visit(Break item) => WriteLineIndented("break;");

    public void Visit(Case item)
    {
        if (item.Expr is Discard)
            WriteLineIndented("default:");
        else
        {
            WriteIndented("case ");
            item.Expr.Accept(this);
            WriteLine(":");
        }
        if (item.Statements.Any())
        {
            Indent++;
            WriteLineIndented("{");
            Indent++;
            foreach (var stmt in item.Statements)
                stmt.Accept(this);
            Indent--;
            WriteLineIndented("}");
            WriteLineIndented("break;");
            Indent--;
        }
    }

    public void Visit(Catch item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Character item) => Write($"'{item.Value}'");

    public void Visit(Core.Ast.Class item)
    {
        CurrentClass = item.Name;

        WriteModifiersIndented(item.Modifiers);
        Write($" class {item.Name}");
        if (item.Parent != null)
        {
            Write(" : ");
            item.Parent.Accept(this);
            foreach (var intf in item.Interfaces)
            {
                Write(", ");
                intf.Accept(this);
            }
        }
        else if (item.Interfaces.Any())
        {
            Write(" : ");
            bool first = true;
            foreach (var intf in item.Interfaces)
            {
                if (!first)
                    Write(", ");
                intf.Accept(this);
                first = false;
            }
        }
        WriteLine();
        WriteLineIndented("{");
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
        WriteLineIndented("}");
    }

    public void Visit(Comment item) => WriteLineIndented($"//{(item.IsDocumentation ? "/" : "")}{item.Value}");

    public void Visit(Condition item)
    {
        WriteIndent();
        item.Value.Accept(this);
        Write(" => ");
        item.Result.Accept(this);
        WriteLine(",");
    }

    public void Visit(Conditional item)
    {
        item.Expr.Accept(this);
        WriteLine(" switch");
        WriteLineIndented("{");
        Indent++;
        foreach (var cond in item.Conditions)
            cond.Accept(this);
        Indent--;
        WriteIndented("}");
    }

    public void Visit(Constant item)
    {
        WriteModifiersIndented(item.Modifiers);
        Write(" const ");
        item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine(";");
    }

    public void Visit(ConstructorCall item)
    {
        Write("new ");
        item.Expr.Accept(this);
        Write("(");
        bool first = true;
        foreach (var arg in item.Arguments)
        {
            if (!first)
                Write(", ");
            arg.Accept(this);
            first = false;
        }
        Write(")");
        if (item.InitProperties.Any())
        {
            Write("{");
            for (int i = 0; i < item.InitProperties.Count; i++)
            {
                if (i > 0)
                    Write(", ");
                Write($"{item.InitProperties[i]} = ");
                item.InitValues[i].Accept(this);
            }
            Write("}");
        }
    }

    public void Visit(ConstructorDefinition item)
    {
        WriteModifiersIndented(item.Modifiers);
        Write($" {CurrentClass}(");
        bool first = true;
        foreach (var param in item.Parameters)
        {
            if (!first)
                Write(", ");
            param.Accept(this);
            first = false;
        }
        Write(")");
        if (item.BaseArguments.Any())
        {
            Write(" : base(");
            first = true;
            foreach (var arg in item.BaseArguments)
            {
                if (!first)
                    Write(", ");
                arg.Accept(this);
                first = false;
            }
            Write(")");
        }
        WriteLine();
        WriteLineIndented("{");
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
        WriteLineIndented("}");
    }

    public void Visit(Continue item) => WriteLineIndented("continue;");

    public void Visit(CurrentObjectInstance item)
    {
        throw new NotImplementedException();
    }

    public void Visit(DelegateDefinition item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Discard item) => Write("_");

    public void Visit(Enumeration item)
    {
        WriteModifiersIndented(item.Modifiers);
        WriteLine($" enum {item.Name}");
        WriteLineIndented("{");
        Indent++;
        foreach (var val in item.Values)
            val.Accept(this);
        Indent--;
        WriteLineIndented("}");
    }

    public void Visit(EnumerationItem item)
    {
        WriteIndented(item.Name);
        if (item.Value != null)
        {
            Write($" = {item.Value}");
        }
        Write(",");
        if (item.Comment != null)
            item.Comment.Accept(this);
        else
            WriteLine();
    }

    public void Visit(ErrorExpression item) => Write($"/* Error: {item.Value} */");

    public void Visit(ErrorStatement item) => WriteLineIndented($"/* Error: {item.Value} */");

    public void Visit(ExpressionStatement item)
    {
        WriteIndent();
        item.Expression.Accept(this);
        WriteLine(";");
    }

    public void Visit(Field item)
    {
        WriteModifiersIndented(item.Modifiers);
        Write(" ");
        item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine(";");
    }

    public void Visit(Core.Ast.File item)
    {
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
    }

    public void Visit(For item)
    {
        throw new NotImplementedException();
    }

    public void Visit(ForEach item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Generic item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Identifier item) => Write(item.Value);

    public void Visit(If item)
    {
        WriteIndented("if (");
        item.Condition.Accept(this);
        WriteLine(")");
        WriteLineIndented("{");
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
        WriteLineIndented("}");
        if (item.Else.Any())
        {
            WriteLineIndented("else");
            WriteLineIndented("{");
            Indent++;
            foreach (var stmt in item.Else)
                stmt.Accept(this);
            Indent--;
            WriteLineIndented("}");
        }
    }

    public void Visit(Import item) => WriteLineIndented($"using {string.Join(".", item.Value)};");

    public void Visit(Indexer item)
    {
        item.Expr.Accept(this);
        Write("[");
        bool first = true;
        foreach (var arg in item.Arguments)
        {
            if (!first)
                Write(", ");
            arg.Accept(this);
            first = false;
        }
        Write("]");
    }

    public void Visit(Interface item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Is item)
    {
        item.Expr.Accept(this);
        Write(" is ");
        item.Type.Accept(this);
        if (item.Name != null)
            Write($" {item.Name}");
    }

    public void Visit(LiteralToken item)
    {
        throw new NotImplementedException();
    }

    public void Visit(LocalConstant item)
    {
        WriteIndented("const ");
        item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine(";");
    }

    public void Visit(LocalVariable item)
    {
        WriteIndent();
        if (item.Type == null)
            Write("var");
        else
            item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine(";");
    }

    public void Visit(MethodCall item)
    {
        item.Expr.Accept(this);
        Write("(");
        bool first = true;
        foreach (var arg in item.Arguments)
        {
            if (!first)
                Write(", ");
            arg.Accept(this);
            first = false;
        }
        Write(")");
    }

    public void Visit(MethodDefinition item)
    {
        WriteModifiersIndented(item.Modifiers);
        Write(" ");
        if (item.ReturnType == null)
            Write("void");
        else
            item.ReturnType.Accept(this);
        Write($" {item.Name}(");
        bool first = true;
        foreach (var param in item.Parameters)
        {
            if (!first)
                Write(", ");
            param.Accept(this);
            first = false;
        }
        WriteLine(")");
        WriteLineIndented("{");
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
        WriteLineIndented("}");
    }

    public void Visit(MethodSignature item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Namespace item) => WriteLineIndented($"namespace {string.Join(".", item.Value)};");

    public void Visit(Number item) => Write(item.Value);

    public void Visit(Parameter item)
    {
        item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
    }

    public void Visit(Property item)
    {
        WriteModifiersIndented(item.Modifiers);
        Write(" ");
        item.Type.Accept(this);
        WriteLine($" {item.Name}");
        WriteLineIndented("{");
        Indent++;
        if (item.GetStatements.Any())
        {
            WriteLineIndented("get");
            WriteLineIndented("{");
            Indent++;
            foreach (var stmt in item.GetStatements)
                stmt.Accept(this);
            Indent--;
            WriteLineIndented("}");
        }
        else if (item.Get)
            WriteLineIndented("get;");
        if (item.SetStatements.Any())
        {
            WriteLineIndented("set");
            WriteLineIndented("{");
            Indent++;
            foreach (var stmt in item.SetStatements)
                stmt.Accept(this);
            Indent--;
            WriteLineIndented("}");
        }
        else if (item.Set)
            WriteLineIndented("set;");
        Indent--;
        WriteIndented("}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
            Write(";");
        }
        WriteLine();
    }

    public void Visit(PropertySignature item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Return item)
    {
        WriteIndented("return ");
        item.Value?.Accept(this);
        WriteLine(";");
    }

    public void Visit(Space item)
    {
        for (int i = 0; i < item.Size; i++)
            WriteLine();
    }

    public void Visit(Core.Ast.String item)
    {
        throw new NotImplementedException();
    }

    public void Visit(StringLiteral item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Switch item)
    {
        WriteIndented("switch (");
        item.Expr.Accept(this);
        WriteLine(")");
        WriteLineIndented("{");
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
        WriteLineIndented("}");
    }

    public void Visit(Throw item)
    {
        WriteIndented("throw ");
        item.Expr.Accept(this);
        WriteLine(";");
    }

    public void Visit(Try item)
    {
        throw new NotImplementedException();
    }

    public void Visit(UnaryOperator item)
    {
        throw new NotImplementedException();
    }

    public void Visit(Using item)
    {
        throw new NotImplementedException();
    }
}