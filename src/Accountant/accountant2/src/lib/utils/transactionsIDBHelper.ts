import { DateHelper } from '../../../../../Core/shared2/utils/dateHelper';

import type { TransactionModel } from '$lib/models/entities/transaction';
import type { CreatedIdPair } from '$lib/models/server/responses/sync';
import { IDBContext } from './idbContext';
import { TransactionType } from '$lib/models/viewmodels/transactionType';
import type { SearchFilters } from '$lib/models/viewmodels/searchFilters';
import type { Category } from '$lib/models/entities/category';

export class TransactionsIDBHelper {
	private readonly db = new IDBContext();

	getAll(): Promise<TransactionModel[]> {
		return this.db.transactions.toArray();
	}

	async getAllForAccount(accountId: number): Promise<TransactionModel[]> {
		const transactions = await this.db.transactions.toArray();
		return transactions.filter((t) => t.fromAccountId === accountId || t.toAccountId === accountId);
	}

	async getAllSavingTransactionsInThePastYear(mainAccountId: number): Promise<TransactionModel[]> {
		const transactions = await this.db.transactions.toArray();

		const now = new Date();
		const aYearAgo = new Date(now.getFullYear() - 1, now.getMonth(), 1);
		const firstOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);

		const investmentFundAccountIds = await this.getInvestmentFundAccountIds();

