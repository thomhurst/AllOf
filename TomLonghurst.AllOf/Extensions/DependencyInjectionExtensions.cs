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

        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes());
        
        var allOfBaseInterface = typeof(AllOf<>);

        var typesImplementingAllOf = allTypes
            .Where(x => x.IsClass)
            .Where(x => x.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Select(i => i.GetGenericTypeDefinition())
                .Contains(allOfBaseInterface))
            .Where(x => !(x.IsGenericType && x.GetGenericTypeDefinition() == typeof(AllOfImpl<>)))
            .ToList();

        foreach (var type in typesImplementingAllOf)
        {
            var allOfInterface = type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .First(i => i.GetGenericTypeDefinition() == allOfBaseInterface);
            
            var otherInterface = type.GetInterfaces()
                .First(i => i.GetInterfaces().Contains(allOfInterface)
                );

            services.AddTransient(allOfInterface, type);
            services.AddTransient(otherInterface, type);
        }

        return services;
    }
}