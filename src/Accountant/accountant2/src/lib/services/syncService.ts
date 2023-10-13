import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { CurrenciesService } from '../../../../../Core/shared2/services/currenciesService';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';

import { syncStatus } from '$lib/stores';
import { AccountsIDBHelper } from '$lib/utils/accountsIDBHelper';
import { CategoriesIDBHelper } from '$lib/utils/categoriesIDBHelper';
import { TransactionsIDBHelper } from '$lib/utils/transactionsIDBHelper';
import { UpcomingExpensesIDBHelper } from '$lib/utils/upcomingExpensesIDBHelper';
import { DebtsIDBHelper } from '$lib/utils/debtsIDBHelper';
import { AutomaticTransactionsIDBHelper } from '$lib/utils/automaticTransactionsIDBHelper';
import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
import { SyncStatus, SyncEvents } from '$lib/models/syncStatus';
import { GetChanges, SyncEntities } from '$lib/models/server/requests/sync';
import type { Changed, CreatedEntityIds } from '$lib/models/server/responses/sync';
import Variables from '$lib/variables';

export class SyncService {
	private readonly httpProxy = new HttpProxy();
	private readonly accountsIDBHelper = new AccountsIDBHelper();
	private readonly categoriesIDBHelper = new CategoriesIDBHelper();
	private readonly transactionsIDBHelper = new TransactionsIDBHelper();
	private readonly upcomingExpensesIDBHelper = new UpcomingExpensesIDBHelper();
	private readonly debtsIDBHelper = new DebtsIDBHelper();
	private readonly automaticTransactionsIDBHelper = new AutomaticTransactionsIDBHelper();
	private readonly localStorage = new LocalStorageUtil();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

	async sync() {
		if (!navigator.onLine) {
			return;
		}

		try {
			syncStatus.set(new SyncStatus(SyncEvents.SyncStarted, 0, 0));
			const lastSynced = this.localStorage.getLastSynced();

			this.currenciesService.loadRates();

			await this.upcomingExpensesIDBHelper.deleteOld();

			const changed = await this.httpProxy.ajax<Changed>(`${Variables.urls.api}/sync/changes`, {
				method: 'post',
				body: window.JSON.stringify(new GetChanges(lastSynced))
			});
			if (!changed) {
				throw new Error('api/sync/changes call failed');
			}

			await this.accountsIDBHelper.sync(changed.deletedAccountIds, changed.accounts);
			await this.categoriesIDBHelper.sync(changed.deletedCategoryIds, changed.categories);
			await this.transactionsIDBHelper.sync(changed.deletedTransactionIds, changed.transactions);
			await this.upcomingExpensesIDBHelper.sync(changed.deletedUpcomingExpenseIds, changed.upcomingExpenses);
			await this.debtsIDBHelper.sync(changed.deletedDebtIds, changed.debts);
			await this.automaticTransactionsIDBHelper.sync(changed.deletedAutomaticTransactionIds, changed.automaticTransactions);

			const accountsToCreate = await this.accountsIDBHelper.getForSyncing();
			const categoriesToCreate = await this.categoriesIDBHelper.getForSyncing();
			const transactionsToCreate = await this.transactionsIDBHelper.getForSyncing();
			const upcomingExpensesToCreate = await this.upcomingExpensesIDBHelper.getForSyncing();
			const debtsToCreate = await this.debtsIDBHelper.getForSyncing();
			const automaticTransactionsToCreate = await this.automaticTransactionsIDBHelper.getForSyncing();

			const syncRequest = new SyncEntities(
				accountsToCreate,
				categoriesToCreate,
				transactionsToCreate,
				upcomingExpensesToCreate,
				debtsToCreate,
				automaticTransactionsToCreate
			);
			const created = await this.httpProxy.ajax<CreatedEntityIds>(`${Variables.urls.api}/sync/create-entities`, {
				method: 'post',
				body: window.JSON.stringify(syncRequest)
			});
			if (!created) {
				throw new Error('api/sync/create-entities call failed');
			}

			await this.accountsIDBHelper.consolidate(created.accountIdPairs);
			await this.categoriesIDBHelper.consolidate(created.categoryIdPairs);
			await this.transactionsIDBHelper.consolidate(created.transactionIdPairs);
			await this.upcomingExpensesIDBHelper.consolidate(created.upcomingExpenseIdPairs);
			await this.debtsIDBHelper.consolidate(created.debtIdPairs);
			await this.automaticTransactionsIDBHelper.consolidate(created.automaticTransactionIdPairs);

			this.localStorage.setLastSynced(changed.lastSynced);

			const retrieved =
				changed.deletedAccountIds.length +
				changed.accounts.length +
				changed.deletedCategoryIds.length +
				changed.categories.length +
				changed.deletedTransactionIds.length +
				changed.transactions.length +
				changed.deletedUpcomingExpenseIds.length +
				changed.upcomingExpenses.length +
				changed.deletedDebtIds.length +
				changed.debts.length +
				changed.deletedAutomaticTransactionIds.length +
				changed.automaticTransactions.length;

			const pushed =
				created.accountIdPairs.length +
				created.categoryIdPairs.length +
				created.transactionIdPairs.length +
				created.upcomingExpenseIdPairs.length +
				created.debtIdPairs.length +
				created.automaticTransactionIdPairs.length;

			syncStatus.set(new SyncStatus(SyncEvents.SyncFinished, retrieved, pushed));
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async totalSync(): Promise<void> {
		try {
			const deleteContextRequest = window.indexedDB.deleteDatabase('IDBContext');

			deleteContextRequest.onsuccess = () => {
				const deleteDbNamesRequest = window.indexedDB.deleteDatabase('__dbnames');

				deleteDbNamesRequest.onsuccess = () => {
					this.localStorage.setLastSynced('1970-01-01T00:00:00.000Z');

					window.location.href = '/';
				};
			};
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	release() {
		this.httpProxy.release();
		this.currenciesService.release();
		this.logger.release();
	}
}
