using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomLonghurst.AllOf.Extensions;
using TomLonghurst.AllOf.Models;
using TomLonghurst.AllOf.SourceGenerator.Attributes;
using TomLonghurst.AllOf.SourceGenerator.Helpers;

namespace TomLonghurst.AllOf.SourceGenerator;

internal class AllOfAttributeSyntaxReceiver : ISyntaxContextReceiver
{
    public List<IndentifiedAllOf> Identified { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if(context.Node is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            Process(context, interfaceDeclarationSyntax);
        }
        
        if (context.Node is ConstructorDeclarationSyntax constructorDeclarationSyntax)
        {
            Process(context, constructorDeclarationSyntax);
        }

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

        if (typeSymbol.TypeArguments.Length != 1)
        {
            return;
        }

        if (typeSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase) !=
            typeof(AllOf<>).GetFullNameWithoutGenericArity())
        {
            return;
        }

        var interfaceArgument = typeSymbol.TypeArguments.First();

        if (interfaceArgument is not INamedTypeSymbol interfaceType)
        {
            return;
        }

        var methods = GetMethods(interfaceType);
        
        Identified.Add(new IndentifiedAllOf
        {
            InterfaceType = interfaceType,
            MethodsInInterface = methods
        });
    }

    private void Process(GeneratorSyntaxContext context, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        var symbol = GetSymbol(context, interfaceDeclarationSyntax);

        if (symbol is not INamedTypeSymbol interfaceSymbol)
        {
            return;
        }

        var attribute = interfaceSymbol
            .GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(GenerateAllOfAttribute).FullName
            );

        if (attribute is null)
        {
            return;
        }

        var methods = GetMethods(interfaceSymbol);

        Identified.Add(new IndentifiedAllOf
        {
            InterfaceType = interfaceSymbol,
            MethodsInInterface = methods,
        });
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

    private void Process(GeneratorSyntaxContext context, ConstructorDeclarationSyntax constructorDeclarationSyntax)
    {
        var parameters = constructorDeclarationSyntax.ParameterList
            .Parameters
            .Select(p => p.Type)
            .OfType<GenericNameSyntax>()
            .Where(gns => gns.Identifier.ToString() == "AllOf")
            .Where(gns => gns.TypeArgumentList.Arguments.Count == 1)
            .ToList();
        
        foreach (var genericNameSyntax in parameters)
        {
            var genericTypeSymbol = GetSymbol(context, genericNameSyntax);

            if (genericTypeSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase) != typeof(AllOf<>).GetFullNameWithoutGenericArity())
            {
                continue;
            }
            
            var typeArgument = genericNameSyntax.TypeArgumentList.Arguments.First();

            var argumentSymbol = GetSymbol(context, typeArgument);

            if (argumentSymbol is ITypeParameterSymbol)
            {
                FindGenericTypesOf(context, constructorDeclarationSyntax.Parent as ClassDeclarationSyntax);
                continue;
            }
            
            if (argumentSymbol is not INamedTypeSymbol argumentNamedTypeSymbol)
            {
                continue;
            }
            
            var methods = GetMethods(argumentNamedTypeSymbol);

            Identified.Add(new IndentifiedAllOf
            {
                InterfaceType = argumentNamedTypeSymbol,
                MethodsInInterface = methods,
            });
        }
    }

    private void FindGenericTypesOf(GeneratorSyntaxContext context, ClassDeclarationSyntax? syntax)
    {
        var classSymbol = GetSymbol(context, syntax) as INamedTypeSymbol;

        var interfaceSymbol = classSymbol?.Interfaces.FirstOrDefault();

        var namedTypeSymbols = syntax.Ancestors()
            .SelectMany(a => a.DescendantNodes())
            .OfType<TypeSyntax>()
            .Select(ts => GetSymbol(context, ts))
            .OfType<INamedTypeSymbol>()
            .ToList();
        
        var typeSymbols = namedTypeSymbols
            .Where(nts => nts.ToDisplayString(SymbolDisplayFormats.GenericBase) == interfaceSymbol?.ToDisplayString(SymbolDisplayFormats.GenericBase));
        
        foreach (var typeSyntax in typeSymbols)
        {
            var argumentSymbol = typeSyntax.TypeArguments.FirstOrDefault();
            
            if (argumentSymbol is not INamedTypeSymbol argumentNamedTypeSymbol)
            {
                continue;
            }

            var methods = GetMethods(argumentNamedTypeSymbol);

            Identified.Add(new IndentifiedAllOf
            {
                InterfaceType = argumentNamedTypeSymbol,
                MethodsInInterface = methods,
            });
        }
    }

    private ISymbol? GetSymbol(GeneratorSyntaxContext context, SyntaxNode syntaxNode)
    {
        return context.SemanticModel.GetDeclaredSymbol(syntaxNode) ?? context.SemanticModel.GetSymbolInfo(syntaxNode).Symbol;
    }
}