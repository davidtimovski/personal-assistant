import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { TransactionsIDBHelper } from "../utils/transactionsIDBHelper";
import { TransactionModel } from "models/entities/transaction";
import { TransactionType } from "models/viewmodels/transactionType";
import { CategoriesService } from "./categoriesService";
import { EncryptionService } from "./encryptionService";
import { SearchFilters } from "models/viewmodels/searchFilters";
import { AmountByCategory } from "models/viewmodels/amountByCategory";

@inject(
  AuthService,
  HttpClient,
  EventAggregator,
  TransactionsIDBHelper,
  CategoriesService,
  CurrenciesService,
  EncryptionService
)
export class TransactionsService extends HttpProxyBase {
  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly idbHelper: TransactionsIDBHelper,
    private readonly categoriesService: CategoriesService,
    private readonly currenciesService: CurrenciesService,
    private readonly encryptionService: EncryptionService
  ) {
    super(authService, httpClient, eventAggregator);
  }

  async count(filters: SearchFilters): Promise<number> {
    return await this.idbHelper.count(filters);
  }

  async getAllByPage(filters: SearchFilters, currency: string): Promise<Array<TransactionModel>> {
    const transactions = await this.idbHelper.getAllByPage(filters);

    transactions.forEach((x: TransactionModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    return transactions;
  }

  async getByCategory(
    transactions: Array<TransactionModel>,
    currency: string,
    uncategorizedLabel: string
  ): Promise<Array<AmountByCategory>> {
    transactions.forEach((x: TransactionModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    const categories = await this.categoriesService.getAll();
    const expendituresByCategory = new Array<AmountByCategory>();

    for (const category of categories) {
      const categoryTransactions = transactions.filter((t) => t.categoryId === category.id);
      const expenditure = new AmountByCategory(category.id, category.parentId, null, 0);
      expenditure.categoryName = category.parentId === null ? category.name : "- " + category.name;

      if (categoryTransactions.length) {
        for (const transaction of categoryTransactions) {
          expenditure.amount += transaction.amount;
        }
      }

      expendituresByCategory.push(expenditure);
    }

    for (const expenditure of expendituresByCategory) {
      const subExpenditures = expendituresByCategory.filter(
        (e) => e.amount !== 0 && e.parentCategoryId === expenditure.categoryId
      );

      if (subExpenditures.length) {
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

    return expendituresByCategory
      .filter((e) => e.amount !== 0 && e.parentCategoryId === null)
      .sort((a, b) => b.amount - a.amount);
  }

  async getExpendituresAndDepositsByCategory(
    fromDate: string,
    toDate: string,
    accountId: number,
    type: TransactionType,
    currency: string,
    uncategorizedLabel: string
  ): Promise<Array<AmountByCategory>> {
    const transactions = await this.idbHelper.getExpendituresAndDepositsBetweenDates(fromDate, toDate, accountId, type);

    return await this.getByCategory(transactions, currency, uncategorizedLabel);
  }

  async getForBarChart(
    fromDate: string,
    mainAccountId: number,
    categoryId: number,
    type: TransactionType,
    currency: string
  ): Promise<Array<TransactionModel>> {
    const transactions = await this.idbHelper.getForBarChart(fromDate, mainAccountId, categoryId, type);

    transactions.forEach((x: TransactionModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    return transactions;
  }

  async getExpendituresFrom(mainAccountId: number, fromDate: Date, currency: string) {
    const transactions = await this.idbHelper.getExpendituresFrom(mainAccountId, fromDate);

    transactions.forEach((x: TransactionModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });
    return transactions;
  }

  async get(id: number): Promise<TransactionModel> {
    const transaction = await this.idbHelper.get(id);
    if (!transaction) {
      return null;
    }

    return transaction;
  }

  async getForViewing(id: number, currency: string): Promise<TransactionModel> {
    const transaction = await this.idbHelper.get(id);
    if (!transaction) {
      return null;
    }

    transaction.convertedAmount = this.currenciesService.convert(transaction.amount, transaction.currency, currency);
    return transaction;
  }

  async create(transaction: TransactionModel, password: string): Promise<void> {
    if (!transaction.fromAccountId && !transaction.toAccountId) {
      throw "AccountId is missing.";
    }

    transaction.amount = parseFloat(<any>transaction.amount);

    if (transaction.fromStocks) {
      transaction.fromStocks = parseFloat(<any>transaction.fromStocks);
    }
    if (transaction.toStocks) {
      transaction.toStocks = parseFloat(<any>transaction.toStocks);
    }

    if (transaction.description) {
      transaction.description = transaction.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
    }

    if (transaction.isEncrypted) {
      const result = await this.encryptionService.encrypt(transaction.description, password);
      transaction.encryptedDescription = result.encryptedData;
      transaction.salt = result.salt;
      transaction.nonce = result.nonce;
      transaction.description = null;
    }

    const now = DateHelper.adjustForTimeZone(new Date());
    transaction.createdDate = transaction.modifiedDate = now;

    if (navigator.onLine) {
      transaction.id = await this.ajax<number>("transactions", {
        method: "post",
        body: json(transaction),
      });
      transaction.synced = true;
    }

    await this.idbHelper.create(transaction);
  }

  async update(transaction: TransactionModel, password: string): Promise<void> {
    if (!transaction.fromAccountId && !transaction.toAccountId) {
      throw "AccountId is missing.";
    }

    transaction.amount = parseFloat(<any>transaction.amount);

    if (transaction.fromStocks) {
      transaction.fromStocks = parseFloat(<any>transaction.fromStocks);
    }
    if (transaction.toStocks) {
      transaction.toStocks = parseFloat(<any>transaction.toStocks);
    }

    if (transaction.description) {
      transaction.description = transaction.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
    }

    if (transaction.isEncrypted) {
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

    transaction.modifiedDate = DateHelper.adjustForTimeZone(new Date());

    if (navigator.onLine) {
      await this.ajaxExecute("transactions", {
        method: "put",
        body: json(transaction),
      });
      transaction.synced = true;
    } else if (await this.idbHelper.isSynced(transaction.id)) {
      throw "failedToFetchError";
    }

    await this.idbHelper.update(transaction);
  }

  async delete(id: number): Promise<void> {
    if (navigator.onLine) {
      await this.ajaxExecute(`transactions/${id}`, {
        method: "delete",
      });
    } else if (await this.idbHelper.isSynced(id)) {
      throw "failedToFetchError";
    }

    await this.idbHelper.delete(id);
  }

  async adjust(accountId: number, amount: number, description: string, currency: string) {
    const date = DateHelper.format(DateHelper.adjustForTimeZone(new Date()));
    const isExpense = amount < 0;
    amount = Math.abs(amount);

    const transaction = new TransactionModel(
      null,
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
      null,
      null
    );

    if (isExpense) {
      transaction.fromAccountId = accountId;
    } else {
      transaction.toAccountId = accountId;
    }

    await this.create(transaction, null);
  }

  async export(fileId: string): Promise<Blob> {
    return this.ajaxBlob("transactions/export", {
      method: "post",
      body: json({
        fileId: fileId,
      }),
      headers: {
        Accept: "text/csv",
      },
    });
  }

  async deleteExportedFile(fileId: string): Promise<void> {
    await this.ajaxExecute(`transactions/exported-file/${fileId}`, {
      method: "delete",
    });
  }
}
