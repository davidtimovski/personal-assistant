using Core.Application.Contracts.Models;
using Sentry;

namespace Core.Application.Contracts;

public interface ICsvService
{
    FileStream ExportTransactionsAsCsv(ExportTransactionsAsCsv model, ISpan metricsSpan);
}
