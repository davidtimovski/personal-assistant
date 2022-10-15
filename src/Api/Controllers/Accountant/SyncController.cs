using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.Accountant.Sync;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Accounts.Models;
using Application.Contracts.Accountant.AutomaticTransactions;
using Application.Contracts.Accountant.AutomaticTransactions.Models;
using Application.Contracts.Accountant.Categories;
using Application.Contracts.Accountant.Categories.Models;
using Application.Contracts.Accountant.Common.Models;
using Application.Contracts.Accountant.Debts;
using Application.Contracts.Accountant.Debts.Models;
using Application.Contracts.Accountant.Sync;
using Application.Contracts.Accountant.Sync.Models;
using Application.Contracts.Accountant.Transactions;
using Application.Contracts.Accountant.Transactions.Models;
using Application.Contracts.Accountant.UpcomingExpenses;
using Application.Contracts.Accountant.UpcomingExpenses.Models;
using Application.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Accountant;

[Authorize]
[EnableCors("AllowAccountant")]
[Route("api/[controller]")]
public class SyncController : BaseController
{
    private readonly ISyncService _syncService;
    private readonly ICategoryService _categoryService;
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly IUpcomingExpenseService _upcomingExpenseService;
    private readonly IDebtService _debtService;
    private readonly IAutomaticTransactionService _automaticTransactionService;

    public SyncController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        ISyncService syncService,
        ICategoryService categoryService,
        IAccountService accountService,
        ITransactionService transactionService,
        IUpcomingExpenseService upcomingExpenseService,
        IDebtService debtService,
        IAutomaticTransactionService automaticTransactionService) : base(userIdLookup, usersRepository)
    {
        _syncService = syncService;
        _categoryService = categoryService;
        _accountService = accountService;
        _transactionService = transactionService;
        _upcomingExpenseService = upcomingExpenseService;
        _debtService = debtService;
        _automaticTransactionService = automaticTransactionService;
    }

    [HttpPost("changes")]
    public async Task<IActionResult> GetChanges([FromBody] GetChangesVm vm)
    {
        if (vm == null)
        {
            return BadRequest();
        }

        await _upcomingExpenseService.DeleteOldAsync(CurrentUserId);

        var getAll = new GetAll(CurrentUserId, vm.LastSynced);

        IEnumerable<CategoryDto> categories = _categoryService.GetAll(getAll);
        IEnumerable<AccountDto> accounts = _accountService.GetAll(getAll);
        IEnumerable<TransactionDto> transactions = _transactionService.GetAll(getAll);
        IEnumerable<UpcomingExpenseDto> upcomingExpenses = _upcomingExpenseService.GetAll(getAll);
        IEnumerable<DebtDto> debts = _debtService.GetAll(getAll);
        IEnumerable<AutomaticTransactionDto> automaticTransactions = _automaticTransactionService.GetAll(getAll);

        var getDeletedIds = new GetDeletedIds(CurrentUserId, vm.LastSynced);

        var changedVm = new ChangedVm
        {
            LastSynced = DateTime.UtcNow,
            DeletedAccountIds = _accountService.GetDeletedIds(getDeletedIds),
            Accounts = accounts,
            DeletedCategoryIds = _categoryService.GetDeletedIds(getDeletedIds),
            Categories = categories,
            DeletedTransactionIds = _transactionService.GetDeletedIds(getDeletedIds),
            Transactions = transactions,
            DeletedUpcomingExpenseIds = _upcomingExpenseService.GetDeletedIds(getDeletedIds),
            UpcomingExpenses = upcomingExpenses,
            DeletedDebtIds = _debtService.GetDeletedIds(getDeletedIds),
            Debts = debts,
            DeletedAutomaticTransactionIds = _debtService.GetDeletedIds(getDeletedIds),
            AutomaticTransactions = automaticTransactions
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

        dto.Accounts.ForEach(x => { x.UserId = CurrentUserId; });
        dto.Categories.ForEach(x => { x.UserId = CurrentUserId; });
        dto.UpcomingExpenses.ForEach(x => { x.UserId = CurrentUserId; });
        dto.Debts.ForEach(x => { x.UserId = CurrentUserId; });
        dto.AutomaticTransactions.ForEach(x => { x.UserId = CurrentUserId; });

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
        for (var i = 0; i < syncedEntityIds.AutomaticTransactionIds.Length; i++)
        {
            createdEntitiesVm.AutomaticTransactionIdPairs.Add(new CreatedEntityIdPair(dto.AutomaticTransactions[i].Id, syncedEntityIds.AutomaticTransactionIds[i]));
        }

        return StatusCode(201, createdEntitiesVm);
    }
}
