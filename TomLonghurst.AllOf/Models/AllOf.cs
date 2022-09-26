namespace TomLonghurst.AllOf.Models;

// ReSharper disable once InconsistentNaming
public interface AllOf<out T> : IAllOf
{
    IEnumerable<T> Items { get; }
    public T OnEach();
}