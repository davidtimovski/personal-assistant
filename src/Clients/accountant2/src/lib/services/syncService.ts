import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { CurrenciesService } from '../../../../shared2/services/currenciesService';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { syncStatus } from '$lib/stores';
import { AccountsIDBHelper } from '$lib/utils/accountsIDBHelper';
import { CategoriesIDBHelper } from '$lib/utils/categoriesIDBHelper';
import { TransactionsIDBHelper } from '$lib/utils/transactionsIDBHelper';
import { UpcomingExpensesIDBHelper } from '$lib/utils/upcomingExpensesIDBHelper';
import { DebtsIDBHelper } from '$lib/utils/debtsIDBHelper';
import { AutomaticTransactionsIDBHelper } from '$lib/utils/automaticTransactionsIDBHelper';
import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
import { Changed, Create, Created } from '$lib/models/sync';
import { AppEvents } from '$lib/models/appEvents';
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
			syncStatus.set(AppEvents.SyncStarted);
			const lastSynced = this.localStorage.get(LocalStorageKeys.LastSynced);

			this.currenciesService.loadRates();

			let lastSyncedServer = '1970-01-01T00:00:00.000Z';

			await this.upcomingExpensesIDBHelper.deleteOld();

			const changed = await this.httpProxy.ajax<Changed>(`${Variables.urls.gateway}/accountant/api/sync/changes`, {
				method: 'post',
				body: window.JSON.stringify({
					lastSynced: lastSynced
				})
			});
			if (!changed) {
				throw new Error('api/sync/changes call failed');
			}

			lastSyncedServer = DateHelper.adjustTimeZone(new Date(changed.lastSynced)).toISOString();

			await this.accountsIDBHelper.sync(changed.deletedAccountIds, changed.accounts);
			await this.categoriesIDBHelper.sync(changed.deletedCategoryIds, changed.categories);
			await this.transactionsIDBHelper.sync(changed.deletedTransactionIds, changed.transactions);
			await this.upcomingExpensesIDBHelper.sync(changed.deletedUpcomingExpenseIds, changed.upcomingExpenses);
			await this.debtsIDBHelper.sync(changed.deletedDebtIds, changed.debts);
			await this.automaticTransactionsIDBHelper.sync(
				changed.deletedAutomaticTransactionIds,
				changed.automaticTransactions
			);

			const accountsToCreate = await this.accountsIDBHelper.getForSyncing();
			const categoriesToCreate = await this.categoriesIDBHelper.getForSyncing();
			const transactionsToCreate = await this.transactionsIDBHelper.getForSyncing();
			const upcomingExpensesToCreate = await this.upcomingExpensesIDBHelper.getForSyncing();
			const debtsToCreate = await this.debtsIDBHelper.getForSyncing();
			const automaticTransactionsToCreate = await this.automaticTransactionsIDBHelper.getForSyncing();

			const create = new Create(
				accountsToCreate,
				categoriesToCreate,
				transactionsToCreate,
				upcomingExpensesToCreate,
				debtsToCreate,
				automaticTransactionsToCreate
			);
			const created = await this.httpProxy.ajax<Created>(
				`${Variables.urls.gateway}/accountant/api/sync/create-entities`,
				{
					method: 'post',
					body: window.JSON.stringify(create)
				}
			);
			if (!created) {
				throw new Error('api/sync/create-entities call failed');
			}

			await this.accountsIDBHelper.consolidate(created.accountIdPairs);
			await this.categoriesIDBHelper.consolidate(created.categoryIdPairs);
			await this.transactionsIDBHelper.consolidate(created.transactionIdPairs);
			await this.upcomingExpensesIDBHelper.consolidate(created.upcomingExpenseIdPairs);
			await this.debtsIDBHelper.consolidate(created.debtIdPairs);
			await this.automaticTransactionsIDBHelper.consolidate(created.automaticTransactionIdPairs);

			this.localStorage.set(LocalStorageKeys.LastSynced, lastSyncedServer);
			syncStatus.set(AppEvents.SyncFinished);
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
					this.localStorage.set(LocalStorageKeys.LastSynced, '1970-01-01T00:00:00.000Z');

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
