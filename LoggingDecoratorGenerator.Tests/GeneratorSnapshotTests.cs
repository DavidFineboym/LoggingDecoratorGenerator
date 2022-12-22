namespace LoggingDecoratorGenerator.Tests;

[UsesVerify]
public class GeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesCorrectly()
    {
        // The source code to test
        var source = @"
using System;
using System.Threading.Tasks;
using Fineboym.Logging.Generator;

namespace SomeFolder.SomeSubFolder
{
    using OtherFolder.OtherSubFolder;

    [DecorateWithLogger]
    public interface ISomeService
    {
        void ParameterlessMethod();

        void VoidReturningMethod(int x, Person person);

        Task TaskReturningAsyncMethod(int x, int y);
    }
}

namespace OtherFolder.OtherSubFolder
{
    public record Person(string Name, int Age);
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}