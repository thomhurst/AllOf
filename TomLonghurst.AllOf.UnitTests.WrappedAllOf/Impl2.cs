namespace TomLonghurst.AllOf.UnitTests.WrappedAllOf;

public class Impl2 : IInterfaceToWrap
{
    public void Blah()
    {
        Console.WriteLine(GetType().Name);
    }
}