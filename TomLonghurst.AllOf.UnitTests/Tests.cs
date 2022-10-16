using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomLonghurst.AllOf.Extensions;
using TomLonghurst.AllOf.Models;
using TomLonghurst.AllOf.UnitTests.DependentProject;
using TomLonghurst.AllOf.UnitTests.TestModels.Classes;
using TomLonghurst.AllOf.UnitTests.TestModels.Interfaces;

namespace TomLonghurst.AllOf.UnitTests;

/**
 * <summary></summary>
 */
public class Tests
{
    [Test]
    public void VoidTest()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyTestInterface, MyTestClass>()
            .AddSingleton<IMyTestInterface, MyTestClass2>()
            .AddSingleton<IMyTestInterface, MyTestClass3>()
            .AddAllOfs()
            .BuildServiceProvider();
        
        var stringBuilder = new StringBuilder();
    
        var myInterface = serviceProvider.GetRequiredService<AllOf<IMyTestInterface>>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOfImpl`1"));
        
        myInterface.OnEach().Blah(str => stringBuilder.Append(str + " "));
        
        Assert.That(stringBuilder.ToString(), Is.EqualTo("MyTestClass MyTestClass2 MyTestClass3 "));
    }
    
    [Test]
    public async Task AsyncTest()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyAsyncTestInterface, MyAsyncTestClass>()
            .AddSingleton<IMyAsyncTestInterface, MyAsyncTestClass2>()
            .AddSingleton<IMyAsyncTestInterface, MyAsyncTestClass3>()
            .AddAllOfs()
            .BuildServiceProvider();

        var sbLock = new object();
        
        var stringBuilder = new StringBuilder();
    
        var myInterface = serviceProvider.GetRequiredService<AllOf<IMyAsyncTestInterface>>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOfImpl`1"));

        var task = myInterface.OnEach().BlahAsync(async str =>
        {
            await Task.Delay(1000);
            lock (sbLock)
            {
                stringBuilder.Append(str + " ");
            }
        });
    
        // We didn't await so no time for the StringBuilder to be called
        
        Assert.That(stringBuilder.ToString(), Is.EqualTo(""));
    
        await task;
        
        Assert.That(stringBuilder.ToString(), Contains.Substring("MyAsyncTestClass"));
        Assert.That(stringBuilder.ToString(), Contains.Substring("MyAsyncTestClass2"));
        Assert.That(stringBuilder.ToString(), Contains.Substring("MyAsyncTestClass3"));
    }
    
    [Test]
    public async Task ValueTaskAsyncTest()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyValueTaskAsyncTestInterface, MyValueTaskAsyncTestClass>()
            .AddSingleton<IMyValueTaskAsyncTestInterface, MyValueTaskAsyncTestClass2>()
            .AddSingleton<IMyValueTaskAsyncTestInterface, MyValueTaskAsyncTestClass3>()
            .AddAllOfs()
            .BuildServiceProvider();
        
        var stringBuilder = new StringBuilder();
    
        var myInterface = serviceProvider.GetRequiredService<AllOf<IMyValueTaskAsyncTestInterface>>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOfImpl`1"));
        
        var sbLock = new object();

        var task = myInterface.OnEach().BlahAsync(async str =>
        {
            await Task.Delay(100);
            lock (sbLock)
            {
                stringBuilder.Append(str + " ");
            }
        });
    
        // We didn't await so no time for the StringBuilder to be called
        
        Assert.That(stringBuilder.ToString(), Is.EqualTo(""));
    
        await task;
        
        Assert.That(stringBuilder.ToString(), Contains.Substring("MyValueTaskAsyncTestClass"));
        Assert.That(stringBuilder.ToString(), Contains.Substring("MyValueTaskAsyncTestClass2"));
        Assert.That(stringBuilder.ToString(), Contains.Substring("MyValueTaskAsyncTestClass3"));
    }
    
    [Test]
    public void MoqTest()
    {
        var mock1 = new Mock<IMyTestInterface>();
        var mock2 = new Mock<IMyTestInterface>();
        var mock3 = new Mock<IMyTestInterface>();
        
        var services = new ServiceCollection()
            .AddTransient(provider => mock1.Object)
            .AddScoped(provider => mock2.Object)
            .AddSingleton(provider => mock3.Object)
            .AddAllOfs()
            .BuildServiceProvider();
    
        var scope = services.CreateScope().ServiceProvider;
        scope.GetRequiredService<AllOf<IMyTestInterface>>().OnEach().Blah(str => {});
        
        mock1.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Once);
        mock2.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Once);
        mock3.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Once);
        
        scope.GetRequiredService<AllOf<IMyTestInterface>>().OnEach().Blah(str => {});
        
        mock1.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Exactly(2));
        mock2.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Exactly(2));
        mock3.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Exactly(2));
    }
    
    [Test]
    public void DependentProjectClass()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyDependentInterface, MyDependentClass>()
            .AddSingleton<IMyDependentInterface, MyDependentClass>()
            .AddSingleton<IMyDependentInterface, MyDependentClass>()
            .AddAllOfs()
            .BuildServiceProvider();
        
        var myInterface = serviceProvider.GetRequiredService<AllOf<IMyDependentInterface>>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOfImpl`1"));

        var result = 1;
        
        myInterface.OnEach().MyDependentMethod(ref result);
        
        Assert.That(result, Is.EqualTo(123));
    }

    [Test]
    public void ConstructorUsingInterfaceWithoutAttribute()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyInterfaceWithoutAttribute, MyClassWithoutAttribute>()
            .AddSingleton<ConstructorUsingInterfaceWithoutAttribute>()
            .AddAllOfs()
            .BuildServiceProvider();

        var allOf = serviceProvider.GetRequiredService<AllOf<IMyInterfaceWithoutAttribute>>();

        Assert.That(allOf, Is.Not.Null);

        var implementation = allOf.OnEach();
        
        Assert.That(implementation, Is.Not.Null);
        
        var constructorClass = serviceProvider.GetRequiredService<ConstructorUsingInterfaceWithoutAttribute>();
        
        Assert.That(constructorClass, Is.Not.Null);
        Assert.That(constructorClass.MyTestInterface.Items, Is.Not.Null);
        Assert.That(constructorClass.MyTestInterface.OnEach(), Is.Not.Null);
    }
    
    [Test]
    public void ConstructorWithAllOfWrapper()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyInterfaceWithoutAttribute, MyClassWithoutAttribute>()
            .AddSingleton<ConstructorWithAllOfWrapper>()
            .AddSingleton(typeof(PublisherOf<>), typeof(PublisherOfImpl<>))
            .AddAllOfs()
            .BuildServiceProvider();

        var allOf = serviceProvider.GetRequiredService<AllOf<IMyInterfaceWithoutAttribute>>();

        Assert.That(allOf, Is.Not.Null);

        var implementation = allOf.OnEach();
        
        Assert.That(implementation, Is.Not.Null);
        
        var constructorClass = serviceProvider.GetRequiredService<ConstructorWithAllOfWrapper>();
        
        Assert.That(constructorClass, Is.Not.Null);
        Assert.That(constructorClass.Publisher, Is.Not.Null);
        Assert.That(constructorClass.Publisher.ForEachSubscriber(), Is.Not.Null);
    }
    
    
}
