using TomLonghurst.AllOf.SourceGenerator.Attributes;

namespace TomLonghurst.AllOf.UnitTests;

[GenerateAllOf]
public interface IMyValueTaskAsyncTestInterface
{
    public ValueTask BlahAsync(Func<string, ValueTask> action);
}