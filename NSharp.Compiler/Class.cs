using System.Reflection.Metadata;
using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Compiler;

public class Class
{
    public bool Emitting { get; set; }
    public EntityHandle? Handle { get; set; }
    public string Namespace { get; set; }
    public List<ModifierType> Modifiers { get; set; }
    public string Name { get; set; }
    public Expression? Parent { get; set; }
    public List<Class> Classes { get; set; } = new();
    // enum
    // interface
    // struct
    public List<Core.Ast.MethodDefinition> Methods { get; set; } = new();
    // fields

    public Class(string ns, Core.Ast.Class cl)
    {
        Namespace = ns;
        Modifiers = cl.Modifiers;
        Name = cl.Name;
        Parent = cl.Parent;

        foreach (var statement in cl.Statements)
        {
            switch (statement)
            {
                case Core.Ast.Class c:
                    Classes.Add(new Class(ns, c));
                    break;
                // enum
                // interface
                // struct
                case Core.Ast.MethodDefinition methodDefinition:
                    Methods.Add(methodDefinition);
                    break;
            }
        }
    }
}