using System.Data;
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

        RegisterAllOf(services);
        RegisterUserTypes(services);

        return services;
    }

    private static void RegisterAllOf(IServiceCollection services)
    {
        var allOfBaseInterface = typeof(AllOf<>);

        var typesInAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .ToList();

        var internalTypes = new[] { typeof(IAllOf).FullName, typeof(AllOf<>).GetFullNameWithoutGenericArity() };
        
        foreach (var userDeclaredType in typesInAssemblies
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

    private static void RegisterUserTypes(IServiceCollection services)
    {
        var allOfBaseInterface = typeof(IAllOf);

        var typesInAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .ToList();

        foreach (var interfaceType in typesInAssemblies
                     .Where(type => type.IsInterface)
                     .Where(type => allOfBaseInterface != type)
                     .Where(type => allOfBaseInterface.IsAssignableFrom(type)))
        {
            foreach (var implementationType in typesInAssemblies
                         .Where(type => type.IsClass)
                         .Where(type => interfaceType.IsAssignableFrom(type)))
            {
                services.AddTransient(interfaceType, implementationType);
            }
        }
    }
}