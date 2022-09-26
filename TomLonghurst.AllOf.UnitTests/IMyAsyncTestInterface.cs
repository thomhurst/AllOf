using TomLonghurst.AllOf.SourceGenerator.Attributes;

namespace TomLonghurst.AllOf.UnitTests;

[GenerateAllOf]
public interface IMyAsyncTestInterface
{
    public Task BlahAsync(Func<string, Task> action);
}