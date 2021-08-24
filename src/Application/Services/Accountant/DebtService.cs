using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.Debts;
using PersonalAssistant.Application.Contracts.Accountant.Debts.Models;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Services.Accountant
{
    public class DebtService : IDebtService
    {
        private readonly IDebtsRepository _debtsRepository;
        private readonly IMapper _mapper;

        public DebtService(
            IDebtsRepository debtsRepository,
            IMapper mapper)
        {
            _debtsRepository = debtsRepository;
            _mapper = mapper;
        }

        public IEnumerable<DebtDto> GetAll(GetAll model)
        {
            var debt = _debtsRepository.GetAll(model.UserId, model.FromModifiedDate);

            var debtDtos = debt.Select(x => _mapper.Map<DebtDto>(x));

            return debtDtos;
        }

        public IEnumerable<int> GetDeletedIds(GetDeletedIds model)
        {
            return _debtsRepository.GetDeletedIds(model.UserId, model.FromDate);
        }

        public Task<int> CreateAsync(CreateDebt model)
        {
            var debt = _mapper.Map<Debt>(model);
            return _debtsRepository.CreateAsync(debt);
        }

        public async Task UpdateAsync(UpdateDebt model)
        {
            var debt = _mapper.Map<Debt>(model);
            await _debtsRepository.UpdateAsync(debt);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            await _debtsRepository.DeleteAsync(id, userId);
        }
    }
}
