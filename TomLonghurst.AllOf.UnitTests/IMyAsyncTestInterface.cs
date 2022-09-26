using TomLonghurst.AllOf.SourceGenerator.Attributes;

namespace TomLonghurst.AllOf.UnitTests;

[GenerateAllOf]
public interface IMyAsyncTestInterface
{
    public Task BlahAsync(Func<string, Task> action);
}

public class MyAsyncTestClass : IMyAsyncTestInterface
{
    public async Task BlahAsync(Func<string, Task> action)
    {
        await action(GetType().Name);
    }
}
public class MyAsyncTestClass2 : IMyAsyncTestInterface
{
    public async Task BlahAsync(Func<string, Task> action)
    {
        await action(GetType().Name);
    }
}
public class MyAsyncTestClass3 : IMyAsyncTestInterface
{
    public async Task BlahAsync(Func<string, Task> action)
    {
        await action(GetType().Name);
    }
}