using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TomLonghurst.AllOf.Extensions;
using TomLonghurst.AllOf.Models;
using TomLonghurst.AllOf.SourceGenerator.Helpers;

namespace TomLonghurst.AllOf.SourceGenerator;

[Generator]
public class AllOfGenerator : ISourceGenerator
{
    private const string AsyncKeyword = "async ";
    private const string VoidKeyword = "void";

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif
        
        context.RegisterForSyntaxNotifications(() => new AllOfAttributeSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AllOfAttributeSyntaxReceiver syntaxReciever)
        {
            return;
        }

        var source = GenerateSource(context, syntaxReciever);

        context.AddSource("AllOf.generated", SourceText.From(source, Encoding.UTF8));
    }

    /// <summary> 
    /// Your Main comment 
    /// <para><see cref="IEnumerable{T}"/></para> 
    /// <para>This is line 2</para> 
    /// </summary> 
    private string GenerateSource(GeneratorExecutionContext context, AllOfAttributeSyntaxReceiver syntaxReciever)
    {
        var codeWriter = new CodeGenerationTextWriter();
        
        codeWriter.WriteLine(context.GetUsingStatementsForTypes(
            syntaxReciever.Identified.Select(d => d.InterfaceType),
            typeof(DependencyInjectionExtensions),
            typeof(IAllOf),
            typeof(IAllOf<>),
            typeof(AllOf<>),
            typeof(IAllOfImplementationWrapper<>),
            typeof(AllOfImplementationWrapper<>),
            typeof(IEnumerable<>),
            typeof(Enumerable),
            typeof(string),
            typeof(Task),
            typeof(Task<>)
        ));   
        codeWriter.WriteLine();

        
        foreach (var identifiedDecorator in syntaxReciever.Identified.DistinctBy(d => d.InterfaceType))
        {
            var typeSymbol = identifiedDecorator.InterfaceType;
            
            var interfaceShortName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var interfaceLongName = typeSymbol.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);
            
            codeWriter.WriteLine($"namespace {typeSymbol.ContainingNamespace}");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("/// <summary>");
            codeWriter.WriteLine($"/// A wrapper around an <see cref=\"IEnumerable{{{interfaceShortName}}}\"/> with the same methods as {interfaceShortName}.");
            codeWriter.WriteLine($"/// <para>Calling a method on this interface will call the same method on each item in the <see cref=\"IEnumerable{{{interfaceShortName}}}\"/></para>");
            codeWriter.WriteLine($"/// <para>Be sure to call .AddAllOfs() on your IServiceCollection to register this type</para>"); 
            codeWriter.WriteLine("/// </summary>");

            codeWriter.WriteLine($"public interface AllOf_{interfaceShortName} : {interfaceLongName}, IAllOf<{interfaceLongName}>");
            codeWriter.WriteLine("{");
            
            codeWriter.WriteLine($"private class AllOf_{interfaceShortName}_Impl : AllOf_{interfaceShortName}");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"public IEnumerable<{interfaceLongName}> Items {{ get; }}");
            codeWriter.WriteLine();
            codeWriter.WriteLine($"public AllOf_{interfaceShortName}_Impl(IEnumerable<{interfaceLongName}> items)");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("Items = items;");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
            
            foreach (var methodSymbol in identifiedDecorator.MethodsInInterface)
            {
                var parametersWithType = methodSymbol.Parameters.Select(p =>
                    $"{GetRef(p.RefKind)} {string.Join(" ", p.RefCustomModifiers)} {string.Join(" ", p.CustomModifiers)} {p.Type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType)} {p.Name}".Trim());

                var returnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) ;
                
                if (returnType == "System.Void")
                {
                    returnType = VoidKeyword;
                }

                var asyncKeyword = methodSymbol.ReturnType.SpecialType == SpecialType.System_Void ? string.Empty : AsyncKeyword;

                codeWriter.WriteLine("/// <summary>");
                codeWriter.WriteLine($"/// Calls {methodSymbol.Name} on each item in the <see cref=\"IEnumerable{{{interfaceShortName}}}\"/>");
                codeWriter.WriteLine("/// </summary>");
                codeWriter.WriteLine( $"public {asyncKeyword}{returnType} {methodSymbol.Name}{GetGenericType(methodSymbol)}({string.Join(", ", parametersWithType)})");
                codeWriter.WriteLine("{");
                GenerateBody(codeWriter, methodSymbol);
                codeWriter.WriteLine("}");
                
                codeWriter.WriteLine();
                codeWriter.WriteLine($"public static implicit operator AllOf_{interfaceShortName}_Impl(AllOf<AllOf_{interfaceShortName}_Impl> allOf) => allOf.OnEach();"); 
                codeWriter.WriteLine();
                
                codeWriter.WriteLine();
            }
            
            codeWriter.WriteLine("}");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
        }

        return codeWriter.ToString();
    }

    private static void GenerateBody(TextWriter codeWriter, IMethodSymbol methodSymbol)
    {
        var parameters = methodSymbol.Parameters.Select(
            p => $"{GetRef(p.RefKind)} {p.Name}".Trim()
        );
        
        if (methodSymbol.ReturnType.SpecialType == SpecialType.System_Void)
        {
            codeWriter.WriteLine("foreach (var item in Items)");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"item.{methodSymbol.Name}({string.Join(", ", parameters)});");
            codeWriter.WriteLine("}");
        }
        else
        {
            codeWriter.WriteLine($"foreach (var task in Items.Select(item => item.{methodSymbol.Name}({string.Join(", ", parameters)})))");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("await task;");
            codeWriter.WriteLine("}");
        }
    }

    private static string GetGenericType(IMethodSymbol method)
    {
        if (method.TypeParameters.Any() != true)
        {
            return string.Empty;
        }

        var genericTypes = method.TypeParameters.Select(x => x.Name);
        return $"<{string.Join(", ", genericTypes)}>";
    }

    private static string GetRef(RefKind refKind)
    {
        return refKind switch
        {
            RefKind.None => string.Empty,
            RefKind.Ref => "ref",
            RefKind.Out => "out",
            RefKind.In => "in",
            _ => throw new ArgumentOutOfRangeException(nameof(refKind), refKind, null)
        };
    }
}