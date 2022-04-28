using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Language.Min.Compiler;

public class Parser
{
    private List<Token> Tokens { get; set; } = new();
    private int CurrentIndex { get; set; } = 0;

    private Token Peek => Tokens[CurrentIndex];

    private Token Next() => Tokens[CurrentIndex++];

    private ParseResult<Expression> ErrorExpression(string error, Position position)
    {
        // todo - log the error
        return new ParseResult<Expression>(new ErrorExpression(position, error), true);
    }

    private ParseResult<Expression> ErrorExpression(ErrorStatement stmt) => ErrorExpression(stmt.Value, stmt.Position);

    private ParseResult<Statement> ErrorStatement(string error, Position position)
    {
        // todo - log the error
        return new ParseResult<Statement>(new ErrorStatement(position, error), true);
    }

    private ParseResult<Statement> ErrorStatement(ErrorExpression expr) => ErrorStatement(expr.Value, expr.Position);

    private ParseResult<Expression> InvalidTokenErrorExpression(string error, AcceptResult result)
    {
        var token = ErrorToken(result);
        return ErrorExpression($"{error}: {token}", token.Position);
    }

    private ParseResult<Statement> InvalidTokenErrorStatement(string error, AcceptResult result)
    {
        var token = ErrorToken(result);
        return ErrorStatement($"{error}: {token}", token.Position);
    }

    public ParseResult<Statement> Parse(string fileName)
    {
        var lexer = new Lexer();

        using var reader = new StreamReader(fileName);
        string? line;
        while ((line = reader.ReadLine()) != null)
            lexer.Lex(line);
        lexer.EndOfFile();

        Tokens = lexer.Tokens;

        foreach (var token in lexer.Tokens)
        {
            Console.Write(token);
            if (token.Type == TokenType.EOL || token.Type == TokenType.EOF)
                Console.WriteLine();
            else
                Console.Write(" | ");
        }
        Console.WriteLine();

        var file = ParseFileStatement(fileName);

        file.Result.Accept(new SyntaxTreePrinterVisitor());

        return file;
    }

    private AcceptResult Accept(params TokenType[] tokens)
    {
        var result = new AcceptResult(CurrentIndex);
        foreach (var token in tokens)
        {
            var current = Next();
            if (current.Type != token)
            {
                CurrentIndex = result.StartingIndex;
                result.Success = false;
                return result;
            }
            result.Count++;
        }
        return result;
    }

    private Token GetToken(AcceptResult result, int index = 0) => Tokens[result.StartingIndex + index];

    private Token ErrorToken(AcceptResult result) => Tokens[result.StartingIndex + result.Count];

