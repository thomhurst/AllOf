using Microsoft.CodeAnalysis;

namespace TomLonghurst.AllOf.SourceGenerator;

public class IndentifiedAllOf
{
    public INamedTypeSymbol? InterfaceType { get; set; }

    public IReadOnlyList<IMethodSymbol>? MethodsInInterface { get; set; }
}