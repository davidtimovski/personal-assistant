using System.Globalization;
using Accountant.Application.Contracts.Transactions;
using Accountant.Application.Contracts.Transactions.Models;
using Application.Domain.Accountant;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Accountant.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionsRepository transactionsRepository,
        IMapper mapper,
        ILogger<TransactionService> logger)
    {
        _transactionsRepository = transactionsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<int> CreateAsync(CreateTransaction model)
    {
        try
        {
            var transaction = _mapper.Map<Transaction>(model);

            if (transaction.Description != null)
            {
                transaction.Description = transaction.Description.Trim();
            }

            return await _transactionsRepository.CreateAsync(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateTransaction model)
    {
        try
        {
            var transaction = _mapper.Map<Transaction>(model);

            if (transaction.Description != null)
            {
                transaction.Description = transaction.Description.Trim();
            }

            await _transactionsRepository.UpdateAsync(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
    }

    public FileStream ExportAsCsv(ExportAsCsv model)
    {
        try
        {
            if (!Directory.Exists(model.Directory))
            {
                Directory.CreateDirectory(model.Directory);
            }

            string fileName = model.FileId + ".csv";
            string tempFilePath = Path.Combine(model.Directory, fileName);

            IEnumerable<Transaction> transactions = _transactionsRepository.GetAllForExport(model.UserId, model.Uncategorized).ToList();
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
            _logger.LogError(ex, $"Unexpected error in {nameof(ExportAsCsv)}");
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
