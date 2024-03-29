import { autoinject } from "aurelia-framework";

import { Account } from "models/entities/account";
import { CreatedIdPair } from "models/sync";
import { IDBContext } from "./idbContext";
import { TransactionModel } from "models/entities/transaction";

@autoinject
export class AccountsIDBHelper {
  constructor(private readonly db: IDBContext) {}

  async getMainId(): Promise<number> {
    const account = await this.db.accounts.filter((a) => a.isMain).first();

    if (!account) {
      return;
    }

    return account.id;
  }

  async getAll(): Promise<Array<Account>> {
    const accounts = await this.db.accounts.toArray();

    return accounts.sort((a: Account, b: Account) => {
      if (a.isMain) return -1;
      if (b.isMain) return 1;
      return 0;
    });
  }

  async getAllAsOptions(excludeFunds: boolean = false): Promise<Array<Account>> {
    let accounts: Account[];

    if (excludeFunds) {
      accounts = await this.db.accounts.filter((x) => !x.stockPrice).toArray();
    } else {
      accounts = await this.db.accounts.toArray();
    }

    const getCountPromises = Array<Promise<void>>();
    for (const account of accounts) {
      getCountPromises.push(
        this.db.transactions
          .filter((x: TransactionModel) => x.fromAccountId === account.id || x.toAccountId === account.id)
          .count()
          .then((count: number) => {
            (<any>account).transactionsCount = count;
          })
      );
    }

    await Promise.all(getCountPromises);

    // Order by transaction count, then by modifiedDate
    return accounts.sort((a: Account, b: Account) => {
      if ((<any>a).transactionsCount > (<any>b).transactionsCount) return -1;
      if ((<any>a).transactionsCount < (<any>b).transactionsCount) return 1;
      if (new Date(a.modifiedDate) > new Date(b.modifiedDate)) return -1;
      if (new Date(a.modifiedDate) < new Date(b.modifiedDate)) return 1;
      return 0;
    });
  }

  get(id: number): Promise<Account> {
    return this.db.accounts.get(id);
  }

  async isSynced(id: number): Promise<boolean> {
    const account = await this.db.accounts.get(id);
    return account && account.synced;
  }

  async create(account: Account): Promise<void> {
    if (!account.synced) {
      account.id = await this.generateId();
    }

    await this.db.accounts.add(account);
  }

  async update(account: Account): Promise<void> {
    await this.db.accounts.update(account.id, account);
  }

  async delete(id: number): Promise<void> {
    await this.db.transaction("rw", this.db.accounts, this.db.transactions, function* () {
      yield this.db.transactions.where("fromAccountId").equals(id).delete();
      yield this.db.accounts.delete(id);
      yield this.db.transactions.where("toAccountId").equals(id).delete();
      yield this.db.accounts.delete(id);
    });
  }

  async hasTransactions(id: number): Promise<boolean> {
    let transactionsCount = 0;
    const getCountPromises = Array<Promise<void>>();

    getCountPromises.push(
      this.db.transactions
        .where("fromAccountId")
        .equals(id)
        .count()
        .then((count: number) => {
          transactionsCount += count;
        })
    );

    getCountPromises.push(
      this.db.transactions
        .where("toAccountId")
        .equals(id)
        .count()
        .then((count: number) => {
          transactionsCount += count;
        })
    );

    await Promise.all(getCountPromises);

    return transactionsCount > 0;
  }

  async sync(deletedAccountIds: number[], accounts: Account[]) {
    await this.db.transaction("rw", this.db.accounts, this.db.transactions, async () => {
      if (deletedAccountIds.length > 0) {
        for (const accountId of deletedAccountIds) {
          await this.db.transactions.where("fromAccountId").equals(accountId).delete();
          await this.db.transactions.where("toAccountId").equals(accountId).delete();
          await this.db.accounts.delete(accountId);
        }
      }

      if (accounts.length > 0) {
        for (const account of accounts) {
          account.synced = true;
        }
        await this.db.accounts.bulkPut(accounts);
      }
    });
  }

  async getForSyncing(): Promise<Array<Account>> {
    const accounts = this.db.accounts.toCollection();

    return accounts.filter((a) => !a.synced).toArray();
  }

  async consolidate(accountIdPairs: CreatedIdPair[]) {
    if (accountIdPairs.length === 0) {
      return;
    }

    await this.db.transaction("rw", this.db.accounts, async () => {
      for (const accountIdPair of accountIdPairs) {
        const account = await this.db.accounts.get(accountIdPair.localId);

        await this.db.accounts.delete(accountIdPair.localId);

        account.id = accountIdPair.id;
        account.synced = true;
        await this.db.accounts.add(account);
      }
    });
  }

  private async generateId(): Promise<number> {
    const accounts = this.db.accounts.toCollection();

    const keys = await accounts.primaryKeys();
    if (keys.length === 0) {
      return 1;
    }

    const sortedKeys = keys.sort((a: number, b: number) => {
      return a - b;
    });
    return sortedKeys[sortedKeys.length - 1] + 1;
  }
}