    private ParseResult<Expression> ParseAnonymouseFunction()
    {
        // fn([params]) is [statement]
        // fn([params]) [type] is [expression]
        // fn([params])
        //     [statements]
        // fn
        // fn([params]) [type]
        //     [statements]
        // fn

        var res = Accept(TokenType.Function, TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in anonymous function", res);

        var anonFn = new AnonymousFunction(GetToken(res).Position);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorExpression("Invalid token in parameter", res);
            var paramNameToken = GetToken(res);
            var paramType = ParseType();
            if (!paramType.Error)
                anonFn.Parameters.Add(new Parameter(paramType.Result.Position, paramType.Result, paramNameToken.Value));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in anonymous function", res);

        // fn([params]) is [statement]
        if (Accept(TokenType.Is).Success)
        {
            var stmtResult = ParseMethodStatement(false);
            if (stmtResult.Error && stmtResult.Result is ErrorStatement es)
                return ErrorExpression(es);
            anonFn.Statements.Add(stmtResult.Result);
            return new ParseResult<Expression>(anonFn);
        }

        // return type
        if (Peek.Type != TokenType.EOL)
        {
            var typeResult = ParseType();
            if (typeResult.Error)
                return typeResult;
            anonFn.ReturnType = typeResult.Result;

            // fn([params]) [type] is [expression]
            if (Accept(TokenType.Is).Success)
            {
                var exprResult = ParseExpression();
                if (exprResult.Error)
                    return exprResult;
                anonFn.Statements.Add(new Return(exprResult.Result.Position) { Value = exprResult.Result });
                return new ParseResult<Expression>(anonFn);
            }
        }

        // [modifiers] fn [name]([params])
        //     [statements]
        // [modifiers] fn [name]([params]) [type]
        //     [statements]
        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in anonymous function", res);

        while (Peek.Type != TokenType.Dedent)
        {
            var stmtRes = ParseMethodStatement();
            if (stmtRes.Error && stmtRes.Result is ErrorStatement es)
                return ErrorExpression(es);
            anonFn.Statements.Add(stmtRes.Result);
        }

        res = Accept(TokenType.Dedent, TokenType.Function);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in anonymous function", res);

        return new ParseResult<Expression>(anonFn);
    }

    // Returns null on success, or the error if there is one.
    private ParseResult<Expression>? ParseArguments(List<Expression> args)
    {
        var res = Accept(TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in args", res);

        if (Peek.Type != TokenType.RightParenthesis)
        {
            string? name = null;
            res = Accept(TokenType.Literal, TokenType.Assign);
            if (res.Success)
                name = GetToken(res).Value;

            var argExpr = ParseExpression();
            if (argExpr.Error)
                return argExpr;
            args.Add(new Argument(argExpr.Result.Position, argExpr.Result) { Name = name });
            while (Accept(TokenType.Comma).Success)
            {
                name = null;
                res = Accept(TokenType.Literal, TokenType.Assign);
                if (res.Success)
                    name = GetToken(res).Value;

                argExpr = ParseExpression();
                if (argExpr.Error)
                    return argExpr;
                args.Add(new Argument(argExpr.Result.Position, argExpr.Result) { Name = name });
            }
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in args", res);

        return null;
    }

    private ParseResult<Expression> ParseArray()
    {
        var start = Peek;
        var res = Accept(TokenType.LeftBracket, TokenType.RightBracket);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in array type declaration", res);

        var typeResult = ParseType();
        if (typeResult.Error)
            return typeResult;

        return new ParseResult<Expression>(new Core.Ast.Array(start.Position, typeResult.Result));
    }

    private ParseResult<Statement> ParseAssignment(Expression left)
    {
        var op = Next().Type.ToAssignmentOperator();
        var right = ParseExpression();
        if (right.Error && right.Result is ErrorExpression ex)
            return ErrorStatement(ex);
        var res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in assignment", res);
        return new ParseResult<Statement>(new Assignment(left.Position, op, left, right.Result));
    }

    private ParseResult<Expression> ParseBinaryOperatorRightSide(int leftPrecedence, Expression left, bool acceptIs)
    {
        while (true)
        {
            int tokenPrecedence = Peek.Precedence();

            // If this is a binary operator that binds at least as tightly as the
            // current operator then consume it, otherwise we're done.
            if (tokenPrecedence < leftPrecedence)
                return new ParseResult<Expression>(left);

            var op = Next();
            var right = ParsePrimaryExpression(acceptIs);
            if (right.Error)
                return right;

            // If the binary operator binds less tightly with the right than the operator
            // after the right, let the pending operator take the right as its left.
            if (tokenPrecedence < Peek.Precedence())
            {
                right = ParseBinaryOperatorRightSide(tokenPrecedence + 1, right.Result, acceptIs);
                if (right.Error)
                    return right;
            }

            // Merge left and right.
            left = new BinaryOperator(left.Position, op.Type.ToBinaryOperator(), left, right.Result);
        }
    }

    private ParseResult<Statement> ParseBreak()
    {
        var res = Accept(TokenType.Break, TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in break", res);
        return new ParseResult<Statement>(new Break(GetToken(res).Position));
    }

    private ParseResult<Statement> ParseClass(List<Modifier> modifiers)
    {
        var res = Accept(TokenType.Class, TokenType.Literal);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        var classResult = new Class(GetToken(res, 1).Position, modifiers, GetToken(res, 1).Value);

        if (Accept(TokenType.From).Success)
        {
            var parentType = ParseType();
            if (!parentType.Error)
                classResult.Parent = parentType.Result;
        }

        if (Accept(TokenType.Is).Success)
        {
            var interfaceType = ParseType();
            if (!interfaceType.Error)
                classResult.Interfaces.Add(interfaceType.Result);

            while (Accept(TokenType.Comma).Success)
            {
                interfaceType = ParseType();
                if (!interfaceType.Error)
                    classResult.Interfaces.Add(interfaceType.Result);
            }
        }

        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in class", res);

        while (Peek.Type != TokenType.Dedent)
        {
            var stmt = ParseClassStatement();
            if (stmt.Error)
                return stmt;
            classResult.Statements.Add(stmt.Result);
        }

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement($"Invalid token in class {classResult.Name}", res);

        return new ParseResult<Statement>(classResult);
    }

    private ParseResult<Statement> ParseClassStatement()
    {
        var startToken = Peek;
        int size = 0;
        while (Peek.Type == TokenType.EOL)
        {
            Next();
            size++;
        }
        if (size > 0)
            return new ParseResult<Statement>(new Space(startToken.Position, size));

        if (Peek.Type == TokenType.Comment)
            return ParseComment();

        var modifiers = new List<Modifier>();
        while (Peek.Type.IsModifier())
            modifiers.Add(Next().Type.ToModifier());

        switch (Peek.Type)
        {
            case TokenType.Class:
                return ParseClass(modifiers);
            case TokenType.Interface:
                return ParseInterface(modifiers);
            case TokenType.Struct:
                return ParseStruct(modifiers);
            case TokenType.Delegate:
                return ParseDelegate(modifiers);
            case TokenType.Enum:
                return ParseEnum(modifiers);
            case TokenType.Value:
                return ParseConstant(modifiers);
            case TokenType.Function:
                return ParseMethodOrProperty(modifiers);
            case TokenType.Variable:
                return ParseField(modifiers);
        }

        var token = Next();
        return ErrorStatement($"Invalid token in class: {token}", token.Position);
    }

    private ParseResult<Statement> ParseComment(bool acceptEol = true)
    {
        var comment = new Comment(Peek.Position, Peek.Value);
        Next();

        if (comment.Value.StartsWith(";"))
        {
            comment.Value = comment.Value[1..];
            comment.IsDocumentation = true;
        }

        var commentResult = new ParseResult<Statement>(comment);

        if (acceptEol)
        {
            var res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in comment", res);
        }

        return commentResult;
    }

    private ParseResult<Expression> ParseConditional(Expression expr)
    {
        // [expr] ? {[value] is [result], [value] is [result]}

        var res = Accept(TokenType.Conditional, TokenType.LeftCurly);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in conditional", res);

        var conditional = new Conditional(expr.Position, expr);

        while (Peek.Type != TokenType.RightCurly)
        {
            var valRes = ParseExpression(false);
            if (valRes.Error)
                return valRes;

            res = Accept(TokenType.Is);
            if (res.Failure)
                return InvalidTokenErrorExpression("Invalid token in condition", res);

            var resRes = ParseExpression();
            if (resRes.Error)
                return resRes;

            conditional.Conditions.Add(new Condition(valRes.Result.Position, valRes.Result, resRes.Result));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightCurly);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in conditional", res);

        return new ParseResult<Expression>(conditional);
    }

    private ParseResult<Statement> ParseConstant(List<Modifier> modifiers)
    {
        // [modifiers] val [name] [type]
        // [modifiers] val [name] [type] = [expr]

        var start = Next();     // accept val

        var res = Accept(TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in constant", res);

        string name = GetToken(res).Value;

        var typeResult = ParseType();
        if (typeResult.Error && typeResult.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        var constant = new Constant(start.Position, modifiers, name, typeResult.Result);

        if (Accept(TokenType.Assign).Success)
        {
            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp);
            constant.Value = exprResult.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in constant", res);

        return new ParseResult<Statement>(constant);
    }

    private ParseResult<Expression> ParseConstructorCall()
    {
        var start = Next();     // accept new
        var typeResult = ParseType();
        if (typeResult.Error)
            return typeResult;

        var ctor = new ConstructorCall(start.Position, typeResult.Result);

        var argsRes = ParseArguments(ctor.Arguments);
        if (argsRes != null)
            return argsRes;

        AcceptResult res;
        if (Accept(TokenType.LeftCurly).Success)
        {
            while (Peek.Type != TokenType.RightCurly)
            {
                res = Accept(TokenType.Literal, TokenType.Assign);
                if (res.Failure)
                    return InvalidTokenErrorExpression("Invalid token in constructor call", res);

                var prop = GetToken(res).Value;

                var right = ParseExpression();
                if (right.Error)
                    return right;

                ctor.InitProperties.Add(prop);
                ctor.InitValues.Add(right.Result);

                Accept(TokenType.Comma);
            }

            res = Accept(TokenType.RightCurly);
            if (res.Failure)
                return InvalidTokenErrorExpression("Invalid token in constructor call", res);
        }

        return new ParseResult<Expression>(ctor);
    }

    private ParseResult<Statement> ParseConstructorDefinition(List<Modifier> modifiers, Token start)
    {
        // [modifiers] fn new([params]) is [statement]
        // [modifiers] fn new([params]) base([exprs]) is [statement]
        // [modifiers] fn new([params])
        //     [statements]
        // [modifiers] fn new([params]) base([exprs])
        //     [statements]

        var res = Accept(TokenType.New, TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        var ctorDef = new ConstructorDefinition(start.Position, modifiers);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in parameter", res);
            var paramNameToken = GetToken(res);
            var paramType = ParseType();
            if (paramType.Error && paramType.Result is ErrorExpression ex)
                return ErrorStatement(ex);

            var par = new Parameter(paramType.Result.Position, paramType.Result, paramNameToken.Value);
            ctorDef.Parameters.Add(par);

            if (Accept(TokenType.Assign).Success)
            {
                var exprRes = ParseExpression();
                if (exprRes.Error && exprRes.Result is ErrorExpression exp)
                    return ErrorStatement(exp);
                par.Value = exprRes.Result;
            }

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        // base([exprs])
        if (Accept(TokenType.Base, TokenType.LeftParenthesis).Success)
        {
            while (Peek.Type != TokenType.RightParenthesis)
            {
                var exprRes = ParseExpression();
                if (exprRes.Error && exprRes.Result is ErrorExpression ex)
                    return ErrorStatement(ex);

                ctorDef.BaseArguments.Add(exprRes.Result);

                Accept(TokenType.Comma);
            }

            res = Accept(TokenType.RightParenthesis);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in ctor", res);
        }

        // [modifiers] fn new([params]) is [statement]
        if (Accept(TokenType.Is).Success)
        {
            var stmtResult = ParseMethodStatement();
            if (stmtResult.Error)
                return stmtResult;
            ctorDef.Statements.Add(stmtResult.Result);
            return new ParseResult<Statement>(ctorDef);
        }

        // [modifiers] fn new([params])
        //     [statements]
        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        while (Peek.Type != TokenType.Dedent)
        {
            var stmt = ParseMethodStatement();
            if (stmt.Error)
                return stmt;
            ctorDef.Statements.Add(stmt.Result);
        }

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in ctor", res);

        return new ParseResult<Statement>(ctorDef);
    }

    private ParseResult<Statement> ParseContinue()
    {
        var res = Accept(TokenType.Continue, TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in continue", res);
        return new ParseResult<Statement>(new Continue(GetToken(res).Position));
    }

    private ParseResult<Statement> ParseDelegate(List<Modifier> modifiers)
    {
        // [modifiers] del [name]([params])
        // [modifiers] del [name]([params]) [type]

        Next();     // accept del

        var res = Accept(TokenType.Literal, TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in delegate", res);

        var nameToken = GetToken(res);

        var delegateDef = new DelegateDefinition(nameToken.Position, modifiers, nameToken.Value);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in delegate", res);
            var paramNameToken = GetToken(res);
            var paramType = ParseType();
            if (!paramType.Error)
                delegateDef.Parameters.Add(new Parameter(paramType.Result.Position, paramType.Result, paramNameToken.Value));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in delegate", res);

        if (Peek.Type != TokenType.EOL)
        {
            var retType = ParseType();
            if (!retType.Error)
                delegateDef.ReturnType = retType.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in delegate", res);

        return new ParseResult<Statement>(delegateDef);
    }

    private ParseResult<Statement> ParseEnum(List<Modifier> modifiers)
    {
        // [modifiers] enum [name]
        //     Val
        //     Val2 = 3
        //     Val3 ; comment
        //     Val4 = 5 ; comment

        var res = Accept(TokenType.Enum, TokenType.Literal, TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in enum", res);

        var enumDef = new Enumeration(GetToken(res).Position, modifiers, GetToken(res, 1).Value);

        while (Peek.Type != TokenType.Dedent)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in enum", res);
            var enumItem = new EnumerationItem(GetToken(res).Position, GetToken(res).Value);

            res = Accept(TokenType.Assign, TokenType.NumberLiteral);
            if (res.Success)
                enumItem.Value = Convert.ToInt32(GetToken(res, 1).Value);

            if (Peek.Type == TokenType.Comment)
            {
                var commentResult = ParseComment();
                if (commentResult.Error)
                    return commentResult;
                enumItem.Comment = commentResult.Result as Comment;
            }
            else
            {
                res = Accept(TokenType.EOL);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in enum", res);
            }

            enumDef.Values.Add(enumItem);
        }

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in enum", res);

        return new ParseResult<Statement>(enumDef);
    }

    private ParseResult<Expression> ParseExpression(bool acceptIs = true)
    {
        var left = ParsePrimaryExpression(acceptIs);
        if (left.Error)
            return left;
        return ParseBinaryOperatorRightSide(0, left.Result, acceptIs);
    }

    private ParseResult<Statement> ParseExpressionStatement(bool acceptEol)
    {
        var expr = ParseExpression();
        if (expr.Error && expr.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        if (Peek.Type.IsAssignment())
            return ParseAssignment(expr.Result);

        if (acceptEol)
        {
            var res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in expression statement", res);
        }

        return new ParseResult<Statement>(new ExpressionStatement(expr.Result.Position, expr.Result));
    }

    private ParseResult<Statement> ParseField(List<Modifier> modifiers)
    {
        // [modifiers] var [name] [type]
        // [modifiers] var [name] [type] = [expr]

        var start = Next();     // accept var

        var res = Accept(TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in field", res);

        string name = GetToken(res).Value;

        var typeResult = ParseType();
        if (typeResult.Error && typeResult.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        var field = new Field(start.Position, modifiers, name, typeResult.Result);

        if (Accept(TokenType.Assign).Success)
        {
            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp);
            field.Value = exprResult.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in field", res);

        return new ParseResult<Statement>(field);
    }

    private ParseResult<Statement> ParseFileModifiableStatement()
    {
        var modifiers = new List<Modifier>();
        while (Peek.Type.IsModifier())
            modifiers.Add(Next().Type.ToModifier());

        return Peek.Type switch
        {
            TokenType.Class => ParseClass(modifiers),
            TokenType.Delegate => ParseDelegate(modifiers),
            TokenType.Enum => ParseEnum(modifiers),
            TokenType.Interface => ParseInterface(modifiers),
            TokenType.Struct => ParseStruct(modifiers),
            _ => ErrorStatement($"Invalid token: {Peek}", Peek.Position),
        };
    }

    private ParseResult<Statement> ParseFileStatement(string fileName)
    {
        var file = new Core.Ast.File(Path.GetFileName(fileName));
        bool error = false;
        while (CurrentIndex < Tokens.Count)
        {
            switch (Peek.Type)
            {
                case TokenType.Class:
                case TokenType.Delegate:
                case TokenType.Enum:
                case TokenType.Interface:
                case TokenType.Struct:
                    file.Statements.Add(ParseFileModifiableStatement().Result);
                    break;
                case TokenType.Comment:
                    file.Statements.Add(ParseComment().Result);
                    break;
                case TokenType.EOF:
                    // call next so that we break out of the while loop
                    Next();
                    break;
                case TokenType.EOL:
                    file.Statements.Add(ParseSpace().Result);
                    break;
                case TokenType.Namespace:
                    file.Statements.Add(ParseNamespace().Result);
                    break;
                case TokenType.Use:
                    file.Statements.Add(ParseImport().Result);
                    break;
                default:
                    if (Peek.Type.IsModifier())
                        file.Statements.Add(ParseFileModifiableStatement().Result);
                    else
                    {
                        file.Statements.Add(ErrorStatement($"Invalid token: {Peek}", Peek.Position).Result);
                        Next();
                        error = true;
                    }
                    break;
            }
        }

        return new ParseResult<Statement>(file, error);
    }

    private ParseResult<Statement> ParseFor()
    {
        var res = Accept(TokenType.For);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in for", res);

        var pos = GetToken(res).Position;

        // for [name] in [expr]
        //     [statements]
        // div
        //     [statements]
        res = Accept(TokenType.Literal, TokenType.In);
        if (res.Success)
        {
            // for [name] in [expr]
            //     [statements]

            var name = GetToken(res).Value;

            var exprRes = ParseExpression();
            if (exprRes.Error && exprRes.Result is ErrorExpression ex)
                return ErrorStatement(ex);

            var foreachDef = new ForEach(pos, name, exprRes.Result);

            res = Accept(TokenType.EOL, TokenType.Indent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in for", res);

            while (Peek.Type != TokenType.Dedent)
            {
                var stmtRes = ParseMethodStatement();
                if (stmtRes.Error)
                    return stmtRes;
                foreachDef.Statements.Add(stmtRes.Result);
            }

            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in for", res);

            // div
            //     [statements]
            if (Accept(TokenType.Between, TokenType.EOL, TokenType.Indent).Success)
            {
                while (Peek.Type != TokenType.Dedent)
                {
                    var stmtRes = ParseMethodStatement();
                    if (stmtRes.Error)
                        return stmtRes;
                    foreachDef.BetweenStatements.Add(stmtRes.Result);
                }

                res = Accept(TokenType.Dedent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in for", res);
            }

            return new ParseResult<Statement>(foreachDef);
        }

        var forDef = new For(pos);

        res = Accept(TokenType.Literal, TokenType.Assign);
        if (res.Success)
        {
            // for [name] = [expr], [expr], [stmt]

            forDef.Name = GetToken(res).Value;
            var exprRes = ParseExpression();
            if (exprRes.Error && exprRes.Result is ErrorExpression ex1)
                return ErrorStatement(ex1);

            forDef.Init = exprRes.Result;

            res = Accept(TokenType.Comma);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in for", res);

            exprRes = ParseExpression();
            if (exprRes.Error && exprRes.Result is ErrorExpression ex2)
                return ErrorStatement(ex2);

            forDef.Condition = exprRes.Result;

            res = Accept(TokenType.Comma);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in for", res);

            var stmtRes = ParseMethodStatement();
            if (stmtRes.Error)
                return stmtRes;

            forDef.Post = stmtRes.Result;
        }
        else if (Peek.Type != TokenType.EOL)
        {
            // for [expr]

            var exprRes = ParseExpression();
            if (exprRes.Error && exprRes.Result is ErrorExpression ex)
                return ErrorStatement(ex);

            forDef.Condition = exprRes.Result;

            res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in for", res);
        }
        else
        {
            res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in for", res);
        }

        // for
        //     [statements]
        res = Accept(TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in for", res);

        while (Peek.Type != TokenType.Dedent)
        {
            var stmtRes = ParseMethodStatement();
            if (stmtRes.Error)
                return stmtRes;
            forDef.Statements.Add(stmtRes.Result);
        }

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in for", res);

        return new ParseResult<Statement>(forDef);
    }

    private ParseResult<Expression> ParseGeneric(Expression expr)
    {
        var res = Accept(TokenType.LeftCurly);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in generic", res);

        var argType = ParseType();
        if (argType.Error)
            return argType;

        var generic = new Generic(expr.Position, expr, argType.Result);

        while (Accept(TokenType.Comma).Success)
        {
            argType = ParseType();
            if (argType.Error)
                return argType;
            generic.Arguments.Add(argType.Result);
        }

        res = Accept(TokenType.RightCurly);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in generic", res);

        return new ParseResult<Expression>(generic);
    }

    // Returns the error parse result, or null on success.
    private ParseResult<Statement>? ParseGetSet(Property propDef)
    {
        AcceptResult res;
        if (Peek.Type == TokenType.Get)
        {
            if (Accept(TokenType.Get, TokenType.Is).Success)
            {
                // get is [expr]

                var exprResult = ParseExpression();
                if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                    return ErrorStatement(exp);
                propDef.GetStatements.Add(new Return(exprResult.Result.Position) { Value = exprResult.Result });
                res = Accept(TokenType.EOL);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in get", res);
            }
            else
            {
                // get
                //     [statements]

                res = Accept(TokenType.Get, TokenType.EOL, TokenType.Indent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in get", res);

                while (Peek.Type != TokenType.Dedent)
                {
                    var stmt = ParseMethodStatement();
                    if (stmt.Error)
                        return stmt;
                    propDef.GetStatements.Add(stmt.Result);
                }

                res = Accept(TokenType.Dedent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in get", res);
            }
        }
        else
        {
            res = Accept(TokenType.Set, TokenType.LeftParenthesis, TokenType.Literal, TokenType.RightParenthesis);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in set", res);
            propDef.SetParameterName = GetToken(res, 2).Value;

            if (Accept(TokenType.Is).Success)
            {
                // set(v) is [statement]

                var stmtResult = ParseMethodStatement();
                if (stmtResult.Error)
                    return stmtResult;
                propDef.SetStatements.Add(stmtResult.Result);
            }
            else
            {
                // set(v)
                //     [statements]

                res = Accept(TokenType.EOL, TokenType.Indent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in set", res);

                while (Peek.Type != TokenType.Dedent)
                {
                    var stmt = ParseMethodStatement();
                    if (stmt.Error)
                        return stmt;
                    propDef.SetStatements.Add(stmt.Result);
                }

                res = Accept(TokenType.Dedent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in set", res);
            }
        }
        return null;
    }

    private ParseResult<Expression> ParseIdentifier()
    {
        var start = Next();
        return new ParseResult<Expression>(new Identifier(start.Position, start.Value));
    }

    private ParseResult<Statement> ParseIf(bool inElseIf = false)
    {
        // accept if
        var start = Next();
        var condition = ParseExpression();
        if (condition.Error && condition.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        var res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in if", res);

        // if [expr]
        //     is [expr]
        //         [statements]
        //     is [expr]
        //     is [expr]
        //         [statements]
        if (Peek.Type == TokenType.Is && !inElseIf)
        {
            var switchStmt = new Switch(start.Position, condition.Result);
            var isRes = Accept(TokenType.Is);
            while (isRes.Success)
            {
                var exprRes = ParseExpression();
                if (exprRes.Error && exprRes.Result is ErrorExpression exp)
                    return ErrorStatement(exp);

                res = Accept(TokenType.EOL);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in if", res);

                var caseStmt = new Case(GetToken(isRes).Position, exprRes.Result);
                switchStmt.Statements.Add(caseStmt);

                if (Accept(TokenType.Indent).Success)
                {
                    while (Peek.Type != TokenType.Dedent)
                    {
                        var stmtRes = ParseMethodStatement();
                        if (stmtRes.Error)
                            return stmtRes;
                        caseStmt.Statements.Add(stmtRes.Result);
                    }

                    res = Accept(TokenType.Dedent);
                    if (res.Failure)
                        return InvalidTokenErrorStatement("Invalid token in if", res);
                }

                isRes = Accept(TokenType.Is);
            }

            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in if", res);

            return new ParseResult<Statement>(switchStmt);
        }

        var ifStatement = new If(start.Position, condition.Result);

        while (Peek.Type != TokenType.Dedent)
        {
            var statement = ParseMethodStatement();
            if (statement.Error)
                return statement;
            ifStatement.Statements.Add(statement.Result);
        }

        res = Accept(TokenType.Dedent, TokenType.Else);
        if (res.Success)
        {
            if (Peek.Type == TokenType.If)
            {
                var elseIf = ParseIf(true);
                if (elseIf.Error)
                    return elseIf;
                ifStatement.Else.Add(elseIf.Result);
            }
            else
            {
                res = Accept(TokenType.EOL, TokenType.Indent);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in if", res);

                while (Peek.Type != TokenType.Dedent)
                {
                    var statement = ParseMethodStatement();
                    if (statement.Error)
                        return statement;
                    ifStatement.Else.Add(statement.Result);
                }
            }
        }

        if (!inElseIf)
        {
            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in if", res);
        }

        return new ParseResult<Statement>(ifStatement);
    }

    private ParseResult<Statement> ParseImport()
    {
        var res = Accept(TokenType.Use, TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in use", res);

        var import = new Import(GetToken(res).Position, GetToken(res, 1).Value);

        res = Accept(TokenType.Dot, TokenType.Literal);
        while (res.Success)
        {
            import.Value.Add(GetToken(res, 1).Value);
            res = Accept(TokenType.Dot, TokenType.Literal);
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in use", res);

        return new ParseResult<Statement>(import);
    }

    private ParseResult<Expression> ParseIndexer(Expression expr)
    {
        var res = Accept(TokenType.LeftBracket);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in indexer", res);

        var argExpr = ParseExpression();
        if (argExpr.Error)
            return argExpr;

        var indexer = new Indexer(expr.Position, expr, argExpr.Result);

        while (Accept(TokenType.Comma).Success)
        {
            argExpr = ParseExpression();
            if (argExpr.Error)
                return argExpr;
            indexer.Arguments.Add(argExpr.Result);
        }

        res = Accept(TokenType.RightBracket);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in indexer", res);

        return new ParseResult<Expression>(indexer);
    }

    private ParseResult<Statement> ParseInterface(List<Modifier> modifiers)
    {
        var res = Accept(TokenType.Interface, TokenType.Literal);

        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in interface", res);

        var intf = new Interface(GetToken(res, 1).Position, modifiers, GetToken(res, 1).Value);

        if (Accept(TokenType.Is).Success)
        {
            var interfaceType = ParseType();
            if (!interfaceType.Error)
                intf.Interfaces.Add(interfaceType.Result);

            while (Accept(TokenType.Comma).Success)
            {
                interfaceType = ParseType();
                if (!interfaceType.Error)
                    intf.Interfaces.Add(interfaceType.Result);
            }
        }

        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in interface", res);

        while (Peek.Type != TokenType.Dedent)
        {
            var ist = ParseInterfaceStatement();
            if (ist.Error)
                return ist;
            intf.Statements.Add(ist.Result);
        }

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in interface", res);

        return new ParseResult<Statement>(intf);
    }

    private ParseResult<Statement> ParseInterfaceStatement()
    {
        var startToken = Peek;
        int size = 0;
        while (Peek.Type == TokenType.EOL)
        {
            Next();
            size++;
        }
        if (size > 0)
            return new ParseResult<Statement>(new Space(startToken.Position, size));

        if (Peek.Type == TokenType.Comment)
            return ParseComment();

        var modifiers = new List<Modifier>();
        while (Peek.Type.IsModifier())
            modifiers.Add(Next().Type.ToModifier());

        var res = Accept(TokenType.Function, TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in interface", res);

        var fnPos = GetToken(res).Position;
        string name = GetToken(res, 1).Value;

        if (Peek.Type == TokenType.LeftParenthesis)
            return ParseMethodSignature(modifiers, fnPos, name);

        return ParsePropertySignature(modifiers, fnPos, name);
    }

    private ParseResult<Expression> ParseIs(Expression expr)
    {
        Next(); // accept is
        var typeResult = ParseType();
        if (typeResult.Error)
            return typeResult;
        var isExpr = new Is(expr.Position, expr, typeResult.Result);

        if (Peek.Type == TokenType.Literal)
            isExpr.Name = Next().Value;

        return new ParseResult<Expression>(isExpr);
    }

    private ParseResult<Statement> ParseLocalConstant()
    {
        // val [name] [type] = [expr]

        var res = Accept(TokenType.Value, TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in val", res);

        var pos = GetToken(res).Position;
        var name = GetToken(res, 1).Value;

        var typeRes = ParseType();
        if (typeRes.Error && typeRes.Result is ErrorExpression ex)
            return ErrorStatement(ex);


        res = Accept(TokenType.Assign);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in val", res);

        var valRes = ParseExpression();
        if (valRes.Error && valRes.Result is ErrorExpression exp)
            return ErrorStatement(exp);

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in val", res);

        return new ParseResult<Statement>(new LocalConstant(pos, name, typeRes.Result, valRes.Result));
    }

    private ParseResult<Statement> ParseLocalVariable()
    {
        // var [name] [type]
        // var [name] = [expr]
        // var [name] [type] = [expr]

        var res = Accept(TokenType.Variable, TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in var", res);

        var local = new LocalVariable(GetToken(res).Position, GetToken(res, 1).Value);

        if (Accept(TokenType.Assign).Success)
        {
            var valRes = ParseExpression();
            if (valRes.Error && valRes.Result is ErrorExpression exp)
                return ErrorStatement(exp);
            local.Value = valRes.Result;
        }
        else
        {
            var typeRes = ParseType();
            if (typeRes.Error && typeRes.Result is ErrorExpression ex)
                return ErrorStatement(ex);
            local.Type = typeRes.Result;

            if (Accept(TokenType.Assign).Success)
            {
                var valRes = ParseExpression();
                if (valRes.Error && valRes.Result is ErrorExpression exp)
                    return ErrorStatement(exp);
                local.Value = valRes.Result;
            }
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in var", res);

        return new ParseResult<Statement>(local);
    }

    private ParseResult<Expression> ParseMethodCall(Expression expr)
    {
        var methodCall = new MethodCall(expr.Position, expr);

        var argsRes = ParseArguments(methodCall.Arguments);
        if (argsRes != null)
            return argsRes;

        return new ParseResult<Expression>(methodCall);
    }

    private ParseResult<Statement> ParseMethodDefinition(List<Modifier> modifiers, Token nameToken)
    {
        // method
        // [modifiers] fn [name]([params]) is [statement]
        // [modifiers] fn [name]([params])
        //     [statements]
        // [modifiers] fn [name]([params]) [type] is [expr]
        // [modifiers] fn [name]([params]) [type]
        //     [statements]

        var res = Accept(TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        var methodDef = new MethodDefinition(nameToken.Position, modifiers, nameToken.Value);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in parameter", res);
            var paramNameToken = GetToken(res);
            var paramType = ParseType();
            if (paramType.Error && paramType.Result is ErrorExpression ex)
                return ErrorStatement(ex);

            var par = new Parameter(paramType.Result.Position, paramType.Result, paramNameToken.Value);
            methodDef.Parameters.Add(par);

            if (Accept(TokenType.Assign).Success)
            {
                var exprRes = ParseExpression();
                if (exprRes.Error && exprRes.Result is ErrorExpression exp)
                    return ErrorStatement(exp);
                par.Value = exprRes.Result;
            }

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        // [modifiers] fn [name]([params]) is [statement]
        if (Accept(TokenType.Is).Success)
        {
            var stmtResult = ParseMethodStatement();
            if (stmtResult.Error)
                return stmtResult;
            methodDef.Statements.Add(stmtResult.Result);
            return new ParseResult<Statement>(methodDef);
        }

        // return type
        if (Peek.Type != TokenType.EOL)
        {
            var typeResult = ParseType();
            if (typeResult.Error && typeResult.Result is ErrorExpression ex)
                return ErrorStatement(ex);
            methodDef.ReturnType = typeResult.Result;

            // [modifiers] fn [name]([params]) [type] is [expr]
            if (Accept(TokenType.Is).Success)
            {
                var exprResult = ParseExpression();
                if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                    return ErrorStatement(exp);
                methodDef.Statements.Add(new Return(exprResult.Result.Position) { Value = exprResult.Result });
                res = Accept(TokenType.EOL);
                if (res.Failure)
                    return InvalidTokenErrorStatement("Invalid token in method", res);
                return new ParseResult<Statement>(methodDef);
            }
        }

        // [modifiers] fn [name]([params])
        //     [statements]
        // [modifiers] fn [name]([params]) [type]
        //     [statements]
        res = Accept(TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        while (Peek.Type != TokenType.Dedent)
        {
            var stmtRes = ParseMethodStatement();
            if (stmtRes.Error)
                return stmtRes;
            methodDef.Statements.Add(stmtRes.Result);
        }

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method", res);

        return new ParseResult<Statement>(methodDef);
    }

    private ParseResult<Statement> ParseMethodOrProperty(List<Modifier> modifiers)
    {
        var start = Next();     // accept fn

        if (Peek.Type == TokenType.New)
            return ParseConstructorDefinition(modifiers, start);

        var res = Accept(TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method or property", res);

        var nameToken = GetToken(res);

        if (Peek.Type == TokenType.LeftParenthesis)
            return ParseMethodDefinition(modifiers, nameToken);

        return ParseProperty(modifiers, nameToken);
    }

    private ParseResult<Statement> ParseMethodSignature(List<Modifier> modifiers, Position pos, string name)
    {
        // [modifiers] fn [name]([params])
        // [modifiers] fn [name]([params]) [type]

        var res = Accept(TokenType.LeftParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method signature", res);

        var methodSig = new MethodSignature(pos, modifiers, name);

        while (Peek.Type != TokenType.RightParenthesis)
        {
            res = Accept(TokenType.Literal);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in parameter", res);
            var paramNameToken = GetToken(res);
            var paramType = ParseType();
            if (!paramType.Error)
                methodSig.Parameters.Add(new Parameter(paramType.Result.Position, paramType.Result, paramNameToken.Value));

            Accept(TokenType.Comma);
        }

        res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method signature", res);

        // return type
        if (Peek.Type != TokenType.EOL)
        {
            var typeResult = ParseType();
            if (typeResult.Error && typeResult.Result is ErrorExpression ex)
                return ErrorStatement(ex);
            methodSig.ReturnType = typeResult.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in method signature", res);

        return new ParseResult<Statement>(methodSig);
    }

    private ParseResult<Statement> ParseMethodStatement(bool acceptEol = true) =>
        Peek.Type switch
        {
            TokenType.Break => ParseBreak(),
            TokenType.Comment => ParseComment(acceptEol),
            TokenType.Continue => ParseContinue(),
            TokenType.EOL => ParseSpace(),
            TokenType.For => ParseFor(),
            TokenType.If => ParseIf(),
            TokenType.Return => ParseReturn(acceptEol),
            TokenType.Throw => ParseThrow(acceptEol),
            TokenType.Try => ParseTry(),
            TokenType.Use => ParseUsing(),
            TokenType.Variable => ParseLocalVariable(),
            TokenType.Value => ParseLocalConstant(),
            _ => ParseExpressionStatement(acceptEol),
        };

    private ParseResult<Statement> ParseNamespace()
    {
        var res = Accept(TokenType.Namespace, TokenType.Literal);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in namespace", res);

        var ns = new Namespace(GetToken(res).Position, GetToken(res, 1).Value);

        res = Accept(TokenType.Dot, TokenType.Literal);
        while (res.Success)
        {
            ns.Value.Add(GetToken(res, 1).Value);
            res = Accept(TokenType.Dot, TokenType.Literal);
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in namespace", res);

        return new ParseResult<Statement>(ns);
    }

    private ParseResult<Expression> ParseParenthesizedExpression()
    {
        // Discard the opening parenthesis
        Next();
        var expression = ParseExpression();
        var res = Accept(TokenType.RightParenthesis);
        if (res.Failure)
            return InvalidTokenErrorExpression("Invalid token in parenthesized expression", res);
        return expression;
    }

    private ParseResult<Expression> ParsePrimaryExpression(bool acceptIs)
    {
        ParseResult<Expression> leftResult;
        if (Peek.Type == TokenType.LeftParenthesis)
            leftResult = ParseParenthesizedExpression();
        else if (Peek.Type == TokenType.Literal)
            leftResult = ParseIdentifier();
        else if (Peek.Type == TokenType.StringStart)
            leftResult = ParseString();
        else if (Peek.Type == TokenType.New)
            leftResult = ParseConstructorCall();
        else if (Peek.Type == TokenType.Function)
            leftResult = ParseAnonymouseFunction();
        else if (Peek.Type.IsUnaryOperator())
            leftResult = ParseUnaryOperator();
        else if (Peek.Type.IsType())
            leftResult = ParseTypeToken();
        else if (Peek.Type == TokenType.LeftBracket)
            leftResult = ParseArray();
        else
        {
            var token = Next();
            leftResult = token.Type switch
            {
                TokenType.CharacterLiteral => new ParseResult<Expression>(new Character(token.Position, token.Value)),
                TokenType.Discard => new ParseResult<Expression>(new Discard(token.Position)),
                TokenType.False => new ParseResult<Expression>(new LiteralToken(token.Position, Literal.False)),
                TokenType.Null => new ParseResult<Expression>(new LiteralToken(token.Position, Literal.Null)),
                TokenType.NumberLiteral => new ParseResult<Expression>(new Number(token.Position, token.Value)),
                TokenType.This => new ParseResult<Expression>(new CurrentObjectInstance(token.Position)),
                TokenType.True => new ParseResult<Expression>(new LiteralToken(token.Position, Literal.True)),
                _ => ErrorExpression("Invalid token in expression: " + token, token.Position)
            };
        }
        if (leftResult.Error)
            return leftResult;

        while (Peek.Type == TokenType.LeftParenthesis ||
            Peek.Type == TokenType.LeftBracket ||
            Peek.Type == TokenType.LeftCurly ||
            (acceptIs && Peek.Type == TokenType.Is) ||
            Peek.Type == TokenType.Conditional)
        {
            if (Peek.Type == TokenType.LeftParenthesis)
                leftResult = ParseMethodCall(leftResult.Result);
            else if (Peek.Type == TokenType.LeftBracket)
                leftResult = ParseIndexer(leftResult.Result);
            else if (Peek.Type == TokenType.LeftCurly)
                leftResult = ParseGeneric(leftResult.Result);
            else if (acceptIs && Peek.Type == TokenType.Is)
                leftResult = ParseIs(leftResult.Result);
            else if (Peek.Type == TokenType.Conditional)
                leftResult = ParseConditional(leftResult.Result);
        }

        return leftResult;
    }

    private ParseResult<Statement> ParseProperty(List<Modifier> modifiers, Token nameToken)
    {
        // property
        // [modifiers] fn [name] [type] [= [expr]]
        // [modifiers] fn [name] [type] [get and/or set] [= [expr]]
        // [modifiers] fn [name] [type] is [expr]
        // [modifiers] fn [name] [type]
        //     [statements]
        // [modifiers] fn [name] [type] [= [expr]]
        //     get is [expr]
        //     set is [statement]
        // [modifiers] fn [name] [type] [= [expr]]
        //     get
        //         [statements]
        //     set(v)
        //         [statements]

        AcceptResult res;

        var typeResult = ParseType();
        if (typeResult.Error && typeResult.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        var propDef = new Property(nameToken.Position, modifiers, nameToken.Value, typeResult.Result);

        // [modifiers] fn [name] [type] [get and/or set]
        if (Peek.Type == TokenType.Get || Peek.Type == TokenType.Set)
        {
            var getSet = Next();
            var other = TokenType.Set;
            if (getSet.Type == TokenType.Get)
                propDef.Set = false;
            else
            {
                propDef.Get = false;
                other = TokenType.Get;
            }

            res = Accept(TokenType.Comma, other);
            if (res.Success)
            {
                if (GetToken(res, 1).Type == TokenType.Get)
                    propDef.Get = true;
                else
                    propDef.Set = true;
            }
        }
        else if (Accept(TokenType.Is).Success)
        {
            // [is [expr]]

            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp);
            propDef.GetStatements.Add(new Return(exprResult.Result.Position) { Value = exprResult.Result });
            res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in property", res);
            return new ParseResult<Statement>(propDef);
        }

        // [= [expr]]
        if (Accept(TokenType.Assign).Success)
        {
            var exprResult = ParseExpression();
            if (exprResult.Error && exprResult.Result is ErrorExpression exp)
                return ErrorStatement(exp);
            propDef.Value = exprResult.Result;
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in property", res);

        if (Accept(TokenType.Indent).Success)
        {
            if (Peek.Type == TokenType.Get || Peek.Type == TokenType.Set)
            {
                // [modifiers] fn [name] [type]
                //     get is [expr]
                //     set(v) is [statement]
                // [modifiers] fn [name] [type]
                //     get
                //         [statements]
                //     set(v)
                //         [statements]

                var other = Peek.Type == TokenType.Get ? TokenType.Set : TokenType.Get;

                var getSetResult = ParseGetSet(propDef);
                if (getSetResult != null)
                    return getSetResult;

                if (Peek.Type == other)
                {
                    getSetResult = ParseGetSet(propDef);
                    if (getSetResult != null)
                        return getSetResult;
                }
            }
            else
            {
                // [modifiers] fn [name] [type]
                //     [statements]

                while (Peek.Type != TokenType.Dedent)
                {
                    var stmt = ParseMethodStatement();
                    if (stmt.Error)
                        return stmt;
                    propDef.GetStatements.Add(stmt.Result);
                }
            }
            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in property", res);
        }

        return new ParseResult<Statement>(propDef);
    }

    private ParseResult<Statement> ParsePropertySignature(List<Modifier> modifiers, Position pos, string name)
    {
        // [modifiers] fn [name] [type]
        // [modifiers] fn [name] [type] [get and/or set]

        AcceptResult res;

        var typeResult = ParseType();
        if (typeResult.Error && typeResult.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        var propSig = new PropertySignature(pos, modifiers, name, typeResult.Result);

        if (Peek.Type == TokenType.Get || Peek.Type == TokenType.Set)
        {
            var getSet = Next();
            var other = TokenType.Set;
            if (getSet.Type == TokenType.Get)
                propSig.Set = false;
            else
            {
                propSig.Get = false;
                other = TokenType.Get;
            }

            res = Accept(TokenType.Comma, other);
            if (res.Success)
            {
                if (GetToken(res, 1).Type == TokenType.Get)
                    propSig.Get = true;
                else
                    propSig.Set = true;
            }
        }

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in property signature", res);

        return new ParseResult<Statement>(propSig);
    }

    private ParseResult<Statement> ParseReturn(bool acceptEol)
    {
        var start = Next();     // accept return
        var ret = new Return(start.Position);
        if (Peek.Type != TokenType.EOL)
        {
            var expr = ParseExpression();
            if (expr.Error && expr.Result is ErrorExpression ex)
                return ErrorStatement(ex);
            ret.Value = expr.Result;
        }
        if (acceptEol)
        {
            var res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in return", res);
        }
        return new ParseResult<Statement>(ret);
    }

    private ParseResult<Statement> ParseSpace()
    {
        var position = Peek.Position;
        int size = 0;
        while (Peek.Type == TokenType.EOL)
        {
            Next();
            size++;
        }
        return new ParseResult<Statement>(new Space(position, size));
    }

    private ParseResult<Expression> ParseString()
    {
        var stringResult = new Core.Ast.String(Peek.Position);
        AcceptResult res;

        do
        {
            var currentLine = new List<Expression>();
            stringResult.Lines.Add(currentLine);

            res = Accept(TokenType.StringStart);
            if (res.Failure)
                return InvalidTokenErrorExpression("Invalid token in string", res);

            while (Peek.Type != TokenType.StringEnd)
            {
                if (Peek.Type == TokenType.StringLiteral)
                {
                    currentLine.Add(new StringLiteral(Peek.Position, Peek.Value));
                    Next();
                }
                else if (Accept(TokenType.LeftCurly).Success)
                {
                    var nestedExpression = ParseExpression();
                    if (nestedExpression.Error)
                        return nestedExpression;
                    currentLine.Add(nestedExpression.Result);
                    res = Accept(TokenType.RightCurly);
                    if (res.Failure)
                        return InvalidTokenErrorExpression("Invalid token in string", res);
                }
                else
                    return ErrorExpression("Invalid token in string: " + Peek, Peek.Position);
            }
            res = Accept(TokenType.StringEnd);
            if (res.Failure)
                return InvalidTokenErrorExpression("Invalid token in string", res);
        } while (Accept(TokenType.DoubleDot).Success);

        return new ParseResult<Expression>(stringResult);
    }

    private ParseResult<Statement> ParseStruct(List<Modifier> modifiers)
    {
        Next();
        return ErrorStatement("Struct parsing not supported yet", new Position());
    }

    private ParseResult<Statement> ParseThrow(bool acceptEol)
    {
        var start = Next();     // accept throw
        var expr = ParseExpression();
        if (expr.Error && expr.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        var throwDef = new Throw(start.Position, expr.Result);

        if (acceptEol)
        {
            var res = Accept(TokenType.EOL);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in throw", res);
        }
        return new ParseResult<Statement>(throwDef);
    }

    private ParseResult<Statement> ParseTry()
    {
        // try
        //     [statements]
        // catch
        // catch [type]
        // catch [type] [name]
        //     [statements]
        // fin
        //     [statements]

        var res = Accept(TokenType.Try, TokenType.EOL, TokenType.Indent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in try", res);

        var tryDef = new Try(GetToken(res).Position);

        while (Peek.Type != TokenType.Dedent)
        {
            var stmt = ParseMethodStatement();
            if (stmt.Error)
                return stmt;
            tryDef.Statements.Add(stmt.Result);
        }

        res = Accept(TokenType.Dedent);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in try", res);

        var catchRes = Accept(TokenType.Catch);
        while (catchRes.Success)
        {
            var catchStmt = new Catch(GetToken(catchRes).Position);
            tryDef.Catches.Add(catchStmt);

            if (Peek.Type != TokenType.EOL)
            {
                var typeRes = ParseType();
                if (typeRes.Error && typeRes.Result is ErrorExpression ex)
                    return ErrorStatement(ex);

                catchStmt.Type = typeRes.Result;
                res = Accept(TokenType.Literal);
                if (res.Success)
                    catchStmt.Name = GetToken(res).Value;
            }

            res = Accept(TokenType.EOL, TokenType.Indent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in catch", res);

            while (Peek.Type != TokenType.Dedent)
            {
                var stmt = ParseMethodStatement();
                if (stmt.Error)
                    return stmt;
                catchStmt.Statements.Add(stmt.Result);
            }

            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in catch", res);

            catchRes = Accept(TokenType.Catch);
        }

        if (Accept(TokenType.Finally, TokenType.EOL, TokenType.Indent).Success)
        {
            while (Peek.Type != TokenType.Dedent)
            {
                var stmt = ParseMethodStatement();
                if (stmt.Error)
                    return stmt;
                tryDef.FinallyStatements.Add(stmt.Result);
            }

            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in finally", res);
        }

        return new ParseResult<Statement>(tryDef);
    }

    private ParseResult<Expression> ParseType()
    {
        ParseResult<Expression> leftResult;
        if (Peek.Type == TokenType.LeftBracket)
            return ParseArray();
        else if (Peek.Type.IsType())
            leftResult = ParseTypeToken();
        else if (Peek.Type == TokenType.Literal)
            leftResult = ParseIdentifier();
        else
            return ErrorExpression("Invalid token in type: " + Peek.Type, Peek.Position);

        if (leftResult.Error)
            return leftResult;

        while (Peek.Type == TokenType.LeftCurly || Peek.Type == TokenType.Dot)
        {
            if (Peek.Type == TokenType.LeftCurly)
                leftResult = ParseGeneric(leftResult.Result);

            var res = Accept(TokenType.Dot, TokenType.Literal);
            if (res.Success)
            {
                leftResult = new ParseResult<Expression>(new BinaryOperator(
                    leftResult.Result.Position,
                    BinaryOperatorType.Dot,
                    leftResult.Result,
                    new Identifier(GetToken(res, 1).Position, GetToken(res, 1).Value)));
            }
        }
        return leftResult;
    }

    private ParseResult<Expression> ParseTypeToken()
    {
        Identifier ident = new Identifier(Peek.Position, Peek.Type switch
        {
            TokenType.String => "String",
            TokenType.Character => "Char",
            TokenType.Boolean => "Boolean",
            TokenType.I8 => "SByte",
            TokenType.I16 => "Int16",
            TokenType.I32 => "Int32",
            TokenType.I64 => "Int64",
            TokenType.U8 => "Byte",
            TokenType.U16 => "UInt16",
            TokenType.U32 => "UInt32",
            TokenType.U64 => "UInt64",
            TokenType.F32 => "Single",
            TokenType.F64 => "Double",
            TokenType.Decimal => "Decimal",
            _ => "Error",
        });
        var type = new BinaryOperator(Peek.Position, BinaryOperatorType.Dot, new Identifier(Peek.Position, "System"), ident);
        Next();
        return new ParseResult<Expression>(type);
    }

    private ParseResult<Expression> ParseUnaryOperator()
    {
        var op = Next();
        var exprResult = ParseExpression();
        if (exprResult.Error)
            return exprResult;
        return new ParseResult<Expression>(new UnaryOperator(op.Position, op.Type.ToUnaryOperator(), exprResult.Result));
    }

    private ParseResult<Statement> ParseUsing()
    {
        // use [name] = [expr]
        // use [name] = [expr]
        //     [statements]

        var res = Accept(TokenType.Use, TokenType.Literal, TokenType.Assign);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in use", res);

        var pos = GetToken(res).Position;
        var name = GetToken(res, 1).Value;

        var exprRes = ParseExpression();
        if (exprRes.Error && exprRes.Result is ErrorExpression ex)
            return ErrorStatement(ex);

        res = Accept(TokenType.EOL);
        if (res.Failure)
            return InvalidTokenErrorStatement("Invalid token in use", res);

        var use = new Using(pos, name, exprRes.Result);

        if (Accept(TokenType.Indent).Success)
        {
            while (Peek.Type != TokenType.Dedent)
            {
                var stmtRes = ParseMethodStatement();
                if (stmtRes.Error)
                    return stmtRes;
                use.Statements.Add(stmtRes.Result);
            }

            res = Accept(TokenType.Dedent);
            if (res.Failure)
                return InvalidTokenErrorStatement("Invalid token in use", res);
        }

        return new ParseResult<Statement>(use);
    }
}