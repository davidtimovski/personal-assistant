using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using PersonalAssistant.Application.Contracts.Accountant;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Application.Contracts.Accountant.Transactions.Models;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Services.Accountant
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;

        public TransactionService(
            ITransactionsRepository transactionsRepository,
            IAccountsRepository accountsRepository,
            IMapper mapper)
        {
            _transactionsRepository = transactionsRepository;
            _accountsRepository = accountsRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransactionDto>> GetAllAsync(GetAll model)
        {
            var transactions = await _transactionsRepository.GetAllAsync(model.UserId, model.FromModifiedDate);

            var transactionDtos = transactions.Select(x => _mapper.Map<TransactionDto>(x));

            return transactionDtos;
        }

        public Task<IEnumerable<int>> GetDeletedIdsAsync(GetDeletedIds model)
        {
            return _transactionsRepository.GetDeletedIdsAsync(model.UserId, model.FromDate);
        }

        public async Task<int> CreateAsync(CreateTransaction model)
        {
            if (!model.FromAccountId.HasValue && !model.ToAccountId.HasValue)
            {
                throw new ArgumentException("AccountId is missing.");
            }

            if (model.FromAccountId.HasValue &&
                !(await _accountsRepository.ExistsAsync(model.FromAccountId.Value, model.UserId)))
            {
                throw new ArgumentException("FromAccount doesn't belong to user with specified userId.");
            }

            if (model.ToAccountId.HasValue &&
                !(await _accountsRepository.ExistsAsync(model.ToAccountId.Value, model.UserId)))
            {
                throw new ArgumentException("ToAccount doesn't belong to user with specified userId.");
            }

            var transaction = _mapper.Map<Transaction>(model);

            return await _transactionsRepository.CreateAsync(transaction);
        }

        public async Task UpdateAsync(UpdateTransaction model)
        {
            if (model.FromAccountId.HasValue &&
                !(await _accountsRepository.ExistsAsync(model.FromAccountId.Value, model.UserId)))
            {
                throw new ArgumentException("FromAccount doesn't belong to user with specified userId.");
            }

            if (model.ToAccountId.HasValue &&
                !(await _accountsRepository.ExistsAsync(model.ToAccountId.Value, model.UserId)))
            {
                throw new ArgumentException("ToAccount doesn't belong to user with specified userId.");
            }

            var transaction = _mapper.Map<Transaction>(model);

            await _transactionsRepository.UpdateAsync(transaction);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            await _transactionsRepository.DeleteAsync(id, userId);
        }

        public async Task<FileStream> ExportAsCsvAsync(ExportAsCsv model)
        {
            string fileName = model.FileId + ".csv";
            string tempFilePath = Path.Combine(model.Directory, fileName);

            IEnumerable<Transaction> transactions = await _transactionsRepository.GetAllForExportAsync(model.UserId, model.Uncategorized);
            foreach (var transaction in transactions)
            {
                if (transaction.IsEncrypted)
                {
                    transaction.Description = model.Encrypted;
                }
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
            config.RegisterClassMap<TransactionMap>();

            using (var writer = new StreamWriter(tempFilePath))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(transactions);
            }

            return new FileStream(tempFilePath, FileMode.Open);
        }

        public void DeleteExportedFile(DeleteExportedFile model)
        {
            string filePath = Path.Combine(model.Directory, model.FileId + ".csv");
            File.Delete(filePath);
        }
    }

    public class TransactionMap : ClassMap<Transaction>
    {
        public TransactionMap()
        {
            Map(m => m.Date).Index(0).Name("Date").ConvertUsing(x => x.Date.ToString("dd/MM/yyyy"));
            Map(m => m.Amount).Index(1).Name("Amount");
            Map(m => m.Currency).Index(2).Name("Currency");
            Map(m => m.Category.Name).Index(3).Name("Category");
            Map(m => m.Description).Index(4).Name("Description");
            Map(m => m.FromAccount.Name).Index(5).Name("From Account");
            Map(m => m.ToAccount.Name).Index(6).Name("To Account");
        }
    }
}
