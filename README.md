# Logging Decorator Generator

Generates logger decorator class for an interface. Uses `Microsoft.Extensions.Logging.ILogger<{Your interface}>` to log and requires it in decorator class constructor.

Use `[DecorateWithLogger]` attribute in `Fineboym.Logging.Generator` namespace on an interface. For example, if you have an interface named `ISomeService` and you apply the attribute to it, it will create source generated class named `SomeServiceLoggingDecorator`. If you use Visual Studio, you can see the generated code in Solution Explorer if you expand Dependencies of the project with this NuGet installed:
![image](https://user-images.githubusercontent.com/45399687/209295923-d9f7e11c-24c3-40d5-89a9-5eee38df5469.png)

Currently it supports a simple interface, only with methods and up to 6 parameters which is what `LoggerMessage.Define` supports (https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggermessage.define?view=dotnet-plat-ext-7.0).
Please feel free to open issues for questions, bugs, and improvements and I'll try to address them as soon as I can. Thank you.

If you use .NET dependency injection then you can decorate your service interface. See here for example https://andrewlock.net/adding-decorated-classes-to-the-asp.net-core-di-container-using-scrutor/

Thanks to Andrew Lock's blog series on incremental source generators. https://andrewlock.net/series/creating-a-source-generator/
