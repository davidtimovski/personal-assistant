import { autoinject } from "aurelia-framework";

import { AutomaticTransaction } from "models/entities/automaticTransaction";
import { CreatedIdPair } from "models/sync";
import { IDBContext } from "./idbContext";

@autoinject
export class AutomaticTransactionsIDBHelper {
  constructor(private readonly db: IDBContext) {}

  async getAll(): Promise<Array<AutomaticTransaction>> {
    const automaticTransactions = await this.db.automaticTransactions.toArray();

    for (const automaticTransaction of automaticTransactions) {
      if (automaticTransaction.categoryId) {
        const category = await this.db.categories.get(automaticTransaction.categoryId);
        automaticTransaction.categoryName = category.name;
      }
    }

    return automaticTransactions.sort((a: AutomaticTransaction, b: AutomaticTransaction) =>
      a.dayInMonth > b.dayInMonth ? 1 : -1
    );
  }

  get(id: number): Promise<AutomaticTransaction> {
    return this.db.automaticTransactions.get(id);
  }

  async isSynced(id: number): Promise<boolean> {
    const automaticTransaction = await this.db.automaticTransactions.get(id);
    return automaticTransaction && automaticTransaction.synced;
  }

  async create(automaticTransaction: AutomaticTransaction): Promise<void> {
    if (!automaticTransaction.synced) {
      automaticTransaction.id = await this.generateId();
    }

    await this.db.automaticTransactions.add(automaticTransaction);
  }

  async update(automaticTransaction: AutomaticTransaction): Promise<void> {
    await this.db.automaticTransactions.update(automaticTransaction.id, automaticTransaction);
  }

  async delete(id: number): Promise<void> {
    await this.db.automaticTransactions.delete(id);
  }

  async sync(deletedAutomaticTransactionIds: number[], automaticTransactions: AutomaticTransaction[]) {
    await this.db.transaction("rw", this.db.automaticTransactions, async () => {
      if (deletedAutomaticTransactionIds.length > 0) {
        await this.db.automaticTransactions.bulkDelete(deletedAutomaticTransactionIds);
      }

      if (automaticTransactions.length > 0) {
        for (const automaticTransaction of automaticTransactions) {
          automaticTransaction.synced = true;
        }
        await this.db.automaticTransactions.bulkPut(automaticTransactions);
      }
    });
  }

  async getForSyncing(): Promise<Array<AutomaticTransaction>> {
    const automaticTransactions = this.db.automaticTransactions.toCollection();

    return automaticTransactions.filter((x) => !x.synced).toArray();
  }

  async consolidate(automaticTransactionIdPairs: CreatedIdPair[]) {
    if (automaticTransactionIdPairs.length === 0) {
      return;
    }

    await this.db.transaction("rw", this.db.automaticTransactions, async () => {
      for (const automaticTransactionIdPair of automaticTransactionIdPairs) {
        const automaticTransaction = await this.db.automaticTransactions.get(automaticTransactionIdPair.localId);

        await this.db.automaticTransactions.delete(automaticTransactionIdPair.localId);

        automaticTransaction.id = automaticTransactionIdPair.id;
        automaticTransaction.synced = true;
        await this.db.automaticTransactions.add(automaticTransaction);
      }
    });
  }

  private async generateId(): Promise<number> {
    const automaticTransactions = this.db.automaticTransactions.toCollection();

    const keys = await automaticTransactions.primaryKeys();
    if (keys.length === 0) {
      return 1;
    }

    const sortedKeys = keys.sort((a: number, b: number) => {
      return a - b;
    });
    return sortedKeys[sortedKeys.length - 1] + 1;
  }
}
