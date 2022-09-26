using TomLonghurst.AllOf.SourceGenerator.Attributes;

namespace TomLonghurst.AllOf.UnitTests;

[GenerateAllOf]
public interface IMyTestInterface
{
    public void Blah(Action<string> action);
}

public class MyTestClass : IMyTestInterface
{
    public void Blah(Action<string> action)
    {
        action(GetType().Name);
    }
}
public class MyTestClass2 : IMyTestInterface
{
    public void Blah(Action<string> action)
    {
        action(GetType().Name);
    }
}
public class MyTestClass3 : IMyTestInterface
{
    public void Blah(Action<string> action)
    {
        action(GetType().Name);
    }
}