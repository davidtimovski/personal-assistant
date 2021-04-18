using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts.Accountant;
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

        public async Task<IEnumerable<UpcomingExpenseDto>> GetAllAsync(GetAll model)
        {
            var upcomingExpenses = await _upcomingExpensesRepository.GetAllAsync(model.UserId, model.FromModifiedDate);

            var upcomingExpenseDtos = upcomingExpenses.Select(x => _mapper.Map<UpcomingExpenseDto>(x));

            return upcomingExpenseDtos;
        }

        public Task<IEnumerable<int>> GetDeletedIdsAsync(GetDeletedIds model)
        {
            return _upcomingExpensesRepository.GetDeletedIdsAsync(model.UserId, model.FromDate);
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

        public async Task GenerateAsync()
        {
            var now = DateTime.UtcNow;

            string getMostFrequentCurrency(IEnumerable<Transaction> expenses)
            {
                if (expenses.Count() == 1)
                {
                    return expenses.First().Currency;
                }

                var currency = expenses
                  .GroupBy(x => x.Currency)
                  .Where(g => g.Count() > 1)
                  .OrderByDescending(g => g.Count())
                  .Select(g => g.Key).First();

                return currency;
            }

            bool shouldGenerate(IEnumerable<Transaction> expenses)
            {
                if (!expenses.Any())
                {
                    return false;
                }

                Transaction earliest = expenses.OrderBy(x => x.Date).First();
                var twoMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-2);

                return earliest.Date < twoMonthsAgo;
            }

            var categories = await _categoriesRepository.GetAllWithGenerateAsync();

            var userGroups = categories.GroupBy(x => x.UserId);

            foreach (var userGroup in userGroups)
            {
                foreach (Category category in userGroup)
                {
                    var exists = await _upcomingExpensesRepository.ExistsAsync(category.Id, now);
                    if (exists)
                    {
                        continue;
                    }

                    var firstOfThisMonth = new DateTime(now.Year, now.Month, 1);
                    var transactionsExistThisMonth = await _transactionsRepository.AnyAsync(category.UserId, category.Id, firstOfThisMonth);
                    if (transactionsExistThisMonth)
                    {
                        continue;
                    }

                    var threeMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-3);
                    var expenses = await _transactionsRepository.GetAllAsync(category.UserId, category.Id, threeMonthsAgo, firstOfThisMonth);

                    if (shouldGenerate(expenses))
                    {
                        decimal sum = expenses.Sum(x => x.Amount);
                        int months = expenses.GroupBy(x => x.Date.ToString("yyyy-MM")).Count();
                        decimal amount = sum / months;
                        var currency = getMostFrequentCurrency(expenses);
                        if (currency == "MKD")
                        {
                            amount = Math.Round(amount);
                            amount -= amount % 10;
                        }
                        var date = new DateTime(now.Year, now.Month, 1);

                        var upcomingExpense = new UpcomingExpense
                        {
                            UserId = category.UserId,
                            CategoryId = category.Id,
                            Amount = amount,
                            Currency = currency,
                            Date = date,
                            Generated = true,
                            CreatedDate = now,
                            ModifiedDate = now
                        };

                        await _upcomingExpensesRepository.CreateAsync(upcomingExpense);
                    }
                }
            }
        }
    }
}
