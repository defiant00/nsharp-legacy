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
        var state = new CompilerState();
        foreach (var file in Files)
            BuildTypeSignatures(state, file);

        // add interfaces and inheritance
        state = new CompilerState();
        foreach (var file in Files)
            CompleteTypeSignatures(state, file);

        // build method signatures
        state = new CompilerState();
        foreach (var file in Files)
            BuildMethodSignatures(state, file);

        // build method bodies
        state = new CompilerState();
        foreach (var file in Files)
            BuildMethodBodies(state, file);

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

    private void BuildTypeSignatures(CompilerState state, Core.Ast.File file)
    {
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:
                    BuildClassSignature(state, cl);
                    break;
                case Namespace ns:
                    state.Namespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private void CompleteTypeSignatures(CompilerState state, Core.Ast.File file)
    {
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:

                    break;
                case Namespace ns:
                    state.Namespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private void BuildMethodSignatures(CompilerState state, Core.Ast.File file)
    {
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:
                    BuildClassMethodSignatures(state, cl);
                    break;
                case Namespace ns:
                    state.Namespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private void BuildMethodBodies(CompilerState state, Core.Ast.File file)
    {
        foreach (var statement in file.Statements)
        {
            switch (statement)
            {
                case Class cl:

                    break;
                case Namespace ns:
                    state.Namespace = ns.Name.ToDottedString();
                    break;
            }
        }
    }

    private void BuildClassSignature(CompilerState state, Class cl)
    {
        string name = state.Namespace + "." + cl.Name;

        var attrs = TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit;
        if (cl.Modifiers.Contains(Modifier.Public))
            attrs |= TypeAttributes.Public;

        TypeDefinitions[name] = MetadataBuilder.AddTypeDefinition(
            attrs,
            MetadataBuilder.GetOrAddString(state.Namespace),
            MetadataBuilder.GetOrAddString(cl.Name),
            Types["System.Object"],
            MetadataTokens.FieldDefinitionHandle(state.FieldDefinitionRow),
            MetadataTokens.MethodDefinitionHandle(state.MethodDefinitionRow));

        // Count fields and methods.
        foreach (var statement in cl.Statements)
        {
            switch (statement)
            {
                case Core.Ast.MethodDefinition:
                    state.MethodDefinitionRow++;
                    break;
            }
        }
    }

    private void BuildClassMethodSignatures(CompilerState state, Class cl)
    {
        string name = state.Namespace + "." + cl.Name;
        var classDefinition = TypeDefinitions[name];
        foreach (var statement in cl.Statements)
        {
            switch (statement)
            {
                case Core.Ast.MethodDefinition methodDefinition:
                    BuildMethodSignature(classDefinition, methodDefinition);
                    break;
            }
        }
    }

    private void BuildMethodSignature(TypeDefinitionHandle cl, Core.Ast.MethodDefinition methodDefinition)
    {
        var signature = new BlobBuilder();

        Action<ReturnTypeEncoder> returnType = (r) => r.Void();

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

        MethodDefinitionHandle mainMethodDef = MetadataBuilder.AddMethodDefinition(
            attrs,
            MethodImplAttributes.IL,
            MetadataBuilder.GetOrAddString(methodDefinition.Name),
            MetadataBuilder.GetOrAddBlob(signature),
            -1,
            default(ParameterHandle));
    }
}