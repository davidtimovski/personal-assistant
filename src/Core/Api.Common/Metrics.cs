using Sentry;

namespace Api.Common;

public static class Metrics
{
    public static ITransaction StartTransaction(string name, string operation)
    {
        SentrySdk.ConfigureScope(scope => scope.TransactionName = name);
        return SentrySdk.StartTransaction(name, operation);
    }

    public static ITransaction StartTransactionWithUser(string name, string operation, int userId)
    {
        var tr = StartTransaction(name, operation);
        tr.User = new User { Id = userId.ToString() };
        tr.Status = SpanStatus.Ok;

        return tr;
    }
}
