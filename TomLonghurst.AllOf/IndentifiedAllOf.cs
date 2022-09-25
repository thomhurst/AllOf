using Microsoft.CodeAnalysis;

namespace TomLonghurst.AllOf;

public class IndentifiedAllOf
{
    public ITypeSymbol InterfaceType { get; set; }

    public IReadOnlyList<IMethodSymbol> MethodsInInterface { get; set; }
}