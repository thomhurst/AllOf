namespace TomLonghurst.AllOf.UnitTests.WrappedAllOf;

public class Impl3 : IInterfaceToWrap
{
    public void Blah()
    {
        Console.WriteLine(GetType().Name);
    }
}