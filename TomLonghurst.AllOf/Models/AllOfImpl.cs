using System.Collections.Concurrent;

namespace TomLonghurst.AllOf.Models;

public static class AllOfImpl
{
    public static readonly ConcurrentDictionary<Type, Type> Implementations = new();
}

internal class AllOfImpl<T> : AllOf<T>
{
    private T GetRegisteredImplementation()
    {
        if (AllOfImpl.Implementations.TryGetValue(typeof(T), out var implementationType))
        {
            return (T) Activator.CreateInstance(implementationType, Items);
        }

        throw new ArgumentNullException(typeof(T).Name);
    }
    
    public IEnumerable<T> Items { get; }

    public AllOfImpl(IEnumerable<T> items)
    {
        Items = items;
    }

    public T OnEach()
    {
        return GetRegisteredImplementation();
    }

    public static implicit operator T(AllOfImpl<T> allOf) => allOf.OnEach();
}