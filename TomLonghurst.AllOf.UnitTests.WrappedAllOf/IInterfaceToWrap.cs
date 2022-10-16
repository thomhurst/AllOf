namespace TomLonghurst.AllOf.UnitTests.WrappedAllOf;

public interface IInterfaceToWrap
{
    void Blah();
}

public class Impl1 : IInterfaceToWrap
{
    public void Blah()
    {
        Console.WriteLine(GetType().Name);
    }
}

public class Impl2 : IInterfaceToWrap
{
    public void Blah()
    {
        Console.WriteLine(GetType().Name);
    }
}

public class Impl3 : IInterfaceToWrap
{
    public void Blah()
    {
        Console.WriteLine(GetType().Name);
    }
}