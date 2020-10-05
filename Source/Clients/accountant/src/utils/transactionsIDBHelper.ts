import { inject } from "aurelia-framework";
import { TransactionModel } from "models/entities/transaction";
import { CreatedIdPair } from "models/sync/created";
import { IDBContext } from "./idbContext";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { TransactionType } from "models/viewmodels/transactionType";
import { DateHelper } from "../../../shared/src/utils/dateHelper";
import { SearchFilters } from "models/viewmodels/searchFilters";

@inject(IDBContext)
export class TransactionsIDBHelper {
  constructor(private readonly db: IDBContext) {}

  async getAll(): Promise<Array<TransactionModel>> {
    return this.db.transactions.toArray();
  }

  async getAllForAccount(accountId: number): Promise<Array<TransactionModel>> {
    const transactions = await this.db.transactions.toArray();
    return transactions.filter(
      (x: TransactionModel) =>
        x.fromAccountId === accountId || x.toAccountId === accountId
    );
  }

  async count(filters: SearchFilters): Promise<number> {
    return await this.db.transactions
      .filter((t: TransactionModel): boolean =>
        this.checkAgainstFilters(
          t,
          filters.fromDate,
          filters.toDate,
          filters.categoryId,
          filters.accountId,
          filters.type,
          filters.description
        )
      )
      .count();
  }

  async getAllByPage(filters: SearchFilters): Promise<Array<TransactionModel>> {
    const transactions = await this.db.transactions
      .orderBy("date")
      .reverse()
      .filter((t: TransactionModel): boolean =>
        this.checkAgainstFilters(
          t,
          filters.fromDate,
          filters.toDate,
          filters.categoryId,
          filters.accountId,
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

      const aModDate = new Date(a.modifiedDate);
      const bModDate = new Date(b.modifiedDate);
      if (aModDate > bModDate) return -1;
      if (aModDate < bModDate) return 1;
      return 0;
    });
  }

  async getAllBetweenDates(
    fromDate: string,
    toDate: string,
    accountId: number,
    type: TransactionType
  ): Promise<Array<TransactionModel>> {
    const transactionsPromise = this.db.transactions
      .orderBy("date")
      .reverse()
      .filter((t: TransactionModel): boolean =>
        this.checkAgainstFilters2(t, fromDate, toDate, accountId, type)
      )
      .toArray();

    const categoriesPromise = this.db.categories.toArray();

    let transactions: Array<TransactionModel>;
    await Promise.all([transactionsPromise, categoriesPromise]).then(
      (result) => {
        transactions = result[0];
        const categories = result[1];

        for (const transaction of transactions) {
          if (transaction.categoryId) {
            const category = categories.find(
              (x) => x.id === transaction.categoryId
            );
            transaction.categoryName = category.name;
          }
        }
      }
    );

    return transactions;
  }

  async getExpensesAndDepositsFromDate(
    fromDate: string,
    accountId: number,
    type: TransactionType
  ): Promise<Array<TransactionModel>> {
    const transactions = await this.db.transactions
      .orderBy("date")
      .reverse()
      .filter((t: TransactionModel): boolean =>
        this.checkAgainstFilters3(t, fromDate, accountId, type)
      )
      .toArray();

    return transactions;
  }

