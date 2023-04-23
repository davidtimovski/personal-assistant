using System.Globalization;
using Application.Domain.Accountant;
using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services;

public class CsvService : ICsvService
{
    private readonly ICsvRepository _csvRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CsvService> _logger;

    public CsvService(
        ICsvRepository csvRepository,
        IMapper mapper,
        ILogger<CsvService> logger)
    {
        _csvRepository = csvRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public FileStream ExportTransactionsAsCsv(ExportTransactionsAsCsv model)
    {
        try
        {
            if (!Directory.Exists(model.Directory))
            {
                Directory.CreateDirectory(model.Directory);
            }

            string fileName = model.FileId + ".csv";
            string tempFilePath = Path.Combine(model.Directory, fileName);

            IEnumerable<Transaction> transactions = _csvRepository.GetAllTransactionsForExport(model.UserId, model.Uncategorized).ToList();
            foreach (var transaction in transactions)
            {
                if (transaction.IsEncrypted)
                {
                    transaction.Description = model.Encrypted;
                }
            }

            using (var writer = new StreamWriter(tempFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TransactionMap>();
                csv.WriteRecords(transactions);
            }

            return new FileStream(tempFilePath, FileMode.Open);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ExportTransactionsAsCsv)}");
            throw;
        }
    }
}

public class TransactionMap : ClassMap<Transaction>
{
    public TransactionMap()
    {
        Map(m => m.Date).Index(0).Name("Date");
        Map(m => m.Amount).Index(1).Name("Amount");
        Map(m => m.Currency).Index(2).Name("Currency");
        Map(m => m.FromStocks).Index(3).Name("From Stocks");
        Map(m => m.ToStocks).Index(4).Name("To Stocks");
        Map(m => m.Category.Name).Index(5).Name("Category");
        Map(m => m.Description).Index(6).Name("Description");
        Map(m => m.FromAccount.Name).Index(7).Name("From Account");
        Map(m => m.ToAccount.Name).Index(8).Name("To Account");
    }
}
