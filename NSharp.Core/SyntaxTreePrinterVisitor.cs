namespace NSharp.Core.Ast;

public class SyntaxTreePrinterVisitor : ISyntaxTreeVisitor
{
    private int Indent { get; set; }

    private void Write(string line) => Console.Write(line);
    private void WriteLine() => Console.WriteLine();
    private void WriteLine(string line) => Console.WriteLine(line);

    private void WriteIndented(string line)
    {
        for (int i = 0; i < Indent; i++)
            Console.Write("  ");
        Console.Write(line);
    }

    private void WriteLineIndented(string line)
    {
        for (int i = 0; i < Indent; i++)
            Console.Write("  ");
        Console.WriteLine(line);
    }

    public void Visit(Expression item) => WriteLineIndented($"[{item}]");

    public void Visit(Statement item) => WriteLineIndented($"[{item}]");

    public void Visit(Accessor item)
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

    public void Visit(AnonymousFunction item)
    {
        WriteLine();
        Indent++;
        WriteIndented($"Anonymous function: ");
        if (item.ReturnType == null)
            Write("void");
        else
            item.ReturnType.Accept(this);
        Write(" (");
        bool first = true;
        foreach (var par in item.Parameters)
        {
            if (!first)
                Write(", ");
            par.Accept(this);
            first = false;
        }
        WriteLine(")");

        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);

