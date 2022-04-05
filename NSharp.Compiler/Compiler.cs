using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Compiler;

public class Compiler
{
    private List<Class> Classes { get; set; } = new();
    // enums
    // interfaces
    // structs

    private List<Diagnostic> Diagnostics { get; set; } = new();

    private BlobBuilder IlBuilder { get; set; }
    private MetadataBuilder MetadataBuilder { get; set; }
    private MethodBodyStreamEncoder MethodBodyStream { get; set; }
    private InstructionEncoder IlEncoder { get; set; }
    private Dictionary<string, TypeReferenceHandle> Types { get; set; } = new();
    private Dictionary<string, TypeDefinitionHandle> TypeDefinitions { get; set; } = new();

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
                    Classes.Add(new Class(currentNamespace, cl));
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
            default(EntityHandle),
            MetadataTokens.FieldDefinitionHandle(1),
            MetadataTokens.MethodDefinitionHandle(1));

        var runtimeAssemblyRef = MetadataBuilder.AddAssemblyReference(
            MetadataBuilder.GetOrAddString("System.Runtime"),
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
            runtimeAssemblyRef,
            MetadataBuilder.GetOrAddString("System"),
            MetadataBuilder.GetOrAddString("Console"));

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
            CompileClass(state, cl);

        return Diagnostics;
    }

    public void Save()
    {
        using var peStream = new FileStream("test.dll", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        var peHeaderBuilder = new PEHeaderBuilder(
            imageCharacteristics:
                Characteristics.Dll |
                Characteristics.ExecutableImage |
                Characteristics.LargeAddressAware);

        var peBuilder = new ManagedPEBuilder(
            peHeaderBuilder,
            new MetadataRootBuilder(MetadataBuilder),
            IlBuilder,
            deterministicIdProvider: content => AssemblyContentId);

        var peBlob = new BlobBuilder();
        var contentId = peBuilder.Serialize(peBlob);
        peBlob.WriteContentTo(peStream);
    }

    private void CompileClass(CompilerState state, Class cl)
    {
        string fullName = cl.Namespace + cl.Name;
        var attrs = TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit;
        if (cl.Modifiers.Contains(Modifier.Public))
            attrs |= TypeAttributes.Public;

        TypeDefinitions[fullName] = MetadataBuilder.AddTypeDefinition(
            attrs,
            MetadataBuilder.GetOrAddString(cl.Namespace),
            MetadataBuilder.GetOrAddString(cl.Name),
            Types["System.Object"],
            MetadataTokens.FieldDefinitionHandle(state.FieldDefinitionRow),
            MetadataTokens.MethodDefinitionHandle(state.MethodDefinitionRow));

        // state.FieldDefinitionRow += cl.Fields.Count;
        state.MethodDefinitionRow += cl.Methods.Count;

        foreach (var method in cl.Methods)
            CompileMethod(method);
    }

    private void CompileMethod(Core.Ast.MethodDefinition methodDefinition)
    {
        Action<ReturnTypeEncoder> returnType = (r) => r.Void();

        var signature = new BlobBuilder();
        new BlobEncoder(signature)
            .MethodSignature()
            .Parameters(0, returnType, parameters => { });

        var attrs = MethodAttributes.HideBySig;
        if (methodDefinition.Modifiers.Contains(Modifier.Static))
            attrs |= MethodAttributes.Static;

        if (methodDefinition.Modifiers.Contains(Modifier.Public))
            attrs |= MethodAttributes.Public;
        else if (methodDefinition.Modifiers.Contains(Modifier.Private))
            attrs |= MethodAttributes.Private;

        IlEncoder.OpCode(ILOpCode.Nop);
        int bodyOffset = MethodBodyStream.AddMethodBody(IlEncoder);
        IlEncoder.CodeBuilder.Clear();

        MethodDefinitionHandle mainMethodDef = MetadataBuilder.AddMethodDefinition(
            attrs,
            MethodImplAttributes.IL,
            MetadataBuilder.GetOrAddString(methodDefinition.Name),
            MetadataBuilder.GetOrAddBlob(signature),
            bodyOffset,
            default);
    }
}