using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts.Accountant;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Application.Contracts.Accountant.Common;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.Debts;
using PersonalAssistant.Application.Contracts.Accountant.Sync.Models;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Services.Accountant
{
    public class SyncService : ISyncService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IUpcomingExpensesRepository _upcomingExpensesRepository;
        private readonly IDebtsRepository _debtsRepository;
        private readonly IMapper _mapper;

        public SyncService(
            IUnitOfWork unitOfWork,
            ICategoriesRepository categoriesRepository,
            IAccountsRepository accountsRepository,
            ITransactionsRepository transactionsRepository,
            IUpcomingExpensesRepository upcomingExpensesRepository,
            IDebtsRepository debtsRepository,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _categoriesRepository = categoriesRepository;
            _accountsRepository = accountsRepository;
            _transactionsRepository = transactionsRepository;
            _upcomingExpensesRepository = upcomingExpensesRepository;
            _debtsRepository = debtsRepository;
            _mapper = mapper;
        }

        public async Task<SyncedEntityIds> SyncEntitiesAsync(SyncEntities model)
        {
            (var conn, var uowTransaction) = await _unitOfWork.StartTransactionAsync();

            var accountIds = new int[model.Accounts.Count];
            var categoryIds = new int[model.Categories.Count];
            var transactionIds = new int[model.Transactions.Count];
            var upcomingExpenseIds = new int[model.UpcomingExpenses.Count];
            var debtIds = new int[model.Debts.Count];

            for (var i = 0; i < accountIds.Length; i++)
            {
                var account = _mapper.Map<Account>(model.Accounts[i]);
                int id = await _accountsRepository.CreateAsync(account, conn, uowTransaction);
                accountIds[i] = id;
            }

            for (var i = 0; i < categoryIds.Length; i++)
            {
                var category = _mapper.Map<Category>(model.Categories[i]);
                int id = await _categoriesRepository.CreateAsync(category, conn, uowTransaction);
                categoryIds[i] = id;
            }

            for (var i = 0; i < transactionIds.Length; i++)
            {
                var transaction = _mapper.Map<Transaction>(model.Transactions[i]);
                int id = await _transactionsRepository.CreateAsync(transaction, conn, uowTransaction);
                transactionIds[i] = id;
            }

            for (var i = 0; i < upcomingExpenseIds.Length; i++)
            {
                var upcomingExpense = _mapper.Map<UpcomingExpense>(model.UpcomingExpenses[i]);
                int id = await _upcomingExpensesRepository.CreateAsync(upcomingExpense, conn, uowTransaction);
                upcomingExpenseIds[i] = id;
            }

            for (var i = 0; i < debtIds.Length; i++)
            {
                var debt = _mapper.Map<Debt>(model.Debts[i]);
                int id = await _debtsRepository.CreateAsync(debt, conn, uowTransaction);
                debtIds[i] = id;
            }

            await _unitOfWork.CommitTransactionAsync(conn, uowTransaction);

            return new SyncedEntityIds(accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds);
        }
    }
}
