using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Compiler;

public class Compiler
{
    private List<Class> Classes { get; set; } = new();
    private List<Class> ClassOrder { get; set; } = new();
    private Dictionary<string, Class> ClassLookup { get; set; } = new();
    // enums
    // interfaces
    // structs

    private List<Diagnostic> Diagnostics { get; set; } = new();

    private BlobBuilder IlBuilder { get; set; }
    private MetadataBuilder MetadataBuilder { get; set; }
    private MethodBodyStreamEncoder MethodBodyStream { get; set; }
    private InstructionEncoder IlEncoder { get; set; }
    private MethodDefinitionHandle? EntryPoint { get; set; }
    private Dictionary<string, EntityHandle> Types { get; set; } = new();
    private Dictionary<string, EntityHandle> Methods { get; set; } = new();

    private static readonly Guid AssemblyGuid = new Guid("87D4DBE1-1143-4FAD-AAB3-1001F92068E6");
    private static readonly BlobContentId AssemblyContentId = new BlobContentId(AssemblyGuid, 0x04030201);

    public Compiler()
    {
        IlBuilder = new BlobBuilder();
        MetadataBuilder = new MetadataBuilder();
        MethodBodyStream = new MethodBodyStreamEncoder(IlBuilder);
        IlEncoder = new InstructionEncoder(new BlobBuilder());
    }

    public void Add(Core.Ast.File file)
    {
        string currentNamespace = string.Empty;
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Core.Ast.Class cl:
                    // TODO - partial classes
                    var newClass = new Class(currentNamespace, cl);
                    Classes.Add(newClass);
                    ClassLookup[cl.Name] = newClass;
                    break;
                // enum
                // interface
                // struct
                case Namespace ns:
                    currentNamespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    public List<Diagnostic> Compile()
    {
        MetadataBuilder.AddModule(
            0,
            MetadataBuilder.GetOrAddString("test.dll"),
            MetadataBuilder.GetOrAddGuid(AssemblyGuid),
            default,
            default);

        var assemblyHandle = MetadataBuilder.AddAssembly(
            MetadataBuilder.GetOrAddString("test"),
            new Version(1, 0, 0, 0),
            default,
            default,
            0,
            AssemblyHashAlgorithm.None);

        MetadataBuilder.AddTypeDefinition(
            default,
            default,
            MetadataBuilder.GetOrAddString("<Module>"),
            default,
            MetadataTokens.FieldDefinitionHandle(1),
            MetadataTokens.MethodDefinitionHandle(1));

        var runtimeAssemblyRef = MetadataBuilder.AddAssemblyReference(
            MetadataBuilder.GetOrAddString("System.Runtime"),
            new Version(6, 0, 0, 0),
            default,
            MetadataBuilder.GetOrAddBlob(new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A }),
            default,
            default);

        var consoleAssemblyRef = MetadataBuilder.AddAssemblyReference(
            MetadataBuilder.GetOrAddString("System.Console"),
            new Version(6, 0, 0, 0),
            default,
            MetadataBuilder.GetOrAddBlob(new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A }),
            default,
            default);

        Types["System.Object"] = MetadataBuilder.AddTypeReference(
            runtimeAssemblyRef,
            MetadataBuilder.GetOrAddString("System"),
            MetadataBuilder.GetOrAddString("Object"));

        Types["System.Console"] = MetadataBuilder.AddTypeReference(
            consoleAssemblyRef,
            MetadataBuilder.GetOrAddString("System"),
            MetadataBuilder.GetOrAddString("Console"));

        var consoleWriteLineSig = new BlobBuilder();
        new BlobEncoder(consoleWriteLineSig)
            .MethodSignature()
            .Parameters(1, r => r.Void(), p => p.AddParameter().Type().String());
        Methods["System.Console.WriteLine"] = MetadataBuilder.AddMemberReference(Types["System.Console"],
            MetadataBuilder.GetOrAddString("WriteLine"),
            MetadataBuilder.GetOrAddBlob(consoleWriteLineSig));

