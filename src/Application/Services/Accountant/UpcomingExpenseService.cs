using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Services.Accountant
{
    public class UpcomingExpenseService : IUpcomingExpenseService
    {
        private readonly IUpcomingExpensesRepository _upcomingExpensesRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IMapper _mapper;

        public UpcomingExpenseService(
            IUpcomingExpensesRepository upcomingExpensesRepository,
            ICategoriesRepository categoriesRepository,
            ITransactionsRepository transactionsRepository,
            IMapper mapper)
        {
            _upcomingExpensesRepository = upcomingExpensesRepository;
            _categoriesRepository = categoriesRepository;
            _transactionsRepository = transactionsRepository;
            _mapper = mapper;
        }

        public IEnumerable<UpcomingExpenseDto> GetAll(GetAll model)
        {
            var upcomingExpenses = _upcomingExpensesRepository.GetAll(model.UserId, model.FromModifiedDate);

            var upcomingExpenseDtos = upcomingExpenses.Select(x => _mapper.Map<UpcomingExpenseDto>(x));

            return upcomingExpenseDtos;
        }

        public IEnumerable<int> GetDeletedIds(GetDeletedIds model)
        {
            return _upcomingExpensesRepository.GetDeletedIds(model.UserId, model.FromDate);
        }

        public Task<int> CreateAsync(CreateUpcomingExpense model)
        {
            var upcomingExpense = _mapper.Map<UpcomingExpense>(model);
            return _upcomingExpensesRepository.CreateAsync(upcomingExpense);
        }

        public async Task UpdateAsync(UpdateUpcomingExpense model)
        {
            var upcomingExpense = _mapper.Map<UpcomingExpense>(model);
            await _upcomingExpensesRepository.UpdateAsync(upcomingExpense);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            await _upcomingExpensesRepository.DeleteAsync(id, userId);
        }

        public async Task DeleteOldAsync(int userId)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0);
            await _upcomingExpensesRepository.DeleteOldAsync(userId, startOfMonth);
        }
    }
}
