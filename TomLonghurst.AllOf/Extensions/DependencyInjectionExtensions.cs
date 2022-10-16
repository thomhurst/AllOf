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

        return services.AddTransient(typeof(AllOf<>), typeof(AllOfImpl<>));
    }
}