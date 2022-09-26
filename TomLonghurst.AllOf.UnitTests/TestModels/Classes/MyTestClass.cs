using TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

namespace TomLonghurst.AllOf.UnitTests.TestModels.Classes;

public class MyTestClass : IMyTestInterface
{
    public void Blah(Action<string> action)
    {
        action(GetType().Name);
    }
}