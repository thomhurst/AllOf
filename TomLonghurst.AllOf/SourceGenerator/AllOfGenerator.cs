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
            
            var interfaceShortName = typeSymbol.ToDisplayString(SymbolDisplayFormats.GenericBase).Split('.').Last();
            var interfaceLongName = typeSymbol.ToDisplayString(SymbolDisplayFormats.NamespaceAndType);

            var guid = Guid.NewGuid().ToString("N");
            
            var generics = typeSymbol.TypeParameters.Any()
                ? $"<{string.Join(", ", typeSymbol.ToDisplayString(SymbolDisplayFormats.NamespaceAndType))}>"
                : string.Empty;
            
            if (_typesWritten.Contains(interfaceLongName))
            {
                continue;
            }
            
            _typesWritten.Add(interfaceLongName);
            
            codeWriter.WriteLine($"namespace {typeof(AllOfImpl<>).Namespace}");
            codeWriter.WriteLine("{");

            codeWriter.WriteLine($"internal class AllOf_{interfaceShortName}_Impl_{guid}{generics} : AllOfImpl<{interfaceLongName}>, AllOf_{interfaceShortName}_{guid}{generics}");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"public AllOf_{interfaceShortName}_Impl_{guid}(IEnumerable<{interfaceLongName}> items) : base(items)");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine("}");
            codeWriter.WriteLine();
            
            foreach (var methodSymbol in identifiedDecorator.MethodsInInterface)
            {
                var parametersWithType = methodSymbol.Parameters.Select(p =>
                    $"{GetRef(p.RefKind)} {string.Join(" ", p.RefCustomModifiers)} {string.Join(" ", p.CustomModifiers)} {p.Type.ToDisplayString(SymbolDisplayFormats.NamespaceAndType)} {p.Name}".Trim());

                var returnType = methodSymbol.ReturnsVoid 
                ? VoidKeyword
                : methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.NamespaceAndType) ;
                
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
            
            codeWriter.WriteLine($"public partial class {nameof(AllOfImpl<object>)}<T>");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"internal static void Register{Guid.NewGuid():N}()");
            codeWriter.WriteLine("{");
            codeWriter.WriteLine($"{nameof(AllOfImpl<object>.Implementations)}.TryAdd(typeof(T), typeof(AllOf_{interfaceShortName}_Impl_{guid}{generics}));");
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