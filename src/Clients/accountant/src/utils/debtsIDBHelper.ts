import { inject } from "aurelia-framework";

import { DebtModel } from "models/entities/debt";
import { CreatedIdPair } from "models/sync";
import { IDBContext } from "./idbContext";

@inject(IDBContext)
export class DebtsIDBHelper {
  constructor(private readonly db: IDBContext) {}

  async getAll(): Promise<Array<DebtModel>> {
    const debts = await this.db.debts.toArray();

    return debts.sort((a: DebtModel, b: DebtModel) => {
      return b.amount - a.amount;
    });
  }

  async get(id: number): Promise<DebtModel> {
    return await this.db.debts.get(id);
  }

  async getByPerson(person: string): Promise<DebtModel[]> {
    const debtWithPerson = await this.db.debts.filter((x) => x.person.toLowerCase() === person).toArray();
    return debtWithPerson.sort((a: DebtModel, b: DebtModel) => {
      const aDate = new Date(a.createdDate);
      const bDate = new Date(b.createdDate);
      if (aDate < bDate) return -1;
      if (aDate > bDate) return 1;

      return 0;
    });
  }

  async isSynced(id: number): Promise<boolean> {
    const debt = await this.db.debts.get(id);
    return debt && debt.synced;
  }

  async create(debt: DebtModel): Promise<void> {
    if (!debt.synced) {
      debt.id = await this.generateId();
    }

    await this.db.debts.add(debt);
  }

  async createMerged(debt: DebtModel, idsToDelete: number[]): Promise<void> {
    if (!debt.synced) {
      debt.id = await this.generateId();
    }

    await this.db.transaction("rw", this.db.debts, async () => {
      await this.db.debts.add(debt);
      await this.db.debts.bulkDelete(idsToDelete);
    });
  }

  async update(debt: DebtModel): Promise<void> {
    await this.db.debts.update(debt.id, debt);
  }

  async delete(id: number): Promise<void> {
    await this.db.debts.delete(id);
  }

  async sync(deletedDebtIds: number[], debts: DebtModel[]) {
    await this.db.transaction("rw", this.db.debts, async () => {
      if (deletedDebtIds.length > 0) {
        await this.db.debts.bulkDelete(deletedDebtIds);
      }

      if (debts.length > 0) {
        for (const debt of debts) {
          debt.synced = true;
        }
        await this.db.debts.bulkPut(debts);
      }
    });
  }

  async getForSyncing(): Promise<Array<DebtModel>> {
    const debts = this.db.debts.toCollection();

    return debts.filter((d) => !d.synced).toArray();
  }

  async consolidate(debtIdPairs: CreatedIdPair[]) {
    if (debtIdPairs.length === 0) {
      return;
    }

    await this.db.transaction("rw", this.db.debts, async () => {
      for (const debtIdPair of debtIdPairs) {
        const debt = await this.db.debts.get(debtIdPair.localId);

        await this.db.debts.delete(debtIdPair.localId);

        debt.id = debtIdPair.id;
        debt.synced = true;
        await this.db.debts.add(debt);
      }
    });
  }

  private async generateId(): Promise<number> {
    const debts = this.db.debts.toCollection();

    const keys = await debts.primaryKeys();
    if (keys.length === 0) {
      return 1;
    }

    const sortedKeys = keys.sort((a: number, b: number) => {
      return a - b;
    });
    return sortedKeys[sortedKeys.length - 1] + 1;
  }
}
