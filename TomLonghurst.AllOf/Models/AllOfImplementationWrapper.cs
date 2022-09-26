namespace TomLonghurst.AllOf.Models;

public class AllOfImplementationWrapper<T> : IAllOfImplementationWrapper<T>
{
    public T Value { get; }

    public AllOfImplementationWrapper(T value)
    {
        Value = value;
    }
}