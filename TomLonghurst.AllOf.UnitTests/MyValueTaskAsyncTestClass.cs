namespace TomLonghurst.AllOf.UnitTests;

public class MyValueTaskAsyncTestClass : IMyValueTaskAsyncTestInterface
{
    public async ValueTask BlahAsync(Func<string, ValueTask> action)
    {
        await action(GetType().Name);
    }
}