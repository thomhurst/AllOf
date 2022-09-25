using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomLonghurst.AllOf;

internal class AllOfAttributeSyntaxReceiver : ISyntaxContextReceiver
{
    public List<IndentifiedAllOf> Identified { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if(context.Node is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            Process(context, interfaceDeclarationSyntax);
        }
    }

    private void Process(GeneratorSyntaxContext context, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        var symbol = context.SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax);

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
                new ArgumentException($"Only void or Task return types are supported. Cannot convert IEnumerable<{m.ReturnType}> to {m.ReturnType}")
            ).ToList();

        if (returnTypeExceptions.Any())
        {
            throw new AggregateException(returnTypeExceptions);
        }
            
        Identified.Add(new IndentifiedAllOf
        {
            InterfaceType = interfaceSymbol,
            MethodsInInterface = methods,
        });
    }
}