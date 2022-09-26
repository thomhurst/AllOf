namespace TomLonghurst.AllOf.Models;

public abstract class AllOfImpl<T> : AllOf<T>
{
    public IEnumerable<T> Items { get; }

    protected AllOfImpl(IEnumerable<T> items)
    {
        Items = items;
    }

    public abstract T OnEach();

    public static implicit operator T(AllOfImpl<T> allOf) => allOf.OnEach();
}