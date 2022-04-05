using NSharp.Core;
using NSharp.Core.Ast;

namespace NSharp.Compiler;

public class Class
{
    public string Namespace { get; set; }
    public List<Modifier> Modifiers { get; set; }
    public string Name { get; set; }
    public List<Class> Classes { get; set; } = new();
    // enum
    // interface
    // struct
    public List<MethodDefinition> Methods { get; set; } = new();
    // fields

    public Class(string ns, Core.Ast.Class cl)
    {
        Namespace = ns;
        Modifiers = cl.Modifiers;
        Name = cl.Name;

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
                case MethodDefinition methodDefinition:
                    Methods.Add(methodDefinition);
                    break;
            }
        }
    }
}