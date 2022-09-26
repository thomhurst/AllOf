namespace TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

public interface IMyAsyncTestInterface
{
    public Task BlahAsync(Func<string, Task> action);
}