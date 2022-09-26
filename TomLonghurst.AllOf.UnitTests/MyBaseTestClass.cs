namespace TomLonghurst.AllOf.UnitTests;

public class MyBaseTestClass : IMyDummyTestInterface
{
    public int BlahCount { get; private set; }
    public void Blah()
    {
        BlahCount++;
    }
}