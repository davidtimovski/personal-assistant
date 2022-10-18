import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { CurrenciesService } from '../../../../shared2/services/currenciesService';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { AccountsIDBHelper } from '$lib/utils/accountsIDBHelper';
import { TransactionsIDBHelper } from '$lib/utils/transactionsIDBHelper';
import type { Account } from '$lib/models/entities/account';
import { SelectOption } from '$lib/models/viewmodels/selectOption';
import type { TransactionModel } from '$lib/models/entities/transaction';
import Variables from '$lib/variables';

export class AccountsService {
	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new AccountsIDBHelper();
	private readonly transactionsIDBHelper = new TransactionsIDBHelper();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

	getMainId(): Promise<number> {
		return this.idbHelper.getMainId();
	}

	async getAllWithBalance(currency: string): Promise<Array<Account>> {
		try {
			const accounts = await this.idbHelper.getAll();

			const getBalancePromises = new Array<Promise<void>>();
			for (const account of accounts) {
				if (account.stockPrice === null) {
					const getBalancePromise = this.getBalance(account.id, currency).then((balance: number) => {
						account.balance = balance;
					});
					getBalancePromises.push(getBalancePromise);
				} else {
					const getBalancePromise = this.getBalanceAndStocks(account, currency).then(([balance, stocks]) => {
						account.balance = balance;
						account.stocks = stocks;
					});
					getBalancePromises.push(getBalancePromise);
				}
			}

			await Promise.all(getBalancePromises);

			return accounts;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getAllAsOptions(): Promise<Array<SelectOption>> {
		try {
			const accounts = await this.idbHelper.getAllAsOptions();

			const options = new Array<SelectOption>();
			for (const account of accounts) {
				options.push(new SelectOption(account.id, account.name));
			}

			return options;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	/** The first item in the array will be the main account. */
	async getNonInvestmentFundsAsOptions(): Promise<Array<SelectOption>> {
		try {
			const accounts = await this.idbHelper.getAllAsOptions(true);

			const options = new Array<SelectOption>();

			const main = <Account>accounts.find((x) => x.isMain);
			options.push(new SelectOption(main.id, main.name));

			for (const account of accounts.filter((x) => !x.isMain)) {
				options.push(new SelectOption(account.id, account.name));
			}

			return options;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	get(id: number): Promise<Account> {
		return this.idbHelper.get(id);
	}

	async getBalance(id: number, currency: string): Promise<number> {
		try {
			const transactions = await this.transactionsIDBHelper.getAllForAccount(id);

			let balance = 0;
			transactions.forEach((x: TransactionModel) => {
				if (id === x.fromAccountId) {
					balance -= this.currenciesService.convert(x.amount, x.currency, currency);
				} else if (id === x.toAccountId) {
					balance += this.currenciesService.convert(x.amount, x.currency, currency);
				}
			});

			return parseFloat(balance.toFixed(2));
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getAverageMonthlySavingsFromThePastYear(currency: string) {
		try {
			const mainAccountId = await this.getMainId();
			const transferTransactions = await this.transactionsIDBHelper.getAllSavingTransactionsInThePastYear(
				mainAccountId
			);

			const movedFromMain = transferTransactions.filter((x) => x.fromAccountId === mainAccountId);
			const movedToMain = transferTransactions.filter((x) => x.toAccountId === mainAccountId);

			let saving = 0;

			saving += movedFromMain
				.map((x) => this.currenciesService.convert(x.amount, x.currency, currency))
				.reduce((a, b) => a + b, 0);
			saving -= movedToMain
				.map((x) => this.currenciesService.convert(x.amount, x.currency, currency))
				.reduce((a, b) => a + b, 0);

			const earliestTransaction = transferTransactions.sort((a: TransactionModel, b: TransactionModel) => {
				const aDate = new Date(a.date);
				const bDate = new Date(b.date);
				if (aDate < bDate) return -1;
				if (aDate > bDate) return 1;

				return 0;
			})[0];

			const now = new Date();
			const earliestTransactionDate = new Date(earliestTransaction.date);
			const monthsPassed =
				now.getMonth() -
				earliestTransactionDate.getMonth() +
				12 * (now.getFullYear() - earliestTransactionDate.getFullYear());
			const savingsPerMonth = saving / monthsPassed;

			return parseFloat(savingsPerMonth.toFixed(2));
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getBalanceAndStocks(account: Account, currency: string): Promise<[number, number]> {
		try {
			const transactions = await this.transactionsIDBHelper.getAllForAccount(account.id);

			let stocks = 0;
			transactions.forEach((x: TransactionModel) => {
				if (account.id === x.fromAccountId && x.fromStocks) {
					stocks -= x.fromStocks;
				} else if (account.id === x.toAccountId && x.toStocks) {
					stocks += x.toStocks;
				}
			});

			if (!account.stockPrice) {
				throw new Error('Account does not have stock price set');
			}

			const amount = stocks * account.stockPrice;
			const balance = this.currenciesService.convert(amount, account.currency, currency);

			return [parseFloat(balance.toFixed(2)), parseInt(stocks.toString())];
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async create(account: Account): Promise<number> {
		try {
			const now = DateHelper.adjustForTimeZone(new Date());
			account.createdDate = account.modifiedDate = now;

			if (navigator.onLine) {
				account.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/api/accounts`, {
					method: 'post',
					body: window.JSON.stringify(account)
				});
				account.synced = true;
			}

			await this.idbHelper.create(account);

			return account.id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(account: Account): Promise<void> {
		try {
			account.modifiedDate = DateHelper.adjustForTimeZone(new Date());

			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/accounts`, {
					method: 'put',
					body: window.JSON.stringify(account)
				});
				account.synced = true;
			} else if (await this.idbHelper.isSynced(account.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(account);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/accounts/${id}`, {
					method: 'delete'
				});
			} else if (await this.idbHelper.isSynced(id)) {
				throw 'failedToFetchError';
			}
			await this.idbHelper.delete(id);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async hasTransactions(id: number): Promise<boolean> {
		return this.idbHelper.hasTransactions(id);
	}

	release() {
		this.httpProxy.release();
		this.currenciesService.release();
		this.logger.release();
	}
}
