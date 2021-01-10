using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.Accountant.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Accounts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Application.Contracts.Accountant.Categories.Models;
using PersonalAssistant.Application.Contracts.Accountant.Common;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.Debts;
using PersonalAssistant.Application.Contracts.Accountant.Debts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Sync.Models;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Application.Contracts.Accountant.Transactions.Models;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Accountant
{
    [Authorize]
    [EnableCors("AllowAccountant")]
    [Route("api/[controller]")]
    public class SyncController : Controller
    {
        private readonly ISyncService _syncService;
        private readonly ICategoryService _categoryService;
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IUpcomingExpenseService _upcomingExpenseService;
        private readonly IDebtService _debtService;

        public SyncController(
            ISyncService syncService,
            ICategoryService categoryService,
            IAccountService accountService,
            ITransactionService transactionService,
            IUpcomingExpenseService upcomingExpenseService,
            IDebtService debtService)
        {
            _syncService = syncService;
            _categoryService = categoryService;
            _accountService = accountService;
            _transactionService = transactionService;
            _upcomingExpenseService = upcomingExpenseService;
            _debtService = debtService;
        }

        [HttpPost("changes")]
        public async Task<IActionResult> GetChanges([FromBody] GetChangesVm vm)
        {
            if (vm == null)
            {
                return BadRequest();
            }

            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            await _upcomingExpenseService.DeleteOldAsync(userId);

            var getAll = new GetAll(userId, vm.LastSynced);

            IEnumerable<CategoryDto> categories = await _categoryService.GetAllAsync(getAll);
            IEnumerable<AccountDto> accounts = await _accountService.GetAllAsync(getAll);
            IEnumerable<TransactionDto> transactions = await _transactionService.GetAllAsync(getAll);
            IEnumerable<UpcomingExpenseDto> upcomingExpenses = await _upcomingExpenseService.GetAllAsync(getAll);
            IEnumerable<DebtDto> debts = await _debtService.GetAllAsync(getAll);

            var getDeletedIds = new GetDeletedIds(userId, vm.LastSynced);

            var changedVm = new ChangedVm
            {
                LastSynced = DateTime.Now,
                DeletedAccountIds = await _accountService.GetDeletedIdsAsync(getDeletedIds),
                Accounts = accounts,
                DeletedCategoryIds = await _categoryService.GetDeletedIdsAsync(getDeletedIds),
                Categories = categories,
                DeletedTransactionIds = await _transactionService.GetDeletedIdsAsync(getDeletedIds),
                Transactions = transactions,
                DeletedUpcomingExpenseIds = await _upcomingExpenseService.GetDeletedIdsAsync(getDeletedIds),
                UpcomingExpenses = upcomingExpenses,
                DeletedDebtIds = await _debtService.GetDeletedIdsAsync(getDeletedIds),
                Debts = debts
            };

            return Ok(changedVm);
        }

        [HttpPost("create-entities")]
        public async Task<IActionResult> CreateEntities([FromBody] SyncEntities dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            dto.Accounts.ForEach(x => { x.UserId = userId; });
            dto.Categories.ForEach(x => { x.UserId = userId; });
            dto.Transactions.ForEach(x => { x.UserId = userId; });
            dto.UpcomingExpenses.ForEach(x => { x.UserId = userId; });
            dto.Debts.ForEach(x => { x.UserId = userId; });

            var syncedEntityIds = await _syncService.SyncEntitiesAsync(dto);

            var createdEntitiesVm = new CreatedEntityIdsVm();

            for (var i = 0; i < syncedEntityIds.AccountIds.Length; i++)
            {
                createdEntitiesVm.AccountIdPairs.Add(new CreatedEntityIdPair(dto.Accounts[i].Id, syncedEntityIds.AccountIds[i]));
            }
            for (var i = 0; i < syncedEntityIds.CategoryIds.Length; i++)
            {
                createdEntitiesVm.CategoryIdPairs.Add(new CreatedEntityIdPair(dto.Categories[i].Id, syncedEntityIds.CategoryIds[i]));
            }
            for (var i = 0; i < syncedEntityIds.TransactionIds.Length; i++)
            {
                createdEntitiesVm.TransactionIdPairs.Add(new CreatedEntityIdPair(dto.Transactions[i].Id, syncedEntityIds.TransactionIds[i]));
            }
            for (var i = 0; i < syncedEntityIds.UpcomingExpenseIds.Length; i++)
            {
                createdEntitiesVm.UpcomingExpenseIdPairs.Add(new CreatedEntityIdPair(dto.UpcomingExpenses[i].Id, syncedEntityIds.UpcomingExpenseIds[i]));
            }
            for (var i = 0; i < syncedEntityIds.DebtIds.Length; i++)
            {
                createdEntitiesVm.DebtIdPairs.Add(new CreatedEntityIdPair(dto.Debts[i].Id, syncedEntityIds.DebtIds[i]));
            }

            return StatusCode(201, createdEntitiesVm);
        }
    }
}