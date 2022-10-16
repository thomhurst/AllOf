namespace TomLonghurst.AllOf.UnitTests.WrappedAllOf;

public class Impl1 : IInterfaceToWrap
{
    public void Blah()
    {
        Console.WriteLine(GetType().Name);
    }
}