import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { CurrenciesService } from '../../../../../Core/shared2/services/currenciesService';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';
import { DateHelper } from '../../../../../Core/shared2/utils/dateHelper';
import { ValidationResult, ValidationUtil } from '../../../../../Core/shared2/utils/validationUtils';

import { TransactionsIDBHelper } from '$lib/utils/transactionsIDBHelper';
import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
import { TransactionModel } from '$lib/models/entities/transaction';
import { TransactionType } from '$lib/models/viewmodels/transactionType';
import { CategoriesService } from '$lib/services/categoriesService';
import { EncryptionService } from '$lib/services/encryptionService';
import type { SearchFilters } from '$lib/models/viewmodels/searchFilters';
import { AmountByCategory } from '$lib/models/viewmodels/amountByCategory';
import { CreateTransaction, UpdateTransaction } from '$lib/models/server/requests/transaction';
import Variables from '$lib/variables';

export class TransactionsService {
	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new TransactionsIDBHelper();
	private readonly localStorage = new LocalStorageUtil();
	private readonly categoriesService = new CategoriesService();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly encryptionService = new EncryptionService();
	private readonly logger = new ErrorLogger('Accountant');

	count(filters: SearchFilters): Promise<number> {
		return this.idbHelper.count(filters);
	}

	async getAllByPage(filters: SearchFilters, currency: string): Promise<TransactionModel[]> {
		try {
			const transactions = await this.idbHelper.getAllByPage(filters);

			transactions.forEach((x: TransactionModel) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});

			return transactions;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getByCategory(transactions: TransactionModel[], currency: string, uncategorizedLabel: string): Promise<AmountByCategory[]> {
		try {
			transactions.forEach((x: TransactionModel) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});

			const categories = await this.categoriesService.getAll();
			const expendituresByCategory = new Array<AmountByCategory>();

			for (const category of categories) {
				const categoryTransactions = transactions.filter((t) => t.categoryId === category.id);
				const expenditure = new AmountByCategory(category.id, category.parentId, null, 0);
				expenditure.categoryName = category.parentId === null ? category.name : '- ' + category.name;

				if (categoryTransactions.length) {
					for (const transaction of categoryTransactions) {
						expenditure.amount += transaction.amount;
					}
				}

				expendituresByCategory.push(expenditure);
			}

			for (const expenditure of expendituresByCategory) {
				const subExpenditures = expendituresByCategory.filter((e) => e.amount !== 0 && e.parentCategoryId === expenditure.categoryId);

				if (subExpenditures.length === 1) {
					expenditure.categoryId = subExpenditures[0].categoryId;
					expenditure.categoryName += '/' + subExpenditures[0].categoryName?.replace('- ', '');
					expenditure.amount = subExpenditures[0].amount;
				} else if (subExpenditures.length > 1) {
					expenditure.amount += subExpenditures.map((c) => c.amount).reduce((prev, curr) => prev + curr, 0);
					expenditure.subItems = subExpenditures.sort((a, b) => b.amount - a.amount);
				}
			}

			const uncategorizedTransactions = transactions.filter((t) => t.categoryId === null);
			if (uncategorizedTransactions.length) {
				const expenditure = new AmountByCategory(null, null, uncategorizedLabel, 0);

				for (const transaction of uncategorizedTransactions) {
					expenditure.amount += transaction.amount;
				}

				expendituresByCategory.push(expenditure);
			}

			return expendituresByCategory.filter((e) => e.amount !== 0 && e.parentCategoryId === null).sort((a, b) => b.amount - a.amount);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getExpendituresAndDepositsByCategory(
		fromDate: string,
		toDate: string,
		accountId: number,
		type: TransactionType,
		currency: string,
		uncategorizedLabel: string
	): Promise<AmountByCategory[]> {
		try {
			let transactions = await this.idbHelper.getExpendituresAndDepositsBetweenDates(fromDate, toDate, accountId, type);

			if (type === TransactionType.Expense) {
				transactions = transactions.filter((x) => !x.isTax);
			}

			return await this.getByCategory(transactions, currency, uncategorizedLabel);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getForBarChart(
		fromDate: string,
		mainAccountId: number,
		categoryId: number,
		type: TransactionType,
		currency: string
	): Promise<TransactionModel[]> {
		try {
			let transactions = await this.idbHelper.getForBarChart(fromDate, mainAccountId, categoryId, type);

			let filterOutTaxTransactions = true;
			if (categoryId) {
				const category = await this.categoriesService.get(categoryId);
				if (category.isTax) {
					filterOutTaxTransactions = false;
				}
			}

			if (filterOutTaxTransactions) {
				transactions = transactions.filter((x) => !x.isTax);
			}

			transactions.forEach((x: TransactionModel) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});

			return transactions;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getExpendituresFrom(mainAccountId: number, fromDate: Date, currency: string) {
		try {
			let transactions = await this.idbHelper.getExpendituresFrom(mainAccountId, fromDate);

			transactions = transactions.filter((x) => !x.isTax);

			transactions.forEach((x: TransactionModel) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});
			return transactions;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	get(id: number): Promise<TransactionModel> {
		return this.idbHelper.get(id);
	}

	async getForViewing(id: number, currency: string): Promise<TransactionModel> {
		try {
			const transaction = await this.idbHelper.get(id);
			transaction.convertedAmount = this.currenciesService.convert(transaction.amount, transaction.currency, currency);
			return transaction;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	static validate(
		amount: number,
		amountFrom: number,
		amountTo: number,
		fromAccountId: number | null,
		toAccountId: number | null,
		date: string,
		encrypt: boolean,
		encryptionPassword: string | null
	): ValidationResult {
		const result = new ValidationResult();

		if (!ValidationUtil.between(amount, amountFrom, amountTo)) {
			result.fail('amount');
		}

		if (!fromAccountId && !toAccountId) {
			result.fail('accountsMissing');
		} else if (fromAccountId === toAccountId) {
			result.fail('accountsEqual');
		}

		if (!date) {
			result.fail('date');
		}

		if (encrypt && ValidationUtil.isEmptyOrWhitespace(encryptionPassword)) {
			result.fail('encryptionPassword');
		}

		return result;
	}

	async create(
		fromAccountId: number | null,
		toAccountId: number | null,
		categoryId: number | null,
		amount: number,
		toStocks: number | null,
		currency: string,
		description: string | null,
		date: string,
		encrypt: boolean,
		password: string | null
	): Promise<void> {
		try {
			if (!fromAccountId && !toAccountId) {
				throw new Error('AccountId is missing.');
			}

			if (typeof amount === 'string') {
				amount = parseFloat(amount);
			}

			if (toStocks && typeof toStocks === 'string') {
				toStocks = parseFloat(toStocks);
			}

			if (description) {
				description = description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}

			let encryptedDescription: string | null = null;
			let salt: string | null = null;
			let nonce: string | null = null;
			if (encrypt) {
				if (!description) {
					throw new Error('Encrypted description cannot be null');
				}
				if (!password) {
					throw new Error('Encrypted description needs a password');
				}

				const result = await this.encryptionService.encrypt(description, password);
				encryptedDescription = result.encryptedData;
				salt = result.salt;
				nonce = result.nonce;
				description = null;
			}

			const transaction = new TransactionModel(
				0,
				fromAccountId,
				toAccountId,
				categoryId,
				amount,
				null,
				toStocks,
				currency,
				description,
				date,
				encrypt,
				encryptedDescription,
				salt,
				nonce,
				false,
				null,
				null
			);

			await this.createTransaction(transaction);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async buySellStocks(
		fromAccountId: number | null,
		toAccountId: number | null,
		amount: number,
		fromStocks: number | null,
		toStocks: number | null,
		currency: string
	): Promise<void> {
		try {
			if (!fromAccountId && !toAccountId) {
				throw new Error('AccountId is missing.');
			}

			if (typeof amount === 'string') {
				amount = parseFloat(amount);
			}

			if (fromStocks && typeof fromStocks === 'string') {
				fromStocks = parseFloat(fromStocks);
			}
			if (toStocks && typeof toStocks === 'string') {
				toStocks = parseFloat(toStocks);
			}

			const transaction = new TransactionModel(
				0,
				fromAccountId,
				toAccountId,
				null,
				amount,
				fromStocks,
				toStocks,
				currency,
				null,
				DateHelper.format(new Date()),
				false,
				null,
				null,
				null,
				false,
				null,
				null
			);

			await this.createTransaction(transaction);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	private async createTransaction(transaction: TransactionModel): Promise<void> {
		const now = new Date();
		transaction.createdDate = now;
		transaction.modifiedDate = now;

		if (navigator.onLine) {
			const payload = new CreateTransaction(
				transaction.fromAccountId,
				transaction.toAccountId,
				transaction.categoryId,
				transaction.amount,
				transaction.fromStocks,
				transaction.toStocks,
				transaction.currency,
				transaction.description,
				transaction.date,
				transaction.isEncrypted,
				transaction.encryptedDescription,
				transaction.salt,
				transaction.nonce,
				transaction.createdDate,
				transaction.modifiedDate
			);

			transaction.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/transactions`, {
				method: 'post',
				body: window.JSON.stringify(payload)
			});
			transaction.synced = true;
		}

		await this.idbHelper.create(transaction);

		this.localStorage.setLastSyncedNow();
	}

	async update(transaction: TransactionModel, password: string | null): Promise<void> {
		try {
			if (!transaction.fromAccountId && !transaction.toAccountId) {
				throw new Error('AccountId is missing.');
			}

			if (typeof transaction.amount === 'string') {
				transaction.amount = parseFloat(transaction.amount);
			}

			if (transaction.fromStocks && typeof transaction.fromStocks === 'string') {
				transaction.fromStocks = parseFloat(transaction.fromStocks);
			}
			if (transaction.toStocks && typeof transaction.toStocks === 'string') {
				transaction.toStocks = parseFloat(transaction.toStocks);
			}

			if (transaction.description) {
				transaction.description = transaction.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}

			if (transaction.isEncrypted) {
				if (!password) {
					throw new Error('Encryption password cannot be null');
				}
				if (!transaction.description) {
					throw new Error('Encrypted description cannot be null');
				}

				const result = await this.encryptionService.encrypt(transaction.description, password);
				transaction.encryptedDescription = result.encryptedData;
				transaction.salt = result.salt;
				transaction.nonce = result.nonce;
				transaction.description = null;
			} else {
				transaction.encryptedDescription = null;
				transaction.salt = null;
				transaction.nonce = null;
			}

			transaction.modifiedDate = new Date();

			if (navigator.onLine) {
				const payload = new UpdateTransaction(
					transaction.id,
					transaction.fromAccountId,
					transaction.toAccountId,
					transaction.categoryId,
					transaction.amount,
					transaction.fromStocks,
					transaction.toStocks,
					transaction.currency,
					transaction.description,
					transaction.date,
					transaction.isEncrypted,
					transaction.encryptedDescription,
					transaction.salt,
					transaction.nonce,
					transaction.createdDate,
					transaction.modifiedDate
				);

				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/transactions`, {
					method: 'put',
					body: window.JSON.stringify(payload)
				});
				transaction.synced = true;
			} else if (await this.idbHelper.isSynced(transaction.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(transaction);

			this.localStorage.setLastSyncedNow();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/transactions/${id}`, {
					method: 'delete'
				});
			} else if (await this.idbHelper.isSynced(id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.delete(id);

			this.localStorage.setLastSyncedNow();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async adjust(accountId: number, amount: number, description: string, currency: string) {
		try {
			const date = DateHelper.format(new Date());
			const isExpense = amount < 0;
			amount = Math.abs(amount);

			const transaction = new TransactionModel(
				0,
				null,
				null,
				null,
				amount,
				null,
				null,
				currency,
				description,
				date,
				false,
				null,
				null,
				null,
				false,
				null,
				null
			);

			if (isExpense) {
				transaction.fromAccountId = accountId;
			} else {
				transaction.toAccountId = accountId;
			}

			await this.createTransaction(transaction);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	export(fileId: string): Promise<Blob> {
		return this.httpProxy.ajaxBlob(`${Variables.urls.api}/transactions/export`, {
			method: 'post',
			body: window.JSON.stringify({
				fileId: fileId
			}),
			headers: {
				Accept: 'text/csv'
			}
		});
	}

	async deleteExportedFile(fileId: string): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/transactions/exported-file/${fileId}`, {
				method: 'delete'
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	static getType(fromAccountId: number | null, toAccountId: number | null): TransactionType {
		if (fromAccountId && toAccountId) {
			return TransactionType.Transfer;
		}

		if (fromAccountId && !toAccountId) {
			return TransactionType.Expense;
		}

		return TransactionType.Deposit;
	}

	release() {
		this.httpProxy.release();
		this.currenciesService.release();
		this.logger.release();
	}
}
