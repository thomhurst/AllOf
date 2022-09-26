using TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

namespace TomLonghurst.AllOf.UnitTests;

public class ConstructorWithAllOfWrapper
{
    public PublisherOf<IMyInterfaceWithoutAttribute> Publisher { get; }

    public ConstructorWithAllOfWrapper(PublisherOf<IMyInterfaceWithoutAttribute> publisher)
    {
        Publisher = publisher;
    }

    public void DoSomething()
    {
    }
}