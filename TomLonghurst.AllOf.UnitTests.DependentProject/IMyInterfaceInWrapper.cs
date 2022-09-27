using TomLonghurst.AllOf.Models;

namespace TomLonghurst.AllOf.UnitTests.DependentProject;

public interface IMyInterfaceInWrapper
{
    void DoSomething();
}

public class MyClassInWrapper : IMyInterfaceInWrapper
{
    public void DoSomething()
    {
        
    }
}

public interface MyWrapper<out T>
{
    T Get();
}

public class MyWrapperImpl<T> : MyWrapper<T>
{
    private readonly AllOf<T> _allOf;

    public MyWrapperImpl(AllOf<T> allOf)
    {
        _allOf = allOf;
    }

    public T Get()
    {
        return _allOf.OnEach();
    }
}

public class MyConstructor
{
    private readonly MyWrapper<IMyInterfaceInWrapper> _blah;

    public MyConstructor(MyWrapper<IMyInterfaceInWrapper> blah)
    {
        _blah = blah;
    }
}