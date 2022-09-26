﻿using Microsoft.CodeAnalysis;
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

        if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax)
        {
            Process(context, typeDeclarationSyntax);
        }
    }

    private void Process(GeneratorSyntaxContext context, TypeDeclarationSyntax typeDeclarationSyntax)
    {
        if (typeDeclarationSyntax.TypeParameterList?.Parameters.Count != 1)
        {
            return;
        }

        var firstType = typeDeclarationSyntax.TypeParameterList.Parameters.First();

        var nonGenericType = typeDeclarationSyntax.TypeParameterList.WithParameters(default);
        
        var symbol = context.SemanticModel.GetDeclaredSymbol(nonGenericType);

    }

    private void Process(GeneratorSyntaxContext context, TypeParameterSyntax typeParameterSyntax)
    {
        
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

        var methods = GetMethods(interfaceSymbol);

        Identified.Add(new IndentifiedAllOf
        {
            InterfaceType = interfaceSymbol,
            MethodsInInterface = methods,
        });
    }

    private static List<IMethodSymbol> GetMethods(INamedTypeSymbol interfaceSymbol)
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
            var genericTypeSymbol = context.SemanticModel.GetSymbolInfo(genericNameSyntax).Symbol;

            if (genericTypeSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase) != typeof(AllOf<>).GetFullNameWithoutGenericArity())
            {
                return;
            }
            
            var typeArgument = genericNameSyntax.TypeArgumentList.Arguments.First();

            var argumentSymbol = context.SemanticModel.GetSymbolInfo(typeArgument).Symbol;

            if (argumentSymbol is not INamedTypeSymbol argumentNamedTypeSymbol)
            {
                return;
            }
            
            var methods = GetMethods(argumentNamedTypeSymbol);

            Identified.Add(new IndentifiedAllOf
            {
                InterfaceType = argumentNamedTypeSymbol,
                MethodsInInterface = methods,
            });
        }
    }
}