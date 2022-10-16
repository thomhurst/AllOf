namespace TomLonghurst.AllOf.UnitTests.WrappedAllOf;

public interface PublisherOf<out T>
{
    T ForEachSubscriber();
}