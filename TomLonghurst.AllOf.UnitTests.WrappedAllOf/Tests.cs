using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TomLonghurst.AllOf.Extensions;
using TomLonghurst.AllOf.Models;

namespace TomLonghurst.AllOf.UnitTests.WrappedAllOf;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IInterfaceToWrap, Impl1>()
            .AddSingleton<IInterfaceToWrap, Impl2>()
            .AddSingleton<IInterfaceToWrap, Impl3>()
            .AddTransient(typeof(PublisherOf<>), typeof(PublisherOfImpl<>))
            .AddAllOfs()
            .BuildServiceProvider();
        
        var myInterface = serviceProvider.GetRequiredService<PublisherOf<IInterfaceToWrap>>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("PublisherOfImpl`1"));
        Assert.That(myInterface.ForEachSubscriber(), Is.Not.Null);
    }
}