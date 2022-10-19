using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Common.Models;
using Application.Contracts.Accountant.Transactions;
using Application.Contracts.Accountant.Transactions.Models;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities.Accountant;
using Microsoft.Extensions.Logging;

namespace Application.Services.Accountant;

public class TransactionService : ITransactionService
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository,
        IMapper mapper,
        ILogger<TransactionService> logger)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<TransactionDto> GetAll(GetAll model)
    {
        try
        {
            var transactions = _transactionsRepository.GetAll(model.UserId, model.FromModifiedDate);

            var transactionDtos = transactions.Select(x => _mapper.Map<TransactionDto>(x));

            return transactionDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            throw;
        }
    }

    public IEnumerable<int> GetDeletedIds(GetDeletedIds model)
    {
        try
        {
            return _transactionsRepository.GetDeletedIds(model.UserId, model.FromDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetDeletedIds)}");
            throw;
        }
    }

    public async Task<int> CreateAsync(CreateTransaction model)
    {
        if (!model.FromAccountId.HasValue && !model.ToAccountId.HasValue)
        {
            throw new ArgumentException("AccountId is missing.");
        }
        else if (model.FromAccountId == model.ToAccountId)
        {
            throw new ArgumentException("FromAccountId and ToAccountId cannot be the same.");
        }

        if (model.FromAccountId.HasValue &&
            !_accountsRepository.Exists(model.FromAccountId.Value, model.UserId))
        {
            throw new ArgumentException("FromAccountId doesn't belong to user with specified userId.");
        }

        if (model.ToAccountId.HasValue &&
            !_accountsRepository.Exists(model.ToAccountId.Value, model.UserId))
        {
            throw new ArgumentException("ToAccountId doesn't belong to user with specified userId.");
        }

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
        if (!model.FromAccountId.HasValue && !model.ToAccountId.HasValue)
        {
            throw new ArgumentException("AccountId is missing.");
        }
        else if (model.FromAccountId == model.ToAccountId)
        {
            throw new ArgumentException("FromAccountId and ToAccountId cannot be the same.");
        }

        if (model.FromAccountId.HasValue &&
            !_accountsRepository.Exists(model.FromAccountId.Value, model.UserId))
        {
            throw new ArgumentException("FromAccount doesn't belong to user with specified userId.");
        }

        if (model.ToAccountId.HasValue &&
            !_accountsRepository.Exists(model.ToAccountId.Value, model.UserId))
        {
            throw new ArgumentException("ToAccount doesn't belong to user with specified userId.");
        }

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

    public async Task DeleteAsync(int id, int userId)
    {
        try
        {
            await _transactionsRepository.DeleteAsync(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }

    public FileStream ExportAsCsv(ExportAsCsv model)
    {
        try
        {
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

    public void DeleteExportedFile(DeleteExportedFile model)
    {
        try
        {
            string filePath = Path.Combine(model.Directory, model.FileId + ".csv");
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
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
