using System.Threading.Tasks;
using AutoMapper;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Categories;
using Application.Contracts.Accountant.Common;
using Application.Contracts.Accountant.Debts;
using Application.Contracts.Accountant.Sync;
using Application.Contracts.Accountant.Sync.Models;
using Application.Contracts.Accountant.Transactions;
using Application.Contracts.Accountant.UpcomingExpenses;
using Domain.Entities.Accountant;

namespace Application.Services.Accountant
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
            var (conn, uowTransaction) = _unitOfWork.StartTransaction();

            var accountIds = new int[model.Accounts.Count];
            var categoryIds = new int[model.Categories.Count];
            var transactionIds = new int[model.Transactions.Count];
            var upcomingExpenseIds = new int[model.UpcomingExpenses.Count];
            var debtIds = new int[model.Debts.Count];

            for (var i = 0; i < accountIds.Length; i++)
            {
                var account = _mapper.Map<Account>(model.Accounts[i]);
                accountIds[i] = await _accountsRepository.CreateAsync(account, conn, uowTransaction);
            }

            for (var i = 0; i < categoryIds.Length; i++)
            {
                var category = _mapper.Map<Category>(model.Categories[i]);
                categoryIds[i] = await _categoriesRepository.CreateAsync(category, conn, uowTransaction);
            }

            for (var i = 0; i < transactionIds.Length; i++)
            {
                var transaction = _mapper.Map<Transaction>(model.Transactions[i]);
                transactionIds[i] = await _transactionsRepository.CreateAsync(transaction, conn, uowTransaction);
            }

            for (var i = 0; i < upcomingExpenseIds.Length; i++)
            {
                var upcomingExpense = _mapper.Map<UpcomingExpense>(model.UpcomingExpenses[i]);
                upcomingExpenseIds[i] = await _upcomingExpensesRepository.CreateAsync(upcomingExpense, conn, uowTransaction);
            }

            for (var i = 0; i < debtIds.Length; i++)
            {
                var debt = _mapper.Map<Debt>(model.Debts[i]);
                debtIds[i] = await _debtsRepository.CreateAsync(debt, conn, uowTransaction);
            }

            _unitOfWork.CommitTransaction(conn, uowTransaction);

            return new SyncedEntityIds(accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds);
        }
    }
}
