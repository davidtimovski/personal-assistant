import type { Category, CategoryType } from '$lib/models/entities/category';
import type { CreatedIdPair } from '$lib/models/server/responses/sync';
import { IDBContext } from './idbContext';

export class CategoriesIDBHelper {
	private readonly db = new IDBContext();

	getAll(): Promise<Category[]> {
		return this.db.categories.toArray();
	}

	async getAllAsOptions(type: CategoryType): Promise<Category[]> {
		let categories = await this.db.categories.toArray();

		categories = categories.filter((x: Category): boolean => {
			return type === 0 || x.type === 0 || x.type === type;
		});

		const getCountPromises = Array<Promise<void>>();
		for (const category of categories) {
			if (category.parentId !== null) {
				category.parent = categories.find((x) => x.id === category.parentId);
			}

			const getCountPromise = this.db.transactions
				.where('categoryId')
				.equals(category.id)
				.count()
				.then((count: number) => {
					(<any>category).transactionsCount = count;
				});
			getCountPromises.push(getCountPromise);
		}

		await Promise.all(getCountPromises);

		// Order by transaction count, then by modifiedDate
		return categories.sort((a: Category, b: Category) => {
			if ((<any>a).transactionsCount > (<any>b).transactionsCount) return -1;
			if ((<any>a).transactionsCount < (<any>b).transactionsCount) return 1;
			if (new Date(<Date>a.modifiedDate) > new Date(<Date>b.modifiedDate)) return -1;
			if (new Date(<Date>a.modifiedDate) < new Date(<Date>b.modifiedDate)) return 1;
			return 0;
		});
	}

	async getParentAsOptions(): Promise<Category[]> {
		let categories = await this.db.categories.filter((c) => c.parentId === null).toArray();

		return categories.sort((a: Category, b: Category) => {
			return a.name > b.name ? 1 : -1;
		});
	}

	async get(id: number): Promise<Category> {
		const category = await this.db.categories.get(id);

		if (!category) {
			throw new Error('Could not find category');
		}

		if (category.parentId !== null) {
			category.parent = await this.db.categories.get(category.parentId);
		}

		return category;
	}

	async isParent(id: number): Promise<boolean> {
		const subCategories = await this.db.categories.where('parentId').equals(id).count();

		return subCategories > 0;
	}

	async isSynced(id: number): Promise<boolean> {
		const category = await this.db.categories.get(id);

		if (!category) {
			throw new Error('Could not find category');
		}

		return category && category.synced;
	}

	async create(category: Category): Promise<void> {
		if (!category.synced) {
			category.id = await this.generateId();
		}

		await this.db.categories.add(category);
	}

	async update(category: Category): Promise<void> {
		await this.db.categories.update(category.id, category);
	}

	async delete(id: number): Promise<void> {
		const self = this;

		await this.db.transaction('rw', this.db.categories, this.db.transactions, function* () {
			yield self.db.transactions.where('categoryId').equals(id).delete();
			yield self.db.categories.delete(id);

			const subCategories: Category[] = yield self.db.categories.where('parentId').equals(id).toArray();
			for (const subCategory of subCategories) {
				subCategory.parentId = null;
				yield self.db.categories.update(subCategory.id, subCategory);
			}
		});
	}

	async hasTransactions(id: number): Promise<boolean> {
		const transactionsCount = await this.db.transactions.where('categoryId').equals(id).count();

		return transactionsCount > 0;
	}

	async sync(deletedCategoryIds: number[], categories: Category[]) {
		await this.db.transaction('rw', this.db.categories, this.db.transactions, this.db.upcomingExpenses, async () => {
			if (deletedCategoryIds.length > 0) {
				for (const categoryId of deletedCategoryIds) {
					await this.db.transactions.where('categoryId').equals(categoryId).delete();
					await this.db.upcomingExpenses.where('categoryId').equals(categoryId).delete();
					await this.db.categories.delete(categoryId);
				}
			}

			if (categories.length > 0) {
				for (const category of categories) {
					category.synced = true;
				}
				await this.db.categories.bulkPut(categories);
			}
		});
	}

	async getForSyncing(): Promise<Category[]> {
		const categories = this.db.categories.toCollection();

		return categories.filter((c) => !c.synced).toArray();
	}

	async consolidate(categoryIdPairs: CreatedIdPair[]) {
		if (categoryIdPairs.length === 0) {
			return;
		}

		await this.db.transaction('rw', this.db.categories, async () => {
			for (const categoryIdPair of categoryIdPairs) {
				const category = await this.db.categories.get(categoryIdPair.localId);

				if (!category) {
					continue;
				}

				await this.db.categories.delete(categoryIdPair.localId);

				category.id = categoryIdPair.id;
				category.synced = true;
				await this.db.categories.add(category);
			}
		});
	}

	private async generateId(): Promise<number> {
		const categories = this.db.categories.toCollection();

		const keys = await categories.primaryKeys();
		if (keys.length === 0) {
			return 1;
		}

		const sortedKeys = keys.sort((a: number, b: number) => {
			return a - b;
		});
		return sortedKeys[sortedKeys.length - 1] + 1;
	}
}
