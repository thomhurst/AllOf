using TomLonghurst.AllOf.Models;

namespace TomLonghurst.AllOf.UnitTests;

public class ConstructorImplicitConversion
{
    private readonly IMyTestInterface _myTestInterface;

    public ConstructorImplicitConversion(AllOf<IMyTestInterface> myTestInterface)
    {
        _myTestInterface = myTestInterface.OnEach();
    }

    public void DoSomething()
    {
        _myTestInterface.Blah(s => {});
    }
}