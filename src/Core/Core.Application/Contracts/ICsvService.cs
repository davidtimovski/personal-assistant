using Core.Application.Contracts.Models;

namespace Core.Application.Contracts;

public interface ICsvService
{
    FileStream ExportTransactionsAsCsv(ExportTransactionsAsCsv model, ISpan metricsSpan);
}
