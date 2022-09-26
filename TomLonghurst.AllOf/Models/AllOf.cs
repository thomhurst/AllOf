namespace TomLonghurst.AllOf.Models;

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