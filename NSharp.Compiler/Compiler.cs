using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Compiler;

public class Compiler
{
    private List<Core.Ast.File> Files { get; set; } = new();
    private List<Diagnostic> Diagnostics { get; set; } = new();

    private BlobBuilder IlBuilder { get; set; } = new();
    private MetadataBuilder MetadataBuilder { get; set; } = new();
    private Dictionary<string, TypeReferenceHandle> Types { get; set; } = new();
    private Dictionary<string, TypeDefinitionHandle> TypeDefinitions { get; set; } = new();

    private static readonly Guid AssemblyGuid = new Guid("87D4DBE1-1143-4FAD-AAB3-1001F92068E6");
    private static readonly BlobContentId AssemblyContentId = new BlobContentId(AssemblyGuid, 0x04030201);

    public void Add(Core.Ast.File file) => Files.Add(file);

    public List<Diagnostic> Compile()
    {
        MetadataBuilder.AddModule(
            0,
            MetadataBuilder.GetOrAddString("test.dll"),
            MetadataBuilder.GetOrAddGuid(AssemblyGuid),
            default(GuidHandle),
            default(GuidHandle));

        MetadataBuilder.AddAssembly(
            MetadataBuilder.GetOrAddString("test"),
            version: new Version(1, 0, 0, 0),
            culture: default(StringHandle),
            publicKey: default(BlobHandle),
            flags: 0,
            hashAlgorithm: AssemblyHashAlgorithm.None);

        MetadataBuilder.AddTypeDefinition(
            default(TypeAttributes),
            default(StringHandle),
            MetadataBuilder.GetOrAddString("<Module>"),
            default(EntityHandle),
            MetadataTokens.FieldDefinitionHandle(1),
            MetadataTokens.MethodDefinitionHandle(1));

        AssemblyReferenceHandle mscorlibAssemblyRef = MetadataBuilder.AddAssemblyReference(
            MetadataBuilder.GetOrAddString("mscorlib"),
            new Version(4, 0, 0, 0),
            default(StringHandle),
            MetadataBuilder.GetOrAddBlob(new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }),
            default(AssemblyFlags),
            default(BlobHandle));

        Types["System.Object"] = MetadataBuilder.AddTypeReference(
            mscorlibAssemblyRef,
            MetadataBuilder.GetOrAddString("System"),
            MetadataBuilder.GetOrAddString("Object"));

        Types["System.Console"] = MetadataBuilder.AddTypeReference(
            mscorlibAssemblyRef,
            MetadataBuilder.GetOrAddString("System"),
            MetadataBuilder.GetOrAddString("Console"));

        // build types
        foreach (var file in Files)
            BuildTypeSignatures(file);

        // add interfaces and inheritance
        foreach (var file in Files)
            CompleteTypeSignatures(file);
        // build method signatures
        foreach (var file in Files)
            BuildMethodSignatures(file);
        // build method bodies
        foreach (var file in Files)
            BuildMethodBodies(file);

        return Diagnostics;
    }

    public void Save()
    {
        using var peStream = new FileStream("test.dll", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        var peHeaderBuilder = new PEHeaderBuilder();

        var peBuilder = new ManagedPEBuilder(
            peHeaderBuilder,
            new MetadataRootBuilder(MetadataBuilder),
            IlBuilder,
            deterministicIdProvider: content => AssemblyContentId);

        var peBlob = new BlobBuilder();
        var contentId = peBuilder.Serialize(peBlob);
        peBlob.WriteContentTo(peStream);
    }

    private void BuildTypeSignatures(Core.Ast.File file)
    {
        string currentNamespace = string.Empty;
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:
                    BuildClassSignature(cl, currentNamespace);
                    break;
                case Namespace ns:
                    currentNamespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private void CompleteTypeSignatures(Core.Ast.File file)
    {
        string currentNamespace = string.Empty;
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:

                    break;
                case Namespace ns:
                    currentNamespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private void BuildMethodSignatures(Core.Ast.File file)
    {
        string currentNamespace = string.Empty;
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:
                    BuildClassMethodSignatures(cl, currentNamespace);
                    break;
                case Namespace ns:
                    currentNamespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private void BuildMethodBodies(Core.Ast.File file)
    {
        string currentNamespace = string.Empty;
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:

                    break;
                case Namespace ns:
                    currentNamespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private int Handle = 2;

    private void BuildClassSignature(Class cl, string currentNamespace)
    {
        string name = currentNamespace + "." + cl.Name;

        var attrs = TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit;
        if (cl.Modifiers.Contains(Modifier.Public))
            attrs |= TypeAttributes.Public;

        TypeDefinitions[name] = MetadataBuilder.AddTypeDefinition(
            attrs,
            MetadataBuilder.GetOrAddString(currentNamespace),
            MetadataBuilder.GetOrAddString(cl.Name),
            Types["System.Object"],
            MetadataTokens.FieldDefinitionHandle(Handle),
            MetadataTokens.MethodDefinitionHandle(Handle));
        
        Handle += 1;
    }

    private void BuildClassMethodSignatures(Class cl, string currentNamespace)
    {
        string name = currentNamespace + "." + cl.Name;
        var classDefinition = TypeDefinitions[name];
        foreach (var statement in cl.Statements)
        {
            switch (statement)
            {
                case FunctionDefinition functionDefinition:
                    BuildMethodSignature(classDefinition, functionDefinition);
                    break;
            }
        }
    }

    private void BuildMethodSignature(TypeDefinitionHandle cl, FunctionDefinition functionDefinition)
    {
        MetadataBuilder.AddFieldDefinition(
                FieldAttributes.Assembly,
                MetadataBuilder.GetOrAddString("_count"),
                MetadataBuilder.GetOrAddBlob(BuildSignature(e => e.FieldSignature().Int32())));

        var signature = new BlobBuilder();

        new BlobEncoder(signature)
            .MethodSignature()
            .Parameters(0, returnType => returnType.Void(), parameters => { });

        MethodDefinitionHandle mainMethodDef = MetadataBuilder.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            MethodImplAttributes.IL,
            MetadataBuilder.GetOrAddString(functionDefinition.Name),
            MetadataBuilder.GetOrAddBlob(signature),
            -1,
            default(ParameterHandle));
    }

    private static BlobBuilder BuildSignature(Action<BlobEncoder> action)
    {
        var builder = new BlobBuilder();
        action(new BlobEncoder(builder));
        return builder;
    }
}