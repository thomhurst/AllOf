using TomLonghurst.AllOf.SourceGenerator.Attributes;

namespace TomLonghurst.AllOf.UnitTests;

[GenerateAllOf]
public interface IMyTestInterface
{
    public void Blah(Action<string> action);
}