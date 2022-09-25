namespace TomLonghurst.AllOf;

public interface IAllOf
{
}
    
public interface IAllOf<T> : IAllOf
{
    IEnumerable<T> Items { get; }
}