using System;
using System.Threading.Tasks;

namespace PersonalAssistant.Application.Contracts.Accountant.Common
{
    public interface IDeletedEntitiesRepository
    {
        Task DeleteOldAsync(DateTime from);
    }
}
