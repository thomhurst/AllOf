# AllOf

Forward a single method call to the same method on all members of an IEnumerable&lt;T>

## Support

If you like this library, consider buying me a coffee.

<a href="https://www.buymeacoffee.com/tomhurst" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

## Usage

1.  Register multiple implementations of your interfaces(s) in your ServiceCollection

```csharp
services.AddSingleton<IMyInterface, MyImplementation1>()
    .AddScoped<IMyInterface, MyImplementation2>()
    .AddTransient<IMyInterface, MyImplementation3>();
```

2.  Call `AddAllOfs()` on your ServiceCollection

```csharp
        services.AddAllOfs()
```

3.  Inject `AllOf<T>` into your class

```csharp
public class MyWorker
{
  private readonly AllOf<IMyInterface> _myInterfaces;
  
  public MyWorker(AllOf<IMyInterface> myInterfaces)
  {
      _myInterfaces = myInterfaces;
  }
}
```

4.  Call `AllOf.OnEach().SomeMethod()` amd it'll call the same method in all of the different implementations. This handles asynchronous Tasks as well as Synchronous tasks, so no loop or Task handling.

```csharp
_myInterfaces.OnEach().DoSomething();
await _myInterface.OnEach().DoSomethingElseAsync();
```

The above will essentially do:

```csharp
MyImplementation1.DoSomething();
MyImplementation2.DoSomething();
MyImplementation3.DoSomething();

await Task.WhenAll(
  MyImplementation1.DoSomethingElseAsync(),
  MyImplementation2.DoSomethingElseAsync(),
  MyImplementation3.DoSomethingElseAsync()
);
```

`AllOf<>` is an interface so can be easily mocked. As well as `OnEach()`, it holds an `Items` property which is an `IEnumerable<T>` if you need to access your enumerable of implementations.

If you want to change the naming, create a wrapper class around it and register it in the DI.
E.g.

```csharp
public interface PublisherOf<out T>
{
    T ForEachSubscriber();
}

public class PublisherOfImpl<T> : PublisherOf<T>
{
    private readonly AllOf<T> _allOf;

    public PublisherOfImpl(AllOf<T> allOf)
    {
        _allOf = allOf;
    }
    
    public T ForEachSubscriber()
    {
        return _allOf.OnEach();
    }
}
```

in Startup do
```csharp
services.AddTransient(typeof(PublisherOf<>), typeof(PublisherOfImpl<>))
```

And then you can inject this type into your classes, if that reads better for your codebase.

```csharp
public class MyLoginService
{
    public MyLoginService(PublisherOf<ICustomerLoggedInEvent> customerLoggedInEventPublisher)
    {
        _customerLoggedInEventPublisher = customerLoggedInEventPublisher;
    }
}
```
