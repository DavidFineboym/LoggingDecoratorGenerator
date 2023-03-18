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
using Fineboym.Logging.Attributes;
using Microsoft.Extensions.Logging;

namespace SomeFolder.SomeSubFolder
{
    using OtherFolder.OtherSubFolder;

    [DecorateWithLogger]
    public interface ISomeService
    {
        [LogMethod(Level = LogLevel.Trace, EventId = 101, EventName = ""foo"")]
        void VoidParameterlessMethod();

        int IntReturningMethod(int x, Person person);

        Task TaskReturningAsyncMethod(int x, int y);

        Task<int> TaskIntReturningAsyncMethod(int x, int y);

        ValueTask ValueTaskReturningAsyncMethod(int x, int y);

        ValueTask<float> ValueTaskFloatReturningAsyncMethod(int x, int y);
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