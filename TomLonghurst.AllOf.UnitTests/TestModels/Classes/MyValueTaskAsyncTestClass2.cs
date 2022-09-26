using TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

namespace TomLonghurst.AllOf.UnitTests.TestModels.Classes;

public class MyValueTaskAsyncTestClass2 : IMyValueTaskAsyncTestInterface
{
    public async ValueTask BlahAsync(Func<string, ValueTask> action)
    {
        await action(GetType().Name);
    }
}