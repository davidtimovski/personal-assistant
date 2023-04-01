using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.Debts;

public interface IDebtsRepository
{
    Task<int> CreateAsync(Debt debt);
    Task<int> CreateMergedAsync(Debt debt);
    Task UpdateAsync(Debt debt);
    Task DeleteAsync(int id, int userId);
}
