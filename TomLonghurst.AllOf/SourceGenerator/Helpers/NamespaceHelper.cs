using Microsoft.CodeAnalysis;

namespace TomLonghurst.AllOf.SourceGenerator.Helpers;

public static class NamespaceHelper
{
    public static string GetUsingStatementsForTypes(this GeneratorExecutionContext context, params Type[] types)
    {
        return GetUsingStatementsForTypes(context, Array.Empty<ITypeSymbol>(), types);
    }
    
    public static string GetUsingStatementsForTypes(this GeneratorExecutionContext context, IEnumerable<ITypeSymbol> typeSymbols, params Type[] types)
    {
        var namespaces = types
            .Select(type => context.Compilation.GetTypeByMetadataName(type.FullName).ContainingNamespace.ToString())
            .Concat(typeSymbols.Select(type => type.ContainingNamespace.ToString()))
            .Distinct();

        return WriteUsingStatements(namespaces);
    }

    private static string WriteUsingStatements(IEnumerable<string> namespaces)
    {
        return string.Join(Environment.NewLine, namespaces.Select(@namespace => $"using {@namespace};"));
    }
}