using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Compiler;

public class ToCsVisitor : ISyntaxTreeVisitor, IDisposable
{
    private StreamWriter Buffer { get; set; }
    private int Indent { get; set; } = 0;
    private string CurrentObject { get; set; } = string.Empty;
    private Stack<int> BinopParentPrecedence { get; set; } = new();
    private bool InFor { get; set; }
    private bool InExtension { get; set; }

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

    private void WriteModifiersIndented(List<Expression> modifiers)
    {
        WriteIndented(string.Join(" ", modifiers.Select(m => (m as Modifier)?.Type switch
        {
            ModifierType.Abstract => "abstract",
            ModifierType.Internal => "internal",
            ModifierType.Override => "override",
            ModifierType.Private => "private",
            ModifierType.Protected => "protected",
            ModifierType.Public => "public",
            ModifierType.Static => "static",
            ModifierType.Virtual => "virtual",

            ModifierType.Extension => "",

            _ => " [modifier] ",
        })));
    }

    private void WriteStatementBlock(List<Statement> statements)
    {
        WriteLineIndented("{");
        Indent++;
        foreach (var stmt in statements)
            stmt.Accept(this);
        Indent--;
        WriteLineIndented("}");
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
        Write("(");
        bool first = true;
        foreach (var param in item.Parameters)
        {
            if (!first)
                Write(", ");
            param.Accept(this);
            first = false;
        }
        WriteLine(") =>");
        Indent++;
        WriteStatementBlock(item.Statements);
        Indent--;
    }

    public void Visit(Argument item)
    {
        if (item.Name != null)
            Write($"{item.Name}: ");
        item.Expr.Accept(this);
    }

    public void Visit(Core.Ast.Array item)
    {
        item.Type.Accept(this);
        Write("[]");
    }

