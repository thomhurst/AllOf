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

    private readonly List<string> _typesWritten = new();

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AllOfSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AllOfSyntaxReceiver syntaxReciever)
        {
            return;
        }
        
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif

        var source = GenerateSource(context, syntaxReciever);

        context.AddSource("AllOf.generated", SourceText.From(source, Encoding.UTF8));
    }

    /// <summary> 
    /// Your Main comment 
    /// <para><see cref="IEnumerable{T}"/></para> 
    /// <para>This is line 2</para> 
    /// </summary> 
    private string GenerateSource(GeneratorExecutionContext context, AllOfSyntaxReceiver syntaxReciever)
    {
        var codeWriter = new CodeGenerationTextWriter();
        
        codeWriter.WriteLine(context.GetUsingStatementsForTypes(
            syntaxReciever.Identified.Select(d => d.InterfaceType),
            typeof(DependencyInjectionExtensions),
            typeof(IAllOf),
            typeof(AllOf<>),
            typeof(AllOfImpl<>),
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

            if (_typesWritten.Contains(interfaceLongName))
            {
                continue;
            }
            
            _typesWritten.Add(interfaceLongName);
            
            codeWriter.WriteLine($"namespace {typeSymbol.ContainingNamespace}");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("/// <summary>");
            codeWriter.WriteLine($"/// A wrapper around an <see cref=\"IEnumerable{{{interfaceShortName}}}\"/> with the same methods as {interfaceShortName}.");
            codeWriter.WriteLine($"/// <para>Calling a method on this interface will call the same method on each item in the <see cref=\"IEnumerable{{{interfaceShortName}}}\"/></para>");
            codeWriter.WriteLine($"/// <para>Be sure to call .AddAllOfs() on your IServiceCollection to register this type</para>"); 
            codeWriter.WriteLine("/// </summary>");

            codeWriter.WriteLine($"public interface AllOf_{interfaceShortName} : AllOf<{interfaceLongName}>, {interfaceLongName}");
            codeWriter.WriteLine("{");
            
            codeWriter.WriteLine($"private class AllOf_{interfaceShortName}_Impl : AllOfImpl<{interfaceLongName}>, AllOf_{interfaceShortName}");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"public AllOf_{interfaceShortName}_Impl(IEnumerable<{interfaceLongName}> items) : base(items)");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
            codeWriter.WriteLine($"public override {interfaceShortName} OnEach() => this;");
            
            foreach (var methodSymbol in identifiedDecorator.MethodsInInterface)
            {
                var parametersWithType = methodSymbol.Parameters.Select(p =>
                    $"{GetRef(p.RefKind)} {string.Join(" ", p.RefCustomModifiers)} {string.Join(" ", p.CustomModifiers)} {p.Type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType)} {p.Name}".Trim());

                var returnType = methodSymbol.ReturnsVoid 
                ? VoidKeyword
                : methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) ;

                var asyncKeyword = methodSymbol.ReturnsVoid ? string.Empty : AsyncKeyword;

                codeWriter.WriteLine("/// <summary>");
                codeWriter.WriteLine($"/// Calls {methodSymbol.Name} on each item in the <see cref=\"IEnumerable{{{interfaceShortName}}}\"/>");
                codeWriter.WriteLine("/// </summary>");
                codeWriter.WriteLine( $"public {returnType} {methodSymbol.Name}{GetGenericType(methodSymbol)}({string.Join(", ", parametersWithType)})");
                codeWriter.WriteLine("{");
                GenerateBody(codeWriter, methodSymbol);
                codeWriter.WriteLine("}");
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
        
        if (methodSymbol.ReturnsVoid)
        {
            codeWriter.WriteLine("foreach (var item in Items)");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"item.{methodSymbol.Name}({string.Join(", ", parameters)});");
            codeWriter.WriteLine("}");
        }
        else if(methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) == typeof(Task).FullName)
        {
            codeWriter.WriteLine($"return Task.WhenAll(Items.Select(item => item.{methodSymbol.Name}({string.Join(", ", parameters)})));");
        }
        else
        {
            codeWriter.WriteLine($"var tasks = Items.Select(item => item.{methodSymbol.Name}({string.Join(", ", parameters)}).AsTask());");
            codeWriter.WriteLine("return new ValueTask(Task.WhenAll(tasks));");
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