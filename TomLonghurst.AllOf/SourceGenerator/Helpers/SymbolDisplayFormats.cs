using Microsoft.CodeAnalysis;

namespace TomLonghurst.AllOf.SourceGenerator.Helpers;

public static class SymbolDisplayFormats
{
    public static readonly SymbolDisplayFormat NamespaceAndType =
        new(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.IncludeTypeParameters
        );
    
    public static readonly SymbolDisplayFormat GenericBase =
        new(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.None
        );
}