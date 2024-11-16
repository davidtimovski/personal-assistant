using Microsoft.Extensions.Logging;

namespace Jobs;

public static partial class Log
{
    [LoggerMessage(LogLevel.Information, "Starting {operation}")]
    public static partial void StartingOperation(this ILogger logger, string operation);

    [LoggerMessage(LogLevel.Information, "Completed {operation}")]
    public static partial void CompletedOperation(this ILogger logger, string operation);

    [LoggerMessage(LogLevel.Error, "{operation} failed")]
    public static partial void OperationFailed(this ILogger logger, string operation, Exception ex);
}
