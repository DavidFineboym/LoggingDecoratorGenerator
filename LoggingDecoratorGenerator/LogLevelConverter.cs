namespace Fineboym.Logging.Generator;

internal static class LogLevelConverter
{
    public static string FromInt(int value) => value switch
    {
        0 => "Trace",
        1 => "Debug",
        2 => "Information",
        3 => "Warning",
        4 => "Error",
        5 => "Critical",
        6 => "None",
        _ => value.ToString()
    };
}
