namespace CostSaver.Extensions;

public static partial class LogStatements
{
    [LoggerMessage(
        EventId = 7001,
        EventName = nameof(CheckingCostSaver),
        Level = LogLevel.Debug,
        Message = "Checking cost saver {CostSaverName}"
    )]
    public static partial void CheckingCostSaver(this ILogger logger, string costSaverName);
    
    [LoggerMessage(
        EventId = 7002,
        EventName = nameof(NamespaceExpired),
        Level = LogLevel.Information,
        Message = "Namespace {NamespaceName} has been expired"
    )]
    public static partial void NamespaceExpired(this ILogger logger, string namespaceName);

    [LoggerMessage(
        EventId = 7003,
        EventName = nameof(NamespaceDeleted),
        Level = LogLevel.Information,
        Message = "Namespace {NamespaceName} has been deleted"
    )]
    public static partial void NamespaceDeleted(this ILogger logger, string namespaceName);
    
    [LoggerMessage(
        EventId = 7999,
        EventName = nameof(GenericError),
        Level = LogLevel.Error,
        Message = "An unhandled error occurred while performing {Operation}"
    )]
    public static partial void GenericError(this ILogger logger, string operation, Exception exception);
}