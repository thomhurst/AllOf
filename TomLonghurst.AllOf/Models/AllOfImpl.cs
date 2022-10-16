using System.Collections.Concurrent;
using System.Reflection;

namespace TomLonghurst.AllOf.Models;

public partial class AllOfImpl<T> : AllOf<T>
{
    internal static readonly ConcurrentDictionary<Type, Type> Implementations = new();
    
    static AllOfImpl()
    {
        var registerMethods = typeof(AllOfImpl<>)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(m => m.Name.StartsWith("Register"));

        foreach (var registerMethod in registerMethods)
        {
            registerMethod.Invoke(null, null);
        }
    }

    private T GetRegisteredImplementation()
    {
        if (Implementations.TryGetValue(typeof(T), out var implementationType))
        {
            return (T) Activator.CreateInstance(implementationType, Items);
        }

        throw new ArgumentNullException(typeof(T).Name);
    }
    
    public IEnumerable<T> Items { get; }

    protected AllOfImpl(IEnumerable<T> items)
    {
        Items = items;
    }

    public T OnEach()
    {
        return GetRegisteredImplementation();
    }

    public static implicit operator T(AllOfImpl<T> allOf) => allOf.OnEach();
}