		return transactions.filter(
			(t) =>
				new Date(t.date) >= aYearAgo &&
				new Date(t.date) < firstOfMonth &&
				this.isSavingOrWithdrawingFromSavings(t.fromAccountId, t.toAccountId, mainAccountId, investmentFundAccountIds)
		);
	}

	async count(filters: SearchFilters): Promise<number> {
		const categoryIds = await this.getWithSubCategoryIds(filters.categoryId);

		return await this.db.transactions
			.filter((t) =>
				this.checkAgainstFilters(
					t,
					filters.fromDate,
					filters.toDate,
					filters.accountId,
					filters.categoryId !== 0,
					categoryIds,
					filters.type,
					filters.description
				)
			)
			.count();
	}

	async getAllByPage(filters: SearchFilters): Promise<TransactionModel[]> {
		const categoryIds = await this.getWithSubCategoryIds(filters.categoryId);

		const transactions = await this.db.transactions
			.orderBy('date')
			.reverse()
			.filter((t) =>
				this.checkAgainstFilters(
					t,
					filters.fromDate,
					filters.toDate,
					filters.accountId,
					filters.categoryId !== 0,
					categoryIds,
					filters.type,
					filters.description
				)
			)
			.offset((filters.page - 1) * filters.pageSize)
			.limit(filters.pageSize)
			.toArray();

		// Order by date, then by modifiedDate
		return transactions.sort((a: TransactionModel, b: TransactionModel) => {
			const aDate = new Date(a.date);
			const bDate = new Date(b.date);
			if (aDate > bDate) return -1;
			if (aDate < bDate) return 1;

			const aModDate = new Date(<Date>a.modifiedDate);
			const bModDate = new Date(<Date>b.modifiedDate);
			if (aModDate > bModDate) return -1;
			if (aModDate < bModDate) return 1;
			return 0;
		});
	}

	async getForPieChart(fromDate: string, toDate: string, mainAccountId: number, type: TransactionType): Promise<TransactionModel[]> {
		const investmentFundAccountIds = await this.getInvestmentFundAccountIds();

		const transactionsPromise = this.db.transactions
			.orderBy('date')
			.reverse()
			.filter((t) => {
				const withinDates = (!fromDate || new Date(t.date) >= new Date(fromDate)) && (!toDate || new Date(t.date) <= new Date(toDate));
				if (!withinDates) {
					return false;
				}

				switch (type) {
					case TransactionType.Expense:
						if (!this.isExpense(t.fromAccountId, t.toAccountId)) {
							return false;
						}
						break;
					case TransactionType.Deposit:
						if (!this.isDeposit(t.fromAccountId, t.toAccountId)) {
							return false;
						}
						break;
				}

				if (t.fromAccountId === mainAccountId) {
					// Expense from main account
					return true;
				}

				if (t.toAccountId === mainAccountId) {
					// Deposit to main account
					return true;
				}

				if (t.toAccountId && investmentFundAccountIds.includes(t.toAccountId)) {
					// Deposit to savings account
					return true;
				}

				return false;
			})
			.toArray();

		const categoriesPromise = this.db.categories.toArray();

		const transactions = new Array<TransactionModel>();
		await Promise.all([transactionsPromise, categoriesPromise]).then((result) => {
			transactions.push(...result[0]);
			const categories = result[1];

			for (const transaction of transactions) {
				transaction.category = this.getCategoryWithParent(transaction.categoryId, categories);
			}
		});

		return transactions;
	}

	async getForBarChart(fromDate: string, mainAccountId: number, categoryId: number, type: TransactionType): Promise<TransactionModel[]> {
		const categoryIds = await this.getWithSubCategoryIds(categoryId);

		const now = new Date();
		const firstOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);

		const investmentFundAccountIds = await this.getInvestmentFundAccountIds();
		const transactionsPromise = this.db.transactions
			.orderBy('date')
			.reverse()
			.filter((t) => {
				if (new Date(t.date) < new Date(fromDate) || new Date(t.date) >= firstOfMonth) {
					return false;
				}

				if (categoryId !== 0) {
					if (categoryIds === null) {
						if (t.categoryId !== null) {
							return false;
						}
					} else if (!categoryIds.includes(<number>t.categoryId)) {
						return false;
					}
				}

				switch (type) {
					case TransactionType.Any:
						if (this.isTransfer(t.fromAccountId, t.toAccountId)) {
							return false;
						}
						if (
							t.fromAccountId !== mainAccountId &&
							t.toAccountId !== mainAccountId &&
							t.toAccountId !== null &&
							!investmentFundAccountIds.includes(t.toAccountId)
						) {
							// If the transaction isn't related to the main account or investment fund accounts
							return false;
						}
						break;
					case TransactionType.Expense:
						if (t.fromAccountId !== mainAccountId || !!t.toAccountId) {
							return false;
						}
						break;
					case TransactionType.Deposit:
						if (t.fromAccountId) {
							return false;
						}
						if (t.toAccountId !== null && t.toAccountId !== mainAccountId && !investmentFundAccountIds.includes(t.toAccountId)) {
							return false;
						}
						break;
					case TransactionType.Saving:
						if (!this.isSavingOrWithdrawingFromSavings(t.fromAccountId, t.toAccountId, mainAccountId, investmentFundAccountIds)) {
							return false;
						}
						break;
				}

				return true;
			})
			.toArray();

		const categoriesPromise = this.db.categories.toArray();

		const transactions = new Array<TransactionModel>();
		await Promise.all([transactionsPromise, categoriesPromise]).then((result) => {
			transactions.push(...result[0]);
			const categories = result[1];

			for (const transaction of transactions) {
				transaction.category = this.getCategoryWithParent(transaction.categoryId, categories);
			}
		});

		return transactions;
	}

	/**
	 * Finds whether a transaction fits the filters.
	 * @param t The transaction
	 * @param fromDate
	 * @param toDate
	 * @param accountId
	 * @param searchByCategory False if the all categories option is selected
	 * @param categoryIds The selected category plus any potential child categories
	 * @param type
	 * @param description
	 * @returns true if a transaction matches the filters
	 */
	private checkAgainstFilters(
		t: TransactionModel,
		fromDate: string,
		toDate: string,
		accountId: number,
		searchByCategory: boolean,
		categoryIds: number[] | null,
		type: TransactionType,
		description: string | null
	): boolean {
		const withinDates = (!fromDate || new Date(t.date) >= new Date(fromDate)) && (!toDate || new Date(t.date) <= new Date(toDate));
		if (!withinDates) {
			return false;
		}

		const inAccount = accountId === 0 || t.fromAccountId === accountId || t.toAccountId === accountId;
		if (!inAccount) {
			return false;
		}

		let inCategory = true;
		if (searchByCategory) {
			if (categoryIds === null) {
				inCategory = t.categoryId === null;
			} else {
				inCategory = categoryIds.includes(<number>t.categoryId);
			}
		}
		if (!inCategory) {
			return false;
		}

		let inType = type === TransactionType.Any;
		if (!inType) {
			if (!accountId) {
				if (type === TransactionType.Transfer) {
					inType = this.isTransfer(t.toAccountId, t.fromAccountId);
				} else if (type === TransactionType.Expense) {
					inType = this.isExpense(t.fromAccountId, t.toAccountId);
				} else {
					inType = this.isDeposit(t.fromAccountId, t.toAccountId);
				}
			} else {
				// Based on selected account
				if (type === TransactionType.Expense) {
					inType = !!t.fromAccountId && accountId === t.fromAccountId;
				} else if (type === TransactionType.Deposit) {
					inType = !!t.toAccountId && accountId === t.toAccountId;
				}
			}
		}
		if (!inType) {
			return false;
		}

		const hasDescription = !description || (!!t.description && t.description.toUpperCase().includes(description.toUpperCase()));

		return hasDescription;
	}

	/**
	 * Finds whether a transaction fits the filters.
	 * @param t The transaction
	 * @param fromDate
	 * @param firstOfMonth
	 * @param mainAccountId
	 * @param investmentFundAccountIds Used for savings calculations
	 * @param searchByCategory False if the all categories option is selected
	 * @param categoryIds The selected category plus any potential child categories
	 * @param type
	 * @returns true if a transaction matches the filters
	 */
	private checkAgainstFilters3(
		t: TransactionModel,
		fromDate: string,
		firstOfMonth: Date,
		mainAccountId: number,
		investmentFundAccountIds: number[],
		searchByCategory: boolean,
		categoryIds: number[] | null,
		type: TransactionType
	): boolean {
		if (new Date(t.date) < new Date(fromDate) || new Date(t.date) >= firstOfMonth) {
			return false;
		}

		if (searchByCategory) {
			if (categoryIds === null) {
				return t.categoryId === null;
			}

			return categoryIds.includes(<number>t.categoryId);
		}

		switch (type) {
			case TransactionType.Any:
				if (this.isTransfer(t.fromAccountId, t.toAccountId)) {
					return false;
				}
				if (
					t.fromAccountId !== mainAccountId &&
					t.toAccountId !== mainAccountId &&
					t.toAccountId !== null &&
					!investmentFundAccountIds.includes(t.toAccountId)
				) {
					// If the money isn't related to the main account or the investment fund accounts
					return false;
				}
				break;
			case TransactionType.Expense:
				if (t.fromAccountId !== mainAccountId || !!t.toAccountId) {
					return false;
				}
				break;
			case TransactionType.Deposit:
				if (t.fromAccountId) {
					return false;
				}
				if (t.toAccountId !== null && t.toAccountId !== mainAccountId && !investmentFundAccountIds.includes(t.toAccountId)) {
					return false;
				}
				break;
			case TransactionType.Saving:
				if (!this.isSavingOrWithdrawingFromSavings(t.fromAccountId, t.toAccountId, mainAccountId, investmentFundAccountIds)) {
					return false;
				}
				break;
		}

		return true;
	}

	private isExpense(fromAccountId: number | null, toAccountId: number | null) {
		return !!fromAccountId && !toAccountId;
	}

	private isDeposit(fromAccountId: number | null, toAccountId: number | null) {
		return !fromAccountId && !!toAccountId;
	}

	private isTransfer(fromAccountId: number | null, toAccountId: number | null) {
		return !!fromAccountId && !!toAccountId;
	}

	private isSavingOrWithdrawingFromSavings(
		fromAccountId: number | null,
		toAccountId: number | null,
		mainAccountId: number,
		investmentFundAccountIds: number[]
	) {
		// Transferred from main account
		if (fromAccountId === mainAccountId && !!toAccountId) {
			return true;
		}

		// Withdrawn from main account
		if (toAccountId === mainAccountId && !!fromAccountId) {
			return true;
		}

		// Deposited to investment fund account directly
		if (!fromAccountId && toAccountId && investmentFundAccountIds.includes(toAccountId)) {
			return true;
		}

		return false;
	}

	async getExpendituresForCurrentMonth(accountId: number): Promise<TransactionModel[]> {
		const now = new Date();
		const from = new Date(now.getFullYear(), now.getMonth(), 1);
		const formatted = DateHelper.format(from);

		const transactionsPromise = this.db.transactions
			.where('date')
			.aboveOrEqual(formatted)
			.and((t: TransactionModel) => t.fromAccountId === accountId && !t.toAccountId)
			.toArray();

		const categoriesPromise = this.db.categories.toArray();

		const transactions = new Array<TransactionModel>();
		await Promise.all([transactionsPromise, categoriesPromise]).then((result) => {
			transactions.push(...result[0]);
			const categories = result[1];

			for (const transaction of transactions) {
				transaction.category = this.getCategoryWithParent(transaction.categoryId, categories);
			}
		});

		return transactions;
	}

	async getExpendituresFrom(mainAccountId: number, fromDate: Date): Promise<TransactionModel[]> {
		const transactionsPromise = this.db.transactions
			.orderBy('date')
			.reverse()
			.filter((t: TransactionModel) => (!fromDate || new Date(t.date) >= new Date(fromDate)) && t.fromAccountId === mainAccountId && !t.toAccountId)
			.toArray();

		const categoriesPromise = this.db.categories.toArray();

		const transactions = new Array<TransactionModel>();
		await Promise.all([transactionsPromise, categoriesPromise]).then((result) => {
			transactions.push(...result[0]);
			const categories = result[1];

			for (const transaction of transactions) {
				transaction.category = this.getCategoryWithParent(transaction.categoryId, categories);
			}
		});

		return transactions;
	}

	async get(id: number): Promise<TransactionModel> {
		const transaction = await this.db.transactions.get(id);

		if (!transaction) {
			throw new Error('Could not find transaction');
		}

		return transaction;
	}

	async isSynced(id: number): Promise<boolean> {
		const transaction = await this.db.transactions.get(id);

		if (!transaction) {
			throw new Error('Could not find transaction');
		}

		return transaction && transaction.synced;
	}

	async create(transaction: TransactionModel): Promise<void> {
		if (!transaction.synced) {
			transaction.id = await this.generateId();
		}

		await this.db.transaction('rw', this.db.transactions, this.db.upcomingExpenses, async () => {
			await this.db.transactions.add(transaction);

			if (transaction.fromAccountId && !transaction.toAccountId) {
				const relatedUpcomingExpenses = await this.db.upcomingExpenses
					.filter(
						(x) =>
							x.categoryId == transaction.categoryId &&
							new Date(x.date).getFullYear() == new Date(transaction.date).getFullYear() &&
							new Date(x.date).getMonth() == new Date(transaction.date).getMonth()
					)
					.toArray();

				if (relatedUpcomingExpenses.length > 0) {
					for (var upcomingExpense of relatedUpcomingExpenses) {
						const bothWithDescriptionsAndTheyMatch =
							upcomingExpense.description &&
							!!transaction.description &&
							upcomingExpense.description.toUpperCase() === transaction.description.toUpperCase();

						if (!upcomingExpense.description || bothWithDescriptionsAndTheyMatch) {
							if (upcomingExpense.amount > transaction.amount) {
								upcomingExpense.amount -= transaction.amount;
								upcomingExpense.modifiedDate = new Date();
								await this.db.upcomingExpenses.update(upcomingExpense.id, upcomingExpense);
							} else {
								await this.db.upcomingExpenses.delete(upcomingExpense.id);
							}
						}
					}
				}
			}
		});
	}

	async update(transaction: TransactionModel): Promise<void> {
		await this.db.transactions.update(transaction.id, transaction);
	}

	async delete(id: number): Promise<void> {
		await this.db.transactions.delete(id);
	}

	async sync(deletedTransactionIds: number[], transactions: TransactionModel[]) {
		await this.db.transaction('rw', this.db.transactions, async () => {
			if (deletedTransactionIds.length > 0) {
				await this.db.transactions.bulkDelete(deletedTransactionIds);
			}

			if (transactions.length > 0) {
				for (const transaction of transactions) {
					transaction.date = transaction.date.split('T')[0];
					transaction.synced = true;
				}
				await this.db.transactions.bulkPut(transactions);
			}
		});
	}

	async getForSyncing(): Promise<TransactionModel[]> {
		const transactions = this.db.transactions.toCollection();

		return transactions.filter((t) => !t.synced).toArray();
	}

	async consolidate(transactionIdPairs: CreatedIdPair[]) {
		if (transactionIdPairs.length === 0) {
			return;
		}

		await this.db.transaction('rw', this.db.transactions, async () => {
			for (const transactionIdPair of transactionIdPairs) {
				const transaction = await this.db.transactions.get(transactionIdPair.localId);

				if (!transaction) {
					continue;
				}

				await this.db.transactions.delete(transactionIdPair.localId);

				transaction.id = transactionIdPair.id;
				transaction.date = transaction.date.split('T')[0];
				transaction.synced = true;
				await this.db.transactions.add(transaction);
			}
		});
	}

	private async getInvestmentFundAccountIds() {
		return (await this.db.accounts.filter((a) => !!a.stockPrice).toArray()).map((x) => x.id);
	}

	private async getWithSubCategoryIds(categoryId: number | null): Promise<Array<number> | null> {
		if (categoryId === null) {
			return null;
		}

		if (categoryId === 0) {
			return [];
		}

		const categoryIds = new Array<number>();
		categoryIds.push(categoryId);

		const subCategories = await this.db.categories.where('parentId').equals(categoryId).toArray();

		categoryIds.push(...subCategories.map((x) => x.id));

		return categoryIds;
	}

	private getCategoryWithParent(categoryId: number | null, categories: Category[]): Category | null {
		if (categoryId === null) {
			return null;
		}

		const category = categories.find((x) => x.id === categoryId);

		if (!category) {
			throw new Error('Could not find category');
		}

		if (category.parentId) {
			category.parent = categories.find((x) => x.id === category.parentId);
		}

		return category;
	}

	private async generateId(): Promise<number> {
		const transactions = this.db.transactions.toCollection();

		const keys = await transactions.primaryKeys();
		if (keys.length === 0) {
			return 1;
		}

		const sortedKeys = keys.sort((a: number, b: number) => {
			return a - b;
		});
		return sortedKeys[sortedKeys.length - 1] + 1;
	}
}
