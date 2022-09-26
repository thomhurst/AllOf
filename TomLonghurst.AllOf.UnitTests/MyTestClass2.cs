namespace TomLonghurst.AllOf.UnitTests;

public class MyTestClass2 : IMyTestInterface
{
    public void Blah(Action<string> action)
    {
        action(GetType().Name);
    }
}