using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace TomLonghurst.AllOf;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAllOfs(this IServiceCollection services)
    {
        if (services.IsReadOnly)
        {
            throw new ReadOnlyException($"{nameof(services)} is read only");
        }

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

        return services;
    }
}