    public void Visit(Assignment item)
    {
        if (!InFor)
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
        if (!InFor)
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
            WriteStatementBlock(item.Statements);
            WriteLineIndented("break;");
            Indent--;
        }
    }

    public void Visit(Catch item)
    {
        WriteIndented("catch");
        if (item.Type != null)
        {
            Write(" (");
            item.Type.Accept(this);
            if (item.Name != null)
                Write($" {item.Name}");
            Write(")");
        }
        WriteLine();
        WriteStatementBlock(item.Statements);
    }

    public void Visit(Character item) => Write($"'{item.Value}'");

    public void Visit(Class item)
    {
        CurrentObject = item.Name;

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
        WriteStatementBlock(item.Statements);
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
        Write($" {CurrentObject}(");
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
        WriteStatementBlock(item.Statements);
    }

    public void Visit(Continue item) => WriteLineIndented("continue;");

    public void Visit(CurrentObjectInstance item) => Write(InExtension ? "__this" : "this");

    public void Visit(DelegateDefinition item)
    {
        WriteModifiersIndented(item.Modifiers);
        Write(" delegate ");
        if (item.ReturnType == null)
            Write("void");
        else
            item.ReturnType.Accept(this);
        Write($" {item.Name}(");
        bool first = true;
        foreach (var par in item.Parameters)
        {
            if (!first)
                Write(", ");
            par.Accept(this);
            first = false;
        }
        WriteLine(");");
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
        WriteLineIndented($"// {DateTime.Now}");
        WriteLine();
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
    }

    public void Visit(For item)
    {
        if (item.Init == null && item.Post == null)
        {
            if (item.Condition == null)
                WriteLineIndented("while (true)");
            else
            {
                WriteIndented("while (");
                item.Condition.Accept(this);
                WriteLine(")");
            }
            WriteStatementBlock(item.Statements);
        }
        else
        {
            WriteIndented($"for (var {item.Name} = ");
            item.Init?.Accept(this);
            Write("; ");
            item.Condition?.Accept(this);
            Write("; ");
            InFor = true;
            item.Post?.Accept(this);
            InFor = false;
            WriteLine(")");
            WriteStatementBlock(item.Statements);
        }
    }

    public void Visit(ForEach item)
    {
        bool between = item.BetweenStatements.Any();
        WriteLineIndented("{");
        Indent++;
        if (between)
            WriteLineIndented("bool __first = true;");
        WriteIndented($"foreach (var {item.Name} in ");
        item.Expr.Accept(this);
        WriteLine(")");
        WriteLineIndented("{");
        Indent++;
        if (between)
        {
            WriteLineIndented("if (__first)");
            Indent++;
            WriteLineIndented("__first = false;");
            Indent--;
            WriteLineIndented("else");
            WriteStatementBlock(item.BetweenStatements);
        }
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
        WriteLineIndented("}");
        Indent--;
        WriteLineIndented("}");
    }

    public void Visit(Generic item)
    {
        item.Expr.Accept(this);
        Write("<");
        bool first = true;
        foreach (var arg in item.Arguments)
        {
            if (!first)
                Write(", ");
            arg.Accept(this);
            first = false;
        }
        Write(">");
    }

    public void Visit(Identifier item) => Write(item.Value);

    public void Visit(If item)
    {
        WriteLineIndented("{");
        Indent++;
        if (item.Name != null && item.AssignExpr != null)
        {
            WriteIndented($"var {item.Name} = ");
            item.AssignExpr.Accept(this);
            WriteLine(";");
        }
        WriteIndented("if (");
        item.Condition.Accept(this);
        WriteLine(")");
        WriteStatementBlock(item.Statements);
        if (item.Else.Any())
        {
            WriteLineIndented("else");
            WriteStatementBlock(item.Else);
        }
        Indent--;
        WriteLineIndented("}");
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
        WriteModifiersIndented(item.Modifiers);
        Write($" interface {item.Name}");
        if (item.Interfaces.Any())
        {
            Write(": ");
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
        WriteStatementBlock(item.Statements);
    }

    public void Visit(Is item)
    {
        item.Expr.Accept(this);
        Write(" is ");
        item.Type.Accept(this);
        if (item.Name != null)
            Write($" {item.Name}");
    }

    public void Visit(LiteralToken item) =>
        Write(item.Token switch
        {
            Literal.False => "false",
            Literal.Null => "null",
            Literal.True => "true",
            _ => "[literal]",
        });

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
        var extension = item.Modifiers.FirstOrDefault(m => (m as Modifier)?.Type == ModifierType.Extension);
        if (extension is Modifier mod && mod.Arg != null)
        {
            first = false;
            Write("this ");
            mod.Arg.Accept(this);
            Write(" __this");
            InExtension = true;
        }
        foreach (var param in item.Parameters)
        {
            if (!first)
                Write(", ");
            param.Accept(this);
            first = false;
        }
        WriteLine(")");
        WriteStatementBlock(item.Statements);
        InExtension = false;
    }

    public void Visit(MethodSignature item)
    {
        WriteModifiersIndented(item.Modifiers);
        Write(" ");
        if (item.ReturnType == null)
            Write("void");
        else
            item.ReturnType.Accept(this);
        Write($" {item.Name}(");
        bool first = true;
        foreach (var par in item.Parameters)
        {
            if (!first)
                Write(", ");
            par.Accept(this);
            first = false;
        }
        WriteLine(");");
    }

    public void Visit(Modifier item) => Write("/* modifier */");

    public void Visit(Namespace item) => WriteLineIndented($"namespace {string.Join(".", item.Value)};");

    public void Visit(Core.Ast.Nullable item)
    {
        item.Type.Accept(this);
        Write("?");
    }

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
            WriteStatementBlock(item.GetStatements);
        }
        else if (item.Get)
            WriteLineIndented("get;");
        if (item.SetStatements.Any())
        {
            WriteLineIndented("set");
            WriteStatementBlock(item.SetStatements);
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
        WriteModifiersIndented(item.Modifiers);
        Write(" ");
        item.Type.Accept(this);
        Write($" {item.Name} {{");
        if (item.Get)
            Write("get;");
        if (item.Set)
            Write("set;");
        WriteLine("}");
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
        Write("$\"");
        bool first = true;
        foreach (var line in item.Lines)
        {
            if (!first)
                Write("{System.Environment.NewLine}");
            foreach (var expr in line)
            {
                bool curlies = expr is not StringLiteral;
                if (curlies)
                    Write("{");
                expr.Accept(this);
                if (curlies)
                    Write("}");
            }
            first = false;
        }
        Write("\"");
    }

    public void Visit(StringLiteral item) => Write(item.Value);

    public void Visit(Struct item)
    {
        CurrentObject = item.Name;

        WriteModifiersIndented(item.Modifiers);
        Write($" struct {item.Name}");
        if (item.Interfaces.Any())
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
        WriteStatementBlock(item.Statements);
    }

    public void Visit(Switch item)
    {
        WriteLineIndented("{");
        Indent++;
        if (item.Name != null && item.AssignExpr != null)
        {
            WriteIndented($"var {item.Name} = ");
            item.AssignExpr.Accept(this);
            WriteLine(";");
        }
        WriteIndented("switch (");
        item.Expr.Accept(this);
        WriteLine(")");
        WriteStatementBlock(item.Statements);
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
        WriteLineIndented("try");
        WriteStatementBlock(item.Statements);
        foreach (var cat in item.Catches)
            cat.Accept(this);
        if (item.FinallyStatements.Any())
        {
            WriteLineIndented("finally");
            WriteStatementBlock(item.FinallyStatements);
        }
    }

    public void Visit(UnaryOperator item)
    {
        Write(item.Operator switch
        {
            UnaryOperatorType.BitwiseNot => "~",
            UnaryOperatorType.Negate => "-",
            UnaryOperatorType.Not => "!",
            _ => "[unary op]",
        });
        item.Expr.Accept(this);
    }

    public void Visit(Using item)
    {
        if (item.Statements.Any())
        {
            WriteIndented($"using (var {item.Name} = ");
            item.Expr.Accept(this);
            WriteLine(")");
            WriteStatementBlock(item.Statements);
        }
        else
        {
            WriteIndented($"using var {item.Name} = ");
            item.Expr.Accept(this);
            WriteLine(";");
        }
    }
}