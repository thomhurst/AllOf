namespace TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

public interface IMyValueTaskAsyncTestInterface
{
    public ValueTask BlahAsync(Func<string, ValueTask> action);
}