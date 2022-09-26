using TomLonghurst.AllOf.SourceGenerator.Attributes;

namespace TomLonghurst.AllOf.UnitTests;

[GenerateAllOf]
public interface IMyDummyTestInterface
{
    void Blah();
}