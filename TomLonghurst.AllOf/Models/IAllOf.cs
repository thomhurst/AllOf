namespace TomLonghurst.AllOf.Models;

public interface IAllOf
{
}

public interface IAllOf<T> : IAllOf
{
    IEnumerable<T> Items { get; }
}