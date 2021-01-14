import { inject } from "aurelia-framework";

import { UpcomingExpense } from "models/entities/upcomingExpense";
import { CreatedIdPair } from "models/sync/created";
import { IDBContext } from "./idbContext";

@inject(IDBContext)
export class UpcomingExpensesIDBHelper {
  constructor(private readonly db: IDBContext) {}

  async getAll(): Promise<Array<UpcomingExpense>> {
    const upcomingExpenses = await this.db.upcomingExpenses
      .orderBy("date")
      .toArray();

    for (const upcomingExpense of upcomingExpenses) {
      if (upcomingExpense.categoryId) {
        const category = await this.db.categories.get(
          upcomingExpense.categoryId
        );
        upcomingExpense.categoryName = category.name;
      }
    }

    return upcomingExpenses;
  }

  async getAllForMonth(): Promise<Array<UpcomingExpense>> {
    const month = new Date().getMonth();

    const upcomingExpenses = await this.db.upcomingExpenses
      .orderBy("date")
      .filter(x => new Date(x.date).getMonth() === month)
      .toArray();

    const categories = await this.db.categories.toArray();

    for (const upcomingExpense of upcomingExpenses) {
      if (upcomingExpense.categoryId) {
        const category = categories.find(x => x.id === upcomingExpense.categoryId);

        if (category.parentId) {
          const parent = categories.find(x => x.id === category.parentId);
          upcomingExpense.categoryName = `${parent.name}/${category.name}`;
        } else {
          upcomingExpense.categoryName = category.name;
        }
      }
    }

    return upcomingExpenses;
  }

  get(id: number): Promise<UpcomingExpense> {
    return this.db.upcomingExpenses.get(id);
  }

  async isSynced(id: number): Promise<boolean> {
    const upcomingExpense = await this.db.upcomingExpenses.get(id);
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

  async sync(
    deletedUpcomingExpenseIds: Array<number>,
    upcomingExpenses: Array<UpcomingExpense>
  ) {
    await this.db.transaction("rw", this.db.upcomingExpenses, async () => {
      if (deletedUpcomingExpenseIds.length > 0) {
        await this.db.upcomingExpenses.bulkDelete(deletedUpcomingExpenseIds);
      }

      if (upcomingExpenses.length > 0) {
        for (const upcomingExpense of upcomingExpenses) {
          upcomingExpense.synced = true;
        }
        await this.db.upcomingExpenses.bulkPut(upcomingExpenses);
      }
    });
  }

  async getForSyncing(): Promise<Array<UpcomingExpense>> {
    const upcomingExpenses = this.db.upcomingExpenses.toCollection();

    return upcomingExpenses
      .filter(x => !x.synced)
      .toArray();
  }

  async consolidate(upcomingExpenseIdPairs: Array<CreatedIdPair>) {
    if (upcomingExpenseIdPairs.length === 0) {
      return;
    }

    await this.db.transaction("rw", this.db.upcomingExpenses, async () => {
      for (const upcomingExpenseIdPair of upcomingExpenseIdPairs) {
        const upcomingExpense = await this.db.upcomingExpenses.get(
          upcomingExpenseIdPair.localId
        );

        await this.db.upcomingExpenses.delete(upcomingExpenseIdPair.localId);

        upcomingExpense.id = upcomingExpenseIdPair.id;
        upcomingExpense.synced = true;
        await this.db.upcomingExpenses.add(upcomingExpense);
      }
    });
  }

  async deleteOld(): Promise<void> {
    const now = new Date();
    const startOfMonth = new Date(now.getFullYear(), 0, 1, 0, 0, 0);

    const upcomingExpensesToDelete = await this.db.upcomingExpenses
      .filter(x => new Date(x.date) < startOfMonth)
      .toArray();

    if (upcomingExpensesToDelete.length > 0) {
      await this.db.transaction("rw", this.db.upcomingExpenses, async () => {
        const deletePromises = Array<Promise<void>>();
        for (const upcomingExpense of upcomingExpensesToDelete) {
          deletePromises.push(
            this.db.upcomingExpenses.delete(upcomingExpense.id)
          );
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
