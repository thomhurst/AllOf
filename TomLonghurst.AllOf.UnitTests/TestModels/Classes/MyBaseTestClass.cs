using TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

namespace TomLonghurst.AllOf.UnitTests.TestModels.Classes;

public class MyBaseTestClass : IMyDummyTestInterface
{
    public int BlahCount { get; private set; }
    public void Blah()
    {
        BlahCount++;
    }
}