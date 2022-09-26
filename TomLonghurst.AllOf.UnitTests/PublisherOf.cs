namespace TomLonghurst.AllOf.UnitTests;

// ReSharper disable once InconsistentNaming
public interface PublisherOf<out T>
{
    T ForEachSubscriber();
}