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

    [LoggerMessage(LogLevel.Information, "Daily job run completed")]
    public static partial void DailyJobRunCompleted(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Generated automatic transaction")]
    public static partial void GeneratedAutomaticTransaction(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Updated upcoming expense")]
    public static partial void UpdatedUpcomingExpense(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Deleted upcoming expense")]
    public static partial void DeletedUpcomingExpense(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Generated upcoming expense")]
    public static partial void GeneratedUpcomingExpense(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Uploaded database backup with name: {backupFileName}")]
    public static partial void UploadedDatabaseBackup(this ILogger logger, string backupFileName);
}
