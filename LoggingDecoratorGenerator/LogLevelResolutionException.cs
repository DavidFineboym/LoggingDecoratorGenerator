namespace Fineboym.Logging.Generator;

[Serializable]
public class LogLevelResolutionException : Exception
{
    public LogLevelResolutionException() { }
    public LogLevelResolutionException(string message) : base(message) { }
    public LogLevelResolutionException(string message, Exception inner) : base(message, inner) { }
    protected LogLevelResolutionException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
