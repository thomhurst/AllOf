using TomLonghurst.AllOf.Models;

namespace TomLonghurst.AllOf.UnitTests;

public class ConstructorUsingInterfaceWithoutAttribute
{
    private readonly IMyInterfaceWithoutAttribute _myTestInterface;

    public ConstructorUsingInterfaceWithoutAttribute(AllOf<IMyInterfaceWithoutAttribute> myTestInterface)
    {
        _myTestInterface = myTestInterface.OnEach();
    }

    public void DoSomething()
    {
        _myTestInterface.DoSomething();
    }
}