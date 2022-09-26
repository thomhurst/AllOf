namespace TomLonghurst.AllOf.UnitTests;

public class MyTestClass3 : IMyTestInterface
{
    public void Blah(Action<string> action)
    {
        action(GetType().Name);
    }
}