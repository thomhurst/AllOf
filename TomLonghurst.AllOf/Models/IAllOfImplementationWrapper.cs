namespace TomLonghurst.AllOf.Models;

public interface IAllOfImplementationWrapper<T>
{
    public T Value { get; }
}