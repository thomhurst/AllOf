namespace TomLonghurst.AllOf.Models;

public interface IAllOf
{
}
    
public interface IAllOf<T> : IAllOf
{
    IEnumerable<T> Items { get; }
}

public class AllOf<T> : IAllOf<T>
{
    private readonly IAllOfImplementationWrapper<T> _allOfImplementationWrapper;
    public IEnumerable<T> Items { get; }

    public AllOf(IEnumerable<T> items, IAllOfImplementationWrapper<T> allOfImplementationWrapper)
    {
        _allOfImplementationWrapper = allOfImplementationWrapper;
        Items = items;
    }

    public T OnEach() => _allOfImplementationWrapper.Value;

    public static implicit operator T(AllOf<T> allOf) => allOf.OnEach();
}

public interface IAllOfImplementationWrapper<T>
{
    public T Value { get; }
}

public class AllOfImplementationWrapper<T> : IAllOfImplementationWrapper<T>
{
    public T Value { get; }

    public AllOfImplementationWrapper(T value)
    {
        Value = value;
    }
}