        Indent -= 2;
    }

    public void Visit(Array item)
    {
        Write("[]");
        item.Type.Accept(this);
    }

    public void Visit(Assignment item)
    {
        WriteIndented("Assignment: ");
        item.Left.Accept(this);
        Write($" Assign.{item.Operator} ");
        item.Right.Accept(this);
        WriteLine();
    }

    public void Visit(BinaryOperator item)
    {
        Write("(");
        item.Left.Accept(this);
        Write($" {item.Operator} ");
        item.Right.Accept(this);
        Write(")");
    }

    public void Visit(Break item) => WriteLineIndented("Break");

    public void Visit(Case item)
    {
        WriteIndented($"Case: ");
        item.Expr.Accept(this);
        WriteLine();
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
    }

    public void Visit(Character item) => Write($"'{item.Value}'");

    public void Visit(Class item)
    {
        WriteIndented($"Class: {string.Join(" ", item.Modifiers)} {item.Name}");
        if (item.Parent != null)
        {
            Write(" inherits ");
            item.Parent.Accept(this);
        }
        if (item.Interfaces.Any())
        {
            Write(" implements ");
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
        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);
        Indent--;
    }

    public void Visit(Comment item) => WriteLineIndented($"Comment{(item.IsDocumentation ? " (doc)" : "")}: {item.Value}");

    public void Visit(Constant item)
    {
        WriteIndented($"Constant: {string.Join(" ", item.Modifiers)} ");
        item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine();
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
        if (item.Statements.Any())
        {
            WriteLine(" {");
            Indent++;
            foreach (var statement in item.Statements)
                statement.Accept(this);
            Indent--;
            WriteIndented("}");
        }
    }

    public void Visit(ConstructorDefinition item)
    {
        WriteIndented($"Constructor: {string.Join(" ", item.Modifiers)} .ctor(");
        bool first = true;
        foreach (var p in item.Parameters)
        {
            if (!first)
                Write(", ");
            p.Accept(this);
            first = false;
        }
        WriteLine(")");
        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);
        Indent--;
    }

    public void Visit(Continue item) => WriteLineIndented("Continue");

    public void Visit(CurrentObjectInstance item) => Write("[this]");

    public void Visit(DelegateDefinition item)
    {
        WriteIndented($"Delegate: {string.Join(" ", item.Modifiers)} ");
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
        WriteLine(")");
    }

    public void Visit(Discard item) => Write($"[discard]");

    public void Visit(Enumeration item)
    {
        WriteLineIndented($"Enum: {string.Join(" ", item.Modifiers)} {item.Name}");
        Indent++;
        foreach (var val in item.Values)
            val.Accept(this);
        Indent--;
    }

    public void Visit(EnumerationItem item)
    {
        WriteIndented($"{item.Name}{(item.Value != null ? " = " + item.Value : "")}");
        if (item.Comment != null)
        {
            int prev = Indent;
            Indent = 1;
            item.Comment.Accept(this);
            Indent = prev;
        }
        else
            WriteLine();
    }

    public void Visit(ErrorExpression item) => Write($"Error: {item.Value}");

    public void Visit(ErrorStatement item) => WriteLineIndented($"Error: {item.Value}");

    public void Visit(ExpressionStatement item)
    {
        WriteIndented($"Expression Statement: ");
        item.Expression.Accept(this);
        WriteLine();
    }

    public void Visit(Field item)
    {
        WriteIndented($"Field: {string.Join(" ", item.Modifiers)} ");
        item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine();
    }

    public void Visit(File item)
    {
        WriteLineIndented($"File: {item.Name}");
        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);
        Indent--;
    }

    public void Visit(For item)
    {
        WriteLineIndented("For");
        if (item.Init != null)
        {
            WriteLineIndented("Init");
            Indent++;
            item.Init.Accept(this);
            Indent--;
        }
        if (item.Condition != null)
        {
            WriteIndented("Condition: ");
            item.Condition.Accept(this);
            WriteLine();
        }
        if (item.Post != null)
        {
            WriteLineIndented("Post");
            Indent++;
            item.Post.Accept(this);
            Indent--;
        }
        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);
        Indent--;
    }

    public void Visit(Generic item)
    {
        item.Expr.Accept(this);
        Write("{");
        bool first = true;
        foreach (var arg in item.Arguments)
        {
            if (!first)
                Write(", ");
            arg.Accept(this);
            first = false;
        }
        Write("}");
    }

    public void Visit(Identifier item) => Write(item.Value);

    public void Visit(If item)
    {
        WriteIndented("If ");
        item.Condition.Accept(this);
        WriteLine();
        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);
        Indent--;
        if (item.Else != null)
        {
            WriteLineIndented("Else");
            Indent++;
            foreach (var statement in item.Else)
                statement.Accept(this);
            Indent--;
        }
    }

    public void Visit(Import item) => WriteLineIndented($"Import: {string.Join(".", item.Value)}");

    public void Visit(Interface item)
    {
        WriteIndented($"Interface: {string.Join(" ", item.Modifiers)} {item.Name}");
        if (item.Interfaces.Any())
        {
            Write(" is ");
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
        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);
        Indent--;
    }

    public void Visit(Is item)
    {
        item.Expr.Accept(this);
        Write(" is ");
        item.Type.Accept(this);
        if (item.Name != null)
            Write($" {item.Name}");
    }

    public void Visit(LiteralToken item) => Console.Write(item.Token);

    public void Visit(LocalConstant item)
    {
        WriteIndented($"Const: ");
        item.Type.Accept(this);
        Write($" {item.Name} = ");
        item.Value.Accept(this);
        WriteLine();
    }

    public void Visit(LocalVariable item)
    {
        WriteIndented($"Var: ");
        item.Type.Accept(this);
        Write($" {item.Name}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine();
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
        WriteIndented($"Method: {string.Join(" ", item.Modifiers)} ");
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
        WriteLine(")");

        Indent++;
        foreach (var statement in item.Statements)
            statement.Accept(this);
        Indent--;
    }

    public void Visit(MethodSignature item)
    {
        WriteIndented($"Method Sig: {string.Join(" ", item.Modifiers)} ");
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
        WriteLine(")");
    }

    public void Visit(Namespace item) => WriteLineIndented($"Namespace: {string.Join(".", item.Value)}");

    public void Visit(Number item) => Write(item.Value);

    public void Visit(Parameter item)
    {
        item.Type.Accept(this);
        Write($" {item.Name}");
    }

    public void Visit(Property item)
    {
        WriteIndented($"Property: {string.Join(" ", item.Modifiers)} ");
        item.Type.Accept(this);
        Write($" {item.Name}{(item.Get ? " get" : "")}{(item.Set ? " set" : "")}");
        if (item.Value != null)
        {
            Write(" = ");
            item.Value.Accept(this);
        }
        WriteLine();
        if (item.GetStatements.Any())
        {
            Indent++;
            WriteLineIndented("get");
            Indent++;
            foreach (var statement in item.GetStatements)
                statement.Accept(this);
            Indent -= 2;
        }
        if (item.SetStatements.Any())
        {
            Indent++;
            WriteLineIndented($"set({item.SetParameterName})");
            Indent++;
            foreach (var statement in item.SetStatements)
                statement.Accept(this);
            Indent -= 2;
        }
    }

    public void Visit(PropertySignature item)
    {
        WriteIndented($"Property Sig: {string.Join(" ", item.Modifiers)} ");
        item.Type.Accept(this);
        WriteLine($" {item.Name}{(item.Get ? " get" : "")}{(item.Set ? " set" : "")}");
    }

    public void Visit(Return item)
    {
        WriteIndented("Return: ");
        if (item.Value != null)
            item.Value.Accept(this);
        WriteLine();
    }

    public void Visit(Space item) => WriteLineIndented($"Space: {item.Size}");

    public void Visit(String item)
    {
        Write("\"");
        bool first = true;
        foreach (var line in item.Lines)
        {
            if (!first)
                WriteLine();
            foreach (var expr in line)
            {
                if (!(expr is StringLiteral))
                {
                    Write("{");
                    expr.Accept(this);
                    Write("}");
                }
                else
                    expr.Accept(this);
            }
            first = false;
        }
        Write("\"");
    }

    public void Visit(StringLiteral item) => Write(item.Value);

    public void Visit(Switch item)
    {
        WriteIndented($"Switch: ");
        item.Expr.Accept(this);
        WriteLine();
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
    }

    public void Visit(UnaryOperator item)
    {
        Write($"({item.Operator})");
        item.Expr.Accept(this);
    }

    public void Visit(Using item)
    {
        WriteIndented($"Using: {item.Name} = ");
        item.Expr.Accept(this);
        WriteLine();
        Indent++;
        foreach (var stmt in item.Statements)
            stmt.Accept(this);
        Indent--;
    }
}
