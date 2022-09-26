using TomLonghurst.AllOf.SourceGenerator.Attributes;

namespace TomLonghurst.AllOf.UnitTests.DependentProject;

[GenerateAllOf]
public interface IMyDependentInterface
{
    void MyDependentMethod(ref int result);
}