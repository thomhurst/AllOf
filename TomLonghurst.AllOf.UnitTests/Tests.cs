using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomLonghurst.AllOf.Extensions;
using TomLonghurst.AllOf.Models;
using TomLonghurst.AllOf.UnitTests.DependentProject;

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
    
        var myInterface = serviceProvider.GetRequiredService<AllOf_IMyTestInterface>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOf_IMyTestInterface_Impl"));
        
        myInterface.Blah(str => stringBuilder.Append(str + " "));
        
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
        
        var stringBuilder = new StringBuilder();
    
        var myInterface = serviceProvider.GetRequiredService<AllOf_IMyAsyncTestInterface>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOf_IMyAsyncTestInterface_Impl"));

        var task = myInterface.BlahAsync(async str =>
        {
            await Task.Delay(1000);
            stringBuilder.Append(str + " ");
        });
    
        // We didn't await so no time for the StringBuilder to be called
        
        Assert.That(stringBuilder.ToString(), Is.EqualTo(""));
    
        await task;
        
        Assert.That(stringBuilder.ToString(), Is.EqualTo("MyAsyncTestClass MyAsyncTestClass2 MyAsyncTestClass3 "));
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
    
        var myInterface = serviceProvider.GetRequiredService<AllOf_IMyValueTaskAsyncTestInterface>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOf_IMyValueTaskAsyncTestInterface_Impl"));
    
        var task = myInterface.BlahAsync(async str =>
        {
            await Task.Delay(100);
            stringBuilder.Append(str + " ");
        });
    
        // We didn't await so no time for the StringBuilder to be called
        
        Assert.That(stringBuilder.ToString(), Is.EqualTo(""));
    
        await task;
        
        Assert.That(stringBuilder.ToString(), Is.EqualTo("MyValueTaskAsyncTestClass MyValueTaskAsyncTestClass2 MyValueTaskAsyncTestClass3 "));
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
        scope.GetRequiredService<AllOf_IMyTestInterface>().Blah(str => {});
        
        mock1.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Once);
        mock2.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Once);
        mock3.Verify(x => x.Blah(It.IsAny<Action<string>>()), Times.Once);
        
        scope.GetRequiredService<AllOf_IMyTestInterface>().Blah(str => {});
        
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
        
        var myInterface = serviceProvider.GetRequiredService<AllOf_IMyDependentInterface>();
        
        Assert.That(myInterface.GetType().Name, Is.EqualTo("AllOf_IMyDependentInterface_Impl"));

        var result = 1;
        
        myInterface.MyDependentMethod(ref result);
        
        Assert.That(result, Is.EqualTo(123));
    }

    [Test]
    public void ConstructorUsingInterfaceWithoutAttribute()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyInterfaceWithoutAttribute, MyClassWithoutAttribute>()
            .AddAllOfs()
            .BuildServiceProvider();

        var allOf = serviceProvider.GetRequiredService<AllOf<IMyInterfaceWithoutAttribute>>();

        AllOf_IMyInterfaceWithoutAttribute blah = default;
        
        Assert.That(allOf, Is.Not.Null);

        var implementation = allOf.OnEach();
        
        Assert.That(implementation, Is.Not.Null);
    }
}
