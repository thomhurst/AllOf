# AllOf
Forward a single method call to the same method on all members of an IEnumerable&lt;T>

## Support

If you like this library, consider buying me a coffee.

<a href="https://www.buymeacoffee.com/tomhurst" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

## Usage

1.  Add the [GenerateAllOf] attribute onto your interface

```csharp
[GenerateAllOf]
public interface IMyInterface
{
    ...
}
```

2.  Register multiple implementations of your interfaces(s) in your ServiceCollection

```csharp
services.AddSingleton<IMyInterface, MyImplementation1>()
    .AddScoped<IMyInterface, MyImplementation2>()
    .AddTransient<IMyInterface, MyImplementation3>();
```

3.  Call `AddAllOfs()` on your ServiceCollection

```csharp
        services.AddAllOfs()
```

4.  Inject `AllOf_T` into your class

```csharp
public class MyWorker
{
  private readonly IMyInterface _myInterface;
  
  public MyWorker(AllOf_IMyInterface allOfMyInterface)
  {
      _myInterface = allOfMyInterface;
  }
}
```

5.  Call a method on your AllOf. It'll now call the same method in all of your registered classes

```csharp
_myInterface.DoSomething();
await _myInterface.DoSomethingElseAsync();
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

5.  Enjoy!

## Caveats

Only interfaces with methods that return void or Task are supported
 - Return types (Properties or methods with return objects) cannot be condensed from an IEnumerable<T> to a T!