  private checkAgainstFilters(
    t: TransactionModel,
    fromDate: string,
    toDate: string,
    categoryId: number,
    accountId: number,
    type: TransactionType,
    description: string
  ): boolean {
    let inType = type === TransactionType.Any;
    if (!inType) {
      if (!accountId) {
        if (type === TransactionType.Transfer) {
          inType = !!t.toAccountId && !!t.fromAccountId;
        } else if (type === TransactionType.Expense) {
          inType = !!t.fromAccountId && !t.toAccountId;
        } else {
          inType = !t.fromAccountId && !!t.toAccountId;
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

    const withinDates =
      (!fromDate || new Date(t.date) >= new Date(fromDate)) &&
      (!toDate || new Date(t.date) <= new Date(toDate));
    if (!withinDates) {
      return false;
    }

    const inCategory = categoryId === 0 || t.categoryId === categoryId;
    if (!inCategory) {
      return false;
    }

    let inAccount =
      accountId === 0 ||
      t.fromAccountId === accountId ||
      t.toAccountId === accountId;
    if (!inAccount) {
      return false;
    }

    const hasDescription =
      !description ||
      t.description?.toUpperCase().includes(description?.toUpperCase());

    return hasDescription;
  }

  private checkAgainstFilters2(
    t: TransactionModel,
    fromDate: string,
    toDate: string,
    accountId: number,
    type: TransactionType
  ): boolean {
    let inType =
      (type === TransactionType.Expense &&
        !!t.fromAccountId &&
        !t.toAccountId) ||
      (type === TransactionType.Deposit && !t.fromAccountId && !!t.toAccountId);
    if (!inType) {
      return false;
    }

    const withinDates =
      (!fromDate || new Date(t.date) >= new Date(fromDate)) &&
      (!toDate || new Date(t.date) <= new Date(toDate));
    if (!withinDates) {
      return false;
    }

    let inAccount =
      t.fromAccountId === accountId || t.toAccountId === accountId;

    return inAccount;
  }

  /**
   * Finds whether a transaction fits the filters. Ignores Transfer transaction types.
   * @param t The transaction
   * @param fromDate
   * @param accountId
   * @param type
   * @returns true if a transaction fits the filters
   */
  private checkAgainstFilters3(
    t: TransactionModel,
    fromDate: string,
    accountId: number,
    type: TransactionType
  ): boolean {
    let inType =
      (type === TransactionType.Any &&
        !(!!t.fromAccountId && !!t.toAccountId)) || // Ignore Transfers if type is Any
      (type === TransactionType.Expense &&
        !!t.fromAccountId &&
        !t.toAccountId) ||
      (type === TransactionType.Deposit && !t.fromAccountId && !!t.toAccountId);
    if (!inType) {
      return false;
    }

    const afterDate = new Date(t.date) >= new Date(fromDate);
    if (!afterDate) {
      return false;
    }

    let inAccount =
      t.fromAccountId === accountId || t.toAccountId === accountId;

    return inAccount;
  }

  async getExpendituresForCurrentMonth(
    accountId: number
  ): Promise<Array<TransactionModel>> {
    const now = new Date();
    const from = new Date(now.getFullYear(), now.getMonth(), 1);
    const formatted = DateHelper.format(from);

    const transactionsPromise = this.db.transactions
      .where("date")
      .aboveOrEqual(formatted)
      .filter(
        (t: TransactionModel) => t.fromAccountId === accountId && !t.toAccountId
      )
      .toArray();

    const categoriesPromise = this.db.categories.toArray();

    let transactions: Array<TransactionModel>;
    await Promise.all([transactionsPromise, categoriesPromise]).then(
      (result) => {
        transactions = result[0];
        const categories = result[1];

        for (const transaction of transactions) {
          if (transaction.categoryId) {
            const category = categories.find(
              (x) => x.id === transaction.categoryId
            );
            transaction.categoryName = category.name;
          }
        }
      }
    );

    return transactions;
  }

  async getExpendituresFrom(
    mainAccountId: number,
    fromDate: Date
  ): Promise<Array<TransactionModel>> {
    const transactionsPromise = this.db.transactions
      .orderBy("date")
      .reverse()
      .filter(
        (t: TransactionModel) =>
          (!fromDate || new Date(t.date) >= new Date(fromDate)) &&
          t.fromAccountId === mainAccountId &&
          !t.toAccountId
      )
      .toArray();

    const categoriesPromise = this.db.categories.toArray();

    let transactions: Array<TransactionModel>;
    await Promise.all([transactionsPromise, categoriesPromise]).then(
      (result) => {
        transactions = result[0];
        const categories = result[1];

        for (const transaction of transactions) {
          if (transaction.categoryId) {
            const category = categories.find(
              (x) => x.id === transaction.categoryId
            );
            transaction.categoryName = category.name;
          }
        }
      }
    );

    return transactions;
  }

  get(id: number): Promise<TransactionModel> {
    return this.db.transactions.get(id);
  }

  async isSynced(id: number): Promise<boolean> {
    const transaction = await this.db.transactions.get(id);
    return transaction && transaction.synced;
  }

  async create(transaction: TransactionModel): Promise<void> {
    if (!transaction.synced) {
      transaction.id = await this.generateId();
    }

    await this.db.transaction(
      "rw",
      this.db.transactions,
      this.db.upcomingExpenses,
      async () => {
        await this.db.transactions.add(transaction);

        if (transaction.fromAccountId && !transaction.toAccountId) {
          const relatedUpcomingExpenses = await this.db.upcomingExpenses
            .filter((x: UpcomingExpense): boolean => {
              return x.categoryId == transaction.categoryId;
            })
            .toArray();

          if (relatedUpcomingExpenses.length > 0) {
            const transactionHasDescription = !!transaction.description;

            for (var upcomingExpense of relatedUpcomingExpenses) {
              const upcomingExpenseHasDescription = !!upcomingExpense.description;
              const bothWithDescriptionsAndTheyMatch =
                upcomingExpenseHasDescription &&
                transactionHasDescription &&
                upcomingExpense.description.toUpperCase() ===
                  transaction.description.toUpperCase();

              if (
                !upcomingExpenseHasDescription ||
                bothWithDescriptionsAndTheyMatch
              ) {
                if (upcomingExpense.amount > transaction.amount) {
                  upcomingExpense.amount -= transaction.amount;
                  upcomingExpense.modifiedDate = new Date();
                  await this.db.upcomingExpenses.update(
                    upcomingExpense.id,
                    upcomingExpense
                  );
                } else {
                  await this.db.upcomingExpenses.delete(upcomingExpense.id);
                }
              }
            }
          }
        }
      }
    );
  }

  async createMultiple(
    ...transactions: Array<TransactionModel>
  ): Promise<void> {
    let id = await this.generateId();

    for (const transaction of transactions) {
      if (!transaction.synced) {
        transaction.id = id;
        id++;
      }
    }

    await this.db.transactions.bulkAdd(transactions);
  }

  async update(transaction: TransactionModel): Promise<void> {
    await this.db.transactions.update(transaction.id, transaction);
  }

  async delete(id: number): Promise<void> {
    await this.db.transactions.delete(id);
  }

  async sync(
    deletedTransactionIds: Array<number>,
    transactions: Array<TransactionModel>
  ) {
    await this.db.transaction("rw", this.db.transactions, async () => {
      if (deletedTransactionIds.length > 0) {
        await this.db.transactions.bulkDelete(deletedTransactionIds);
      }

      if (transactions.length > 0) {
        for (const transaction of transactions) {
          transaction.date = transaction.date.split("T")[0];
          transaction.synced = true;
        }
        await this.db.transactions.bulkPut(transactions);
      }
    });
  }

  async getForSyncing(): Promise<Array<TransactionModel>> {
    const transactions = this.db.transactions.toCollection();

    return transactions
      .filter((t: TransactionModel) => {
        return !t.synced;
      })
      .toArray();
  }

  async consolidate(transactionIdPairs: Array<CreatedIdPair>) {
    if (transactionIdPairs.length === 0) {
      return;
    }

    await this.db.transaction("rw", this.db.transactions, async () => {
      for (const transactionIdPair of transactionIdPairs) {
        const transaction = await this.db.transactions.get(
          transactionIdPair.localId
        );

        await this.db.transactions.delete(transactionIdPair.localId);

        transaction.id = transactionIdPair.id;
        transaction.date = transaction.date.split("T")[0];
        transaction.synced = true;
        await this.db.transactions.add(transaction);
      }
    });
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
