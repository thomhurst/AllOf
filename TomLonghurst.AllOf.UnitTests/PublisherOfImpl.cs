using TomLonghurst.AllOf.Models;

namespace TomLonghurst.AllOf.UnitTests;

public class PublisherOfImpl<T> : PublisherOf<T>
{
    private readonly AllOf<T> _allOf;

    public PublisherOfImpl(AllOf<T> allOf)
    {
        _allOf = allOf;
    }
    
    public T ForEachSubscriber()
    {
        return _allOf.OnEach();
    }
}