        var defaultInterpolatedStringHandlerTypeRef = MetadataBuilder.AddTypeReference(
            runtimeAssemblyRef,
            MetadataBuilder.GetOrAddString("System.Runtime.CompilerServices"),
            MetadataBuilder.GetOrAddString("DefaultInterpolatedStringHandler"));
        Types["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler"] = defaultInterpolatedStringHandlerTypeRef;

        var fnSig = new BlobBuilder();
        new BlobEncoder(fnSig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(2, r => r.Void(), p =>
                {
                    p.AddParameter().Type().Int32();
                    p.AddParameter().Type().Int32();
                });
        Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler..ctor"] = MetadataBuilder.AddMemberReference(
            defaultInterpolatedStringHandlerTypeRef,
            MetadataBuilder.GetOrAddString(".ctor"),
            MetadataBuilder.GetOrAddBlob(fnSig));

        fnSig = new BlobBuilder();
        new BlobEncoder(fnSig)
            .MethodSignature(genericParameterCount: 1, isInstanceMethod: true)
            .Parameters(1, r => r.Void(), p =>
            {
                p.AddParameter().Type().GenericMethodTypeParameter(0);
            });

        Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendFormatted{T}"] = MetadataBuilder.AddMemberReference(
            defaultInterpolatedStringHandlerTypeRef,
            MetadataBuilder.GetOrAddString("AppendFormatted"),
            MetadataBuilder.GetOrAddBlob(fnSig));
        fnSig = new BlobBuilder();
        new BlobEncoder(fnSig).MethodSpecificationSignature(1).AddArgument().Int32();
        Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendFormatted{Int32}"] = MetadataBuilder.AddMethodSpecification(
            Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendFormatted{T}"],
            MetadataBuilder.GetOrAddBlob(fnSig));

        fnSig = new BlobBuilder();
        new BlobEncoder(fnSig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(1, r => r.Void(), p => p.AddParameter().Type().String());
        Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendLiteral"] = MetadataBuilder.AddMemberReference(
            defaultInterpolatedStringHandlerTypeRef,
            MetadataBuilder.GetOrAddString("AppendLiteral"),
            MetadataBuilder.GetOrAddBlob(fnSig));

        fnSig = new BlobBuilder();
        new BlobEncoder(fnSig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(0, r => r.Type().String(), p => { });
        Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.ToStringAndClear"] = MetadataBuilder.AddMemberReference(
            defaultInterpolatedStringHandlerTypeRef,
            MetadataBuilder.GetOrAddString("ToStringAndClear"),
            MetadataBuilder.GetOrAddBlob(fnSig));

        var targetFrameworkTypeRef = MetadataBuilder.AddTypeReference(
            runtimeAssemblyRef,
            MetadataBuilder.GetOrAddString("System.Runtime.Versioning"),
            MetadataBuilder.GetOrAddString("TargetFrameworkAttribute"));
        Types["System.Runtime.Versioning.TargetFrameworkAttribute"] = targetFrameworkTypeRef;

        // TargetFrameworkAttribute
        var targetFrameworkCtorSig = new BlobBuilder();
        new BlobEncoder(targetFrameworkCtorSig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(1, r => r.Void(), p => p.AddParameter().Type().String());
        var targetFrameworkMemberRef = MetadataBuilder.AddMemberReference(
            targetFrameworkTypeRef,
            MetadataBuilder.GetOrAddString(".ctor"),
            MetadataBuilder.GetOrAddBlob(targetFrameworkCtorSig));

        var targetFrameworkBuilder = new BlobBuilder();
        new BlobEncoder(targetFrameworkBuilder).CustomAttributeSignature(
            fa => fa.AddArgument().Scalar().Constant(".NETCoreApp,Version=v6.0"),
            na => na.Count(1).AddArgument(
                false,
                t => t.ScalarType().String(),
                n => n.Name("FrameworkDisplayName"),
                l => l.Scalar().Constant("")));
        MetadataBuilder.AddCustomAttribute(
            assemblyHandle,
            targetFrameworkMemberRef,
            MetadataBuilder.GetOrAddBlob(targetFrameworkBuilder));

        var state = new CompilerState();
        foreach (var cl in Classes)
            CompileClassHeader(state, cl);

        state = new CompilerState();
        foreach (var cl in ClassOrder)
            CompileClass(cl);

        return Diagnostics;
    }

    public void Save()
    {
        using var peStream = new FileStream("c:\\test\\test.dll", FileMode.Create, FileAccess.ReadWrite);

        var peHeaderBuilder = new PEHeaderBuilder(imageCharacteristics: Characteristics.Dll | Characteristics.ExecutableImage | Characteristics.LargeAddressAware);

        var peBuilder = new ManagedPEBuilder(
            peHeaderBuilder,
            new MetadataRootBuilder(MetadataBuilder),
            IlBuilder,
            entryPoint: EntryPoint ?? default,
            deterministicIdProvider: content => AssemblyContentId);

        var peBlob = new BlobBuilder();
        var contentId = peBuilder.Serialize(peBlob);
        peBlob.WriteContentTo(peStream);

        System.IO.File.WriteAllText("c:\\test\\test.runtimeconfig.json", @"{
  ""runtimeOptions"": {
    ""tfm"": ""net6.0"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""6.0.0""
    }
  }
}");
    }

    private EntityHandle GetType(CompilerState state, string name)
    {
        if (Types.TryGetValue(name, out EntityHandle val))
            return val;

        // check for a class
        if (ClassLookup.TryGetValue(name, out Class? cl))
        {
            if (cl.Emitting)
                throw new Exception("Circular reference!");
            if (cl.Handle == null)
                CompileClassHeader(state, cl);
            if (cl.Handle != null)
                return cl.Handle.Value;
        }

        // TODO - full type resolution
        return default;
    }

    private void CompileClassHeader(CompilerState state, Class cl)
    {
        if (cl.Handle != null)
            return;

        cl.Emitting = true;

        var attrs = TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit;
        if (cl.Modifiers.Contains(Modifier.Public))
            attrs |= TypeAttributes.Public;

        cl.Handle = MetadataBuilder.AddTypeDefinition(
            attrs,
            MetadataBuilder.GetOrAddString(cl.Namespace),
            MetadataBuilder.GetOrAddString(cl.Name),
            GetType(state, cl.Parent == null ? "System.Object" : cl.Parent.ToDottedString()),
            MetadataTokens.FieldDefinitionHandle(state.FieldDefinitionRow),
            MetadataTokens.MethodDefinitionHandle(state.MethodDefinitionRow));
        Types[cl.Name] = cl.Handle.Value;

        // state.FieldDefinitionRow += cl.Fields.Count;
        state.MethodDefinitionRow += cl.Methods.Count;

        cl.Emitting = false;
        ClassOrder.Add(cl);
    }

    private void CompileClass(Class cl)
    {
        foreach (var method in cl.Methods)
            CompileMethod(method);
    }

    private void CompileMethod(Core.Ast.MethodDefinition methodDefinition)
    {
        bool isEntryPoint = false;

        Action<ReturnTypeEncoder> returnType = (r) => r.Void();

        var signature = new BlobBuilder();
        new BlobEncoder(signature)
            .MethodSignature()
            .Parameters(0, returnType, parameters => { });

        var attrs = MethodAttributes.HideBySig;
        if (methodDefinition.Modifiers.Contains(Modifier.Static))
        {
            attrs |= MethodAttributes.Static;
            isEntryPoint = methodDefinition.Name == "Main";
        }

        if (methodDefinition.Modifiers.Contains(Modifier.Public))
            attrs |= MethodAttributes.Public;
        else if (methodDefinition.Modifiers.Contains(Modifier.Private))
            attrs |= MethodAttributes.Private;

        foreach (var statement in methodDefinition.Statements)
        {
            switch (statement)
            {
                case ExpressionStatement expressionStatement:
                    Emit(expressionStatement.Expression);
                    break;
            }
        }

        IlEncoder.OpCode(ILOpCode.Ret);

        var localBuilder = new BlobBuilder();
        new BlobEncoder(localBuilder)
            .LocalVariableSignature(1)
            .AddVariable().Type().Type(Types["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler"], true);
        var locals = MetadataBuilder.AddStandaloneSignature(MetadataBuilder.GetOrAddBlob(localBuilder));

        int bodyOffset = MethodBodyStream.AddMethodBody(IlEncoder, localVariablesSignature: locals);
        IlEncoder.CodeBuilder.Clear();

        MethodDefinitionHandle methodDef = MetadataBuilder.AddMethodDefinition(
            attrs,
            MethodImplAttributes.IL | MethodImplAttributes.Managed,
            MetadataBuilder.GetOrAddString(methodDefinition.Name),
            MetadataBuilder.GetOrAddBlob(signature),
            bodyOffset,
            default);

        if (isEntryPoint)
            EntryPoint = methodDef;
    }

    private void Emit(Expression expression)
    {
        switch (expression)
        {
            case MethodCall methodCall:
                foreach (var param in methodCall.Parameters)
                    Emit(param);
                string key = methodCall.Target.ToString() ?? "";
                if (Methods.ContainsKey(key))
                    IlEncoder.Call(Methods[key]);
                break;
            case Number number:
                IlEncoder.LoadConstantI4(Convert.ToInt32(number.Value));
                break;
            case Core.Ast.String str:
                EmitString(str);
                break;
        }
    }

    private void EmitString(Core.Ast.String str)
    {
        int stringLength = 0;
        int expressionCount = 0;
        foreach (var line in str.Lines)
        {
            foreach (var expr in line)
            {
                if (expr is StringLiteral stringLiteral)
                    stringLength += stringLiteral.Value.Length;
                else
                    expressionCount++;
            }
        }
        stringLength += (str.Lines.Count - 1) * Environment.NewLine.Length;

        if (expressionCount > 0)
        {
            IlEncoder.LoadLocalAddress(0);
            IlEncoder.LoadConstantI4(stringLength);
            IlEncoder.LoadConstantI4(expressionCount);
            IlEncoder.Call(Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler..ctor"]);
        }

        bool firstLine = true;

        var buffer = new StringBuilder();
        foreach (var line in str.Lines)
        {
            if (!firstLine)
                buffer.Append(Environment.NewLine);
            foreach (var expr in line)
            {
                if (expr is StringLiteral stringLiteral)
                    buffer.Append(stringLiteral.Value);
                else
                {
                    if (buffer.Length > 0)
                    {
                        // emit the literal string so far
                        IlEncoder.LoadLocalAddress(0);
                        IlEncoder.LoadString(MetadataBuilder.GetOrAddUserString(buffer.ToString()));
                        IlEncoder.Call(Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendLiteral"]);
                        buffer.Clear();
                    }

                    IlEncoder.LoadLocalAddress(0);
                    Emit(expr);
                    IlEncoder.Call(Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendFormatted{Int32}"]);
                }
            }
            firstLine = false;
        }

        if (expressionCount == 0)
            IlEncoder.LoadString(MetadataBuilder.GetOrAddUserString(buffer.ToString()));
        else
        {
            if (buffer.Length > 0)
            {
                // emit the remaining literal string
                IlEncoder.LoadLocalAddress(0);
                IlEncoder.LoadString(MetadataBuilder.GetOrAddUserString(buffer.ToString()));
                IlEncoder.Call(Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendLiteral"]);
            }
            // finish up the DefaultInterpolatedStringHandler
            IlEncoder.LoadLocalAddress(0);
            IlEncoder.Call(Methods["System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.ToStringAndClear"]);
        }
    }
}