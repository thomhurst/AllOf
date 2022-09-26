namespace TomLonghurst.AllOf.UnitTests;

public class MyTestClass : IMyTestInterface
{
    public void Blah(Action<string> action)
    {
        action(GetType().Name);
    }
}