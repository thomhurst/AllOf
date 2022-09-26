namespace TomLonghurst.AllOf.UnitTests;

public class MyAsyncTestClass2 : IMyAsyncTestInterface
{
    public async Task BlahAsync(Func<string, Task> action)
    {
        await action(GetType().Name);
    }
}