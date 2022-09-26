using TomLonghurst.AllOf.Models;
using TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

namespace TomLonghurst.AllOf.UnitTests;

public class ConstructorUsingInterfaceWithoutAttribute
{
    public AllOf<IMyInterfaceWithoutAttribute> MyTestInterface { get; }

    public ConstructorUsingInterfaceWithoutAttribute(AllOf<IMyInterfaceWithoutAttribute> myTestInterface)
    {
        MyTestInterface = myTestInterface;
    }

    public void DoSomething()
    {
    }
}