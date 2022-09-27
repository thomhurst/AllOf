using System.Data;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.AllOf.Models;

namespace TomLonghurst.AllOf.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAllOfs(this IServiceCollection services)
    {
        if (services.IsReadOnly)
        {
            throw new ReadOnlyException($"{nameof(services)} is read only");
        }

        var allTypes = GetAllTypes();
        
        RegisterAllOf(services, allTypes);
        RegisterUserTypes(services, allTypes);

        return services;
    }

    private static void RegisterAllOf(IServiceCollection services, List<Type> allTypes)
    {
        var allOfBaseInterface = typeof(AllOf<>);
        
        var internalTypes = new[] { typeof(IAllOf).FullName, typeof(AllOf<>).GetFullNameWithoutGenericArity() };
        
        foreach (var userDeclaredType in allTypes
                     .Where(type => !type.IsInterface)
                     .Where(type => type.GetInterfaces().Any(i => i.GetFullNameWithoutGenericArity() == "TomLonghurst.AllOf.Models.AllOf")))
        {
            var userDeclaredInterface = userDeclaredType.GetInterfaces()
                .First(i => !internalTypes.Contains(i.GetFullNameWithoutGenericArity())
                            && !i.GetInterfaces().Any(i2 => internalTypes.Contains(i2.GetFullNameWithoutGenericArity())));

            var allOfWithGenericTypeProvided = allOfBaseInterface.MakeGenericType(userDeclaredInterface);

            services.AddTransient(allOfWithGenericTypeProvided, userDeclaredType);
        }
    }

    private static void RegisterUserTypes(IServiceCollection services, List<Type> allTypes)
    {
        var allOfBaseInterface = typeof(IAllOf);

        foreach (var interfaceType in allTypes
                     .Where(type => type.IsInterface)
                     .Where(type => allOfBaseInterface != type)
                     .Where(type => allOfBaseInterface.IsAssignableFrom(type)))
        {
            foreach (var implementationType in allTypes
                         .Where(type => type.IsClass)
                         .Where(type => interfaceType.IsAssignableFrom(type)))
            {
                services.AddTransient(interfaceType, implementationType);
            }
        }
    }

    private static IReadOnlyList<Assembly> GetAllAssemblies()
    {
        var loadedAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .ToList();

        var otherReferencesAssemblies = loadedAssemblies
            .SelectMany(x => x.GetReferencedAssemblies())
            .Distinct()
            .Where(y => loadedAssemblies.Any(a => a.FullName == y.FullName) == false)
            .Select(name => AppDomain.CurrentDomain.Load(name))
            .ToList();
        
        loadedAssemblies.AddRange(otherReferencesAssemblies);

        return loadedAssemblies;
    }

    private static List<Type> GetAllTypes()
    {
        return GetAllAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .ToList();
    }
}