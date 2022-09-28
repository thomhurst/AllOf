using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomLonghurst.AllOf.Extensions;
using TomLonghurst.AllOf.Models;
using TomLonghurst.AllOf.SourceGenerator.Helpers;

namespace TomLonghurst.AllOf.SourceGenerator;

internal class AllOfSyntaxReceiver : ISyntaxContextReceiver
{
    public AllOfSyntaxReceiver()
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif    
    }
    
    public List<IndentifiedAllOf> Identified { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is TypeSyntax typeSyntax)
        {
            Process(context, typeSyntax);
        }
    }

    private void Process(GeneratorSyntaxContext context, TypeSyntax typeSyntax)
    {
        var symbol = GetSymbol(context, typeSyntax);

        if (symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }
        
        var interfaceArguments = typeSymbol.TypeArguments
            .OfType<INamedTypeSymbol>()
            .Where(nts => nts.TypeKind == TypeKind.Interface)
            .Where(HasNoReturnTypes);

        foreach (var interfaceArgument in interfaceArguments)
        {
            var methods = GetMethods(interfaceArgument);
        
            Identified.Add(new IndentifiedAllOf
            {
                InterfaceType = interfaceArgument,
                MethodsInInterface = methods
            });
        }

        if (symbol.ToDisplayString(SymbolDisplayFormats.GenericBase) ==
            typeof(AllOf<>).GetFullNameWithoutGenericArity())
        {
            
        }

        
    }

    private bool HasNoReturnTypes(INamedTypeSymbol type)
    {
        var members = GetMembers(type);
        
        if (members.OfType<IPropertySymbol>().Any())
        {
            return false;
        }

        return members.OfType<IMethodSymbol>().All(m =>
        {
            if (m.ReturnsVoid)
            {
                return true;
            }

            var fullTypeName = m.ReturnType.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);
            return fullTypeName == typeof(Task).FullName || fullTypeName == typeof(ValueTask).FullName;
        });
    }

    private static ImmutableArray<ISymbol> GetMembers(INamedTypeSymbol type)
    {
        return type.GetMembers().Concat(
                type.AllInterfaces.SelectMany(i => i.GetMembers())
            )
            .ToImmutableArray();
    }

    private ISymbol? GetSymbol(GeneratorSyntaxContext context, SyntaxNode syntaxNode)
    {
        return context.SemanticModel.GetDeclaredSymbol(syntaxNode) ?? context.SemanticModel.GetSymbolInfo(syntaxNode).Symbol;
    }

    private static List<IMethodSymbol> GetMethods(ITypeSymbol interfaceSymbol)
    {
        var interfaceMembers = interfaceSymbol.GetMembers();

        var methods = interfaceMembers
            .OfType<IMethodSymbol>()
            .ToList();

        var allowedTypes = new[]
        {
            typeof(Task).FullName,
            typeof(void).FullName,
            typeof(ValueTask).FullName
        };

        var returnTypeExceptions = methods
            .Where(m => !allowedTypes.Contains(m.ReturnType.ToDisplayString(SymbolDisplayFormats.NamespaceAndType)))
            .Select(m =>
                new ArgumentException(
                    $"Only void or Task return types are supported. Cannot convert IEnumerable<{m.ReturnType}> to {m.ReturnType}")
            ).ToList();

        if (returnTypeExceptions.Any())
        {
            throw new AggregateException(returnTypeExceptions);
        }

        return methods;
    }
}