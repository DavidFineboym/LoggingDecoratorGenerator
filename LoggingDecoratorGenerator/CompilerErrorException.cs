namespace Fineboym.Logging.Generator;

[Serializable]
internal class CompilerErrorException : Exception
{
    public CompilerErrorException() { }
    public CompilerErrorException(string message) : base(message) { }
    public CompilerErrorException(string message, Exception inner) : base(message, inner) { }
    protected CompilerErrorException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
