using Microsoft.Extensions.DependencyInjection;
using TomLonghurst.AllOf.Extensions;

namespace TomLonghurst.AllOf.UnitTests;

public class DummyClassTests
{
    [Test]
    public void DummyClass()
    {
        var services = new ServiceCollection()
            .AddTransient<IMyDummyTestInterface, MyTransientTestClass>()
            .AddScoped<IMyDummyTestInterface, MyScopedTestClass>()
            .AddSingleton<IMyDummyTestInterface, MySingletonTestClass>()
            .AddAllOfs()
            .BuildServiceProvider();

        var scope = services.CreateScope().ServiceProvider;
        scope.GetRequiredService<AllOf_IMyDummyTestInterface>().Blah();

        var transient = Get<MyTransientTestClass>(scope);
        var scoped = Get<MyScopedTestClass>(scope);
        var singleton = Get<MySingletonTestClass>(scope);

        Assert.That(transient.BlahCount, Is.EqualTo(0));
        Assert.That(scoped.BlahCount, Is.EqualTo(1));
        Assert.That(singleton.BlahCount, Is.EqualTo(1));

        scope.GetRequiredService<AllOf_IMyDummyTestInterface>().Blah();

        Assert.That(transient.BlahCount, Is.EqualTo(0));
        Assert.That(scoped.BlahCount, Is.EqualTo(2));
        Assert.That(singleton.BlahCount, Is.EqualTo(2));

        var newScope = services.CreateScope().ServiceProvider;

        var newScopedTransient = Get<MyTransientTestClass>(newScope);
        var newScopedScoped = Get<MyScopedTestClass>(newScope);
        var newScopedSingleton = Get<MySingletonTestClass>(newScope);

        Assert.That(newScopedTransient.BlahCount, Is.EqualTo(0));
        Assert.That(newScopedScoped.BlahCount, Is.EqualTo(0));
        Assert.That(newScopedSingleton.BlahCount, Is.EqualTo(2));

        newScope.GetRequiredService<AllOf_IMyDummyTestInterface>().Blah();

        Assert.That(newScopedTransient.BlahCount, Is.EqualTo(0));
        Assert.That(newScopedScoped.BlahCount, Is.EqualTo(1));
        Assert.That(newScopedSingleton.BlahCount, Is.EqualTo(3));

        T Get<T>(IServiceProvider? scope = null) where T : class
        {
            return scope.GetService<AllOf_IMyDummyTestInterface>()
                .Items
                .OfType<T>()
                .First();
        }
    }
}