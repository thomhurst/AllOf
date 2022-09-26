using Microsoft.CodeAnalysis;

namespace TomLonghurst.AllOf.SourceGenerator;

public class IndentifiedAllOf
{
    public ITypeSymbol? InterfaceType { get; set; }

    public IReadOnlyList<IMethodSymbol>? MethodsInInterface { get; set; }
}