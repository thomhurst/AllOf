using System.Collections.Concurrent;

namespace TomLonghurst.AllOf.Models;

public static class AllOfData
{
    public static readonly ConcurrentDictionary<Type, Type> Implementations = new();
}