import { inject } from "aurelia-framework";
import { Category, CategoryType } from "models/entities/category";
import { CreatedIdPair } from "models/sync/created";
import { IDBContext } from "./idbContext";

@inject(IDBContext)
export class CategoriesIDBHelper {
  constructor(private readonly db: IDBContext) {}

  getAll(): Promise<Array<Category>> {
    return this.db.categories.toArray();
  }

  async getAllAsOptions(type: CategoryType): Promise<Array<Category>> {
    const categories = await this.db.categories
      .filter((x: Category): boolean => {
        return type === 0 || x.type === 0 || x.type === type;
      })
      .toArray();

    const getCountPromises = Array<Promise<void>>();
    for (const category of categories) {
      getCountPromises.push(
        this.db.transactions
          .where("categoryId")
          .equals(category.id)
          .count()
          .then((count: number) => {
            (<any>category).transactionsCount = count;
          })
      );
    }

    await Promise.all(getCountPromises);

    // Order by transaction count, then by modifiedDate
    return categories.sort((a: Category, b: Category) => {
      if ((<any>a).transactionsCount > (<any>b).transactionsCount) return -1;
      if ((<any>a).transactionsCount < (<any>b).transactionsCount) return 1;
      if (new Date(a.modifiedDate) > new Date(b.modifiedDate)) return -1;
      if (new Date(a.modifiedDate) < new Date(b.modifiedDate)) return 1;
      return 0;
    });
  }

  get(id: number): Promise<Category> {
    return this.db.categories.get(id);
  }

  async isSynced(id: number): Promise<boolean> {
    const category = await this.db.categories.get(id);
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
    await this.db.transaction(
      "rw",
      this.db.categories,
      this.db.transactions,
      function* () {
        yield this.db.transactions.where("categoryId").equals(id).delete();
        yield this.db.categories.delete(id);
      }
    );
  }

  async hasTransactions(id: number): Promise<boolean> {
    const transactionsCount = await this.db.transactions
      .where("categoryId")
      .equals(id)
      .count();

    return transactionsCount > 0;
  }

  async sync(deletedCategoryIds: Array<number>, categories: Array<Category>) {
    await this.db.transaction(
      "rw",
      this.db.categories,
      this.db.transactions,
      this.db.upcomingExpenses,
      async () => {
        if (deletedCategoryIds.length > 0) {
          for (const categoryId of deletedCategoryIds) {
            await this.db.transactions
              .where("categoryId")
              .equals(categoryId)
              .delete();
            await this.db.upcomingExpenses
              .where("categoryId")
              .equals(categoryId)
              .delete();
            await this.db.categories.delete(categoryId);
          }
        }

        if (categories.length > 0) {
          for (const category of categories) {
            category.synced = true;
          }
          await this.db.categories.bulkPut(categories);
        }
      }
    );
  }

  async getForSyncing(): Promise<Array<Category>> {
    const categories = this.db.categories.toCollection();

    return categories
      .filter((c: Category) => {
        return !c.synced;
      })
      .toArray();
  }

  async consolidate(categoryIdPairs: Array<CreatedIdPair>) {
    if (categoryIdPairs.length === 0) {
      return;
    }

    await this.db.transaction("rw", this.db.categories, async () => {
      for (const categoryIdPair of categoryIdPairs) {
        const category = await this.db.categories.get(categoryIdPair.localId);

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
