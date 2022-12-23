# Logging Decorator Generator

Generates logger decorator class for an interface. Uses `Microsoft.Extensions.Logging.ILogger<{Your interface}>` to log and requires it in decorator class constructor.

Use `[DecorateWithLogger]` attribute in `Fineboym.Logging.Generator` namespace on an interface. For example, if you have an interface named `ISomeService` and you apply the attribute to it, it will create source generated class named `SomeServiceLoggingDecorator`. If you use Visual Studio, you can see the generated code in Solution Explorer if you expand Dependencies of the project with this NuGet installed:
![image](https://user-images.githubusercontent.com/45399687/209295923-d9f7e11c-24c3-40d5-89a9-5eee38df5469.png)

