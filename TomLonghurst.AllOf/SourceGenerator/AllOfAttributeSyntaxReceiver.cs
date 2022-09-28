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
            Process(context, constructorDeclarationSyntax, typeof(AllOf<>).GetFullNameWithoutGenericArity());
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

    private void Process(GeneratorSyntaxContext context, ConstructorDeclarationSyntax constructorDeclarationSyntax, string typeNameWithGeneric)
    {
        var parameters = constructorDeclarationSyntax.ParameterList
            .Parameters
            .Select(p => p.Type)
            .OfType<GenericNameSyntax>()
            .Where(gns => gns.Identifier.ToString() == typeNameWithGeneric.Split('.').Last())
            .Where(gns => gns.TypeArgumentList.Arguments.Count == 1)
            .ToList();
        
        foreach (var genericNameSyntax in parameters)
        {
            var genericTypeSymbol = GetSymbol(context, genericNameSyntax);

            if (genericTypeSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase) != typeNameWithGeneric)
            {
                continue;
            }
            
            var typeArgument = genericNameSyntax.TypeArgumentList.Arguments.First();

            var argumentSymbol = GetSymbol(context, typeArgument);

            if (argumentSymbol is ITypeParameterSymbol)
            {
                FindGenericTypesOf(context, constructorDeclarationSyntax, argumentSymbol.Name);
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

    private void FindGenericTypesOf(GeneratorSyntaxContext context, ConstructorDeclarationSyntax syntax,
        string genericTypeName)
    {
        var classSyntax = syntax.Ancestors()
            .OfType<ClassDeclarationSyntax>()
            .First();

        var classSymbol = GetSymbol(context, classSyntax) as ITypeSymbol;

        var interfaceSymbol = classSymbol?.Interfaces.FirstOrDefault();

        var indexOfType = Array.FindIndex(interfaceSymbol?.TypeParameters.ToArray(), p => p.Name == genericTypeName);
        
        var otherConstructors = syntax.Ancestors()
            .SelectMany(a => a.DescendantNodes())
            .OfType<ConstructorDeclarationSyntax>();

        foreach (var otherConstructor in otherConstructors)
        {
            Process(context, otherConstructor, interfaceSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase));
        }
        
        return;

        var namedTypeSymbols = syntax.Ancestors()
            .SelectMany(a => a.DescendantNodes())
            .OfType<TypeSyntax>()
            .Select(ts => GetSymbol(context, ts))
            .OfType<INamedTypeSymbol>()
            .Distinct()
            .ToList();

        var typeSymbols = namedTypeSymbols
            .Where(nts =>
            {
                var genericBase = nts.ToDisplayString(SymbolDisplayFormats.GenericBase);
                return genericBase == interfaceSymbol?.ToDisplayString(SymbolDisplayFormats.GenericBase)
                       || genericBase == classSymbol?.ToDisplayString(SymbolDisplayFormats.GenericBase);
            });
        
        foreach (var typeSyntax in typeSymbols)
        {
            var argumentSymbol = typeSyntax.TypeArguments[indexOfType];
            
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