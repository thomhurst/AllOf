using TomLonghurst.AllOf.Models;

namespace TomLonghurst.AllOf.UnitTests;

public class ConstructorWithTConversion
{
    private readonly IMyTestInterface _myTestInterface;

    public ConstructorWithTConversion(AllOf<IMyTestInterface> myTestInterface)
    {
        _myTestInterface = myTestInterface.OnEach();
    }

    public void DoSomething()
    {
        _myTestInterface.Blah(s => {});
    }
}