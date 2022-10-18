import type { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
import type { CreatedIdPair } from '$lib/models/sync';
import { IDBContext } from './idbContext';

export class UpcomingExpensesIDBHelper {
	private readonly db = new IDBContext();

	async getAll(): Promise<Array<UpcomingExpense>> {
		const upcomingExpenses = await this.db.upcomingExpenses.toArray();

		for (const upcomingExpense of upcomingExpenses) {
			if (upcomingExpense.categoryId) {
				const category = await this.db.categories.get(upcomingExpense.categoryId);

				if (!category) {
					throw new Error('Could not find category');
				}

				upcomingExpense.categoryName = category.name;
			}
		}

		return upcomingExpenses.sort((a: UpcomingExpense, b: UpcomingExpense) => {
			const aDate = new Date(a.date);
			const bDate = new Date(b.date);
			if (aDate > bDate) return 1;
			if (aDate < bDate) return -1;

			return b.amount - a.amount;
		});
	}

	async getAllForMonth(): Promise<Array<UpcomingExpense>> {
		const month = new Date().getMonth();

		const upcomingExpenses = await this.db.upcomingExpenses
			.filter((x) => new Date(x.date).getMonth() === month)
			.toArray();

		const categories = await this.db.categories.toArray();

		for (const upcomingExpense of upcomingExpenses) {
			if (upcomingExpense.categoryId) {
				const category = categories.find((x) => x.id === upcomingExpense.categoryId);

				if (!category) {
					throw new Error('Could not find category');
				}

				if (category.parentId) {
					const parent = categories.find((x) => x.id === category.parentId);

					if (!parent) {
						throw new Error('Could not find parent category');
					}

					upcomingExpense.categoryName = `${parent.name}/${category.name}`;
				} else {
					upcomingExpense.categoryName = category.name;
				}
			}
		}

		return upcomingExpenses;
	}

	async get(id: number): Promise<UpcomingExpense> {
		const upcomingExpense = await this.db.upcomingExpenses.get(id);

		if (!upcomingExpense) {
			throw new Error('Could not find upcoming expense');
		}

		return upcomingExpense;
	}

	async isSynced(id: number): Promise<boolean> {
		const upcomingExpense = await this.db.upcomingExpenses.get(id);

		if (!upcomingExpense) {
			throw new Error('Could not find upcoming expense');
		}

		return upcomingExpense && upcomingExpense.synced;
	}

	async create(upcomingExpense: UpcomingExpense): Promise<void> {
		if (!upcomingExpense.synced) {
			upcomingExpense.id = await this.generateId();
		}

		await this.db.upcomingExpenses.add(upcomingExpense);
	}

	async update(upcomingExpense: UpcomingExpense): Promise<void> {
		await this.db.upcomingExpenses.update(upcomingExpense.id, upcomingExpense);
	}

	async delete(id: number): Promise<void> {
		await this.db.upcomingExpenses.delete(id);
	}

	async sync(deletedUpcomingExpenseIds: number[], upcomingExpenses: UpcomingExpense[]) {
		await this.db.transaction('rw', this.db.upcomingExpenses, async () => {
			if (deletedUpcomingExpenseIds.length > 0) {
				await this.db.upcomingExpenses.bulkDelete(deletedUpcomingExpenseIds);
			}

			if (upcomingExpenses.length > 0) {
				for (const upcomingExpense of upcomingExpenses) {
					upcomingExpense.date = upcomingExpense.date.split('T')[0];
					upcomingExpense.synced = true;
				}
				await this.db.upcomingExpenses.bulkPut(upcomingExpenses);
			}
		});
	}

	async getForSyncing(): Promise<Array<UpcomingExpense>> {
		const upcomingExpenses = this.db.upcomingExpenses.toCollection();

		return upcomingExpenses.filter((x) => !x.synced).toArray();
	}

	async consolidate(upcomingExpenseIdPairs: CreatedIdPair[]) {
		if (upcomingExpenseIdPairs.length === 0) {
			return;
		}

		await this.db.transaction('rw', this.db.upcomingExpenses, async () => {
			for (const upcomingExpenseIdPair of upcomingExpenseIdPairs) {
				const upcomingExpense = await this.db.upcomingExpenses.get(upcomingExpenseIdPair.localId);

				if (!upcomingExpense) {
					continue;
				}

				await this.db.upcomingExpenses.delete(upcomingExpenseIdPair.localId);

				upcomingExpense.id = upcomingExpenseIdPair.id;
				upcomingExpense.date = upcomingExpense.date.split('T')[0];
				upcomingExpense.synced = true;
				await this.db.upcomingExpenses.add(upcomingExpense);
			}
		});
	}

	async deleteOld(): Promise<void> {
		const now = new Date();
		const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1, 0, 0, 0);

		const upcomingExpensesToDelete = await this.db.upcomingExpenses
			.filter((x) => new Date(x.date) < startOfMonth)
			.toArray();

		if (upcomingExpensesToDelete.length > 0) {
			await this.db.transaction('rw', this.db.upcomingExpenses, async () => {
				const deletePromises = Array<Promise<void>>();
				for (const upcomingExpense of upcomingExpensesToDelete) {
					deletePromises.push(this.db.upcomingExpenses.delete(upcomingExpense.id));
				}
				await Promise.all(deletePromises);
			});
		}
	}

	private async generateId(): Promise<number> {
		const upcomingExpenses = this.db.upcomingExpenses.toCollection();

		const keys = await upcomingExpenses.primaryKeys();
		if (keys.length === 0) {
			return 1;
		}

		const sortedKeys = keys.sort((a: number, b: number) => {
			return a - b;
		});
		return sortedKeys[sortedKeys.length - 1] + 1;
	}
}
