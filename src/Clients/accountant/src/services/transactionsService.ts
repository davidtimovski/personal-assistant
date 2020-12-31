import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { TransactionsIDBHelper } from "../utils/transactionsIDBHelper";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { TransactionModel } from "models/entities/transaction";
import { TransactionType } from "models/viewmodels/transactionType";
import { DateHelper } from "../../../shared/src/utils/dateHelper";
import { EncryptionService } from "./encryptionService";
import { SearchFilters } from "models/viewmodels/searchFilters";

@inject(
  AuthService,
  HttpClient,
  EventAggregator,
  TransactionsIDBHelper,
  CurrenciesService,
  EncryptionService
)
export class TransactionsService extends HttpProxyBase {
  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly idbHelper: TransactionsIDBHelper,
    private readonly currenciesService: CurrenciesService,
    private readonly encryptionService: EncryptionService
  ) {
    super(authService, httpClient, eventAggregator);
  }

  async count(filters: SearchFilters): Promise<number> {
    return await this.idbHelper.count(filters);
  }

  async getAllByPage(
    filters: SearchFilters,
    currency: string
  ): Promise<Array<TransactionModel>> {
    const transactions = await this.idbHelper.getAllByPage(filters);

    transactions.forEach((x: TransactionModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    return transactions;
  }

  async getAllBetweenDates(
    fromDate: string,
    toDate: string,
    accountId: number,
    type: TransactionType,
    currency: string
  ): Promise<Array<TransactionModel>> {
    const transactions = await this.idbHelper.getAllBetweenDates(
      fromDate,
      toDate,
      accountId,
      type
    );

    transactions.forEach((x: TransactionModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    return transactions;
  }

  async getExpensesAndDepositsFromDate(
    fromDate: string,
    accountId: number,
    type: TransactionType,
    currency: string
  ): Promise<Array<TransactionModel>> {
    const transactions = await this.idbHelper.getExpensesAndDepositsFromDate(
      fromDate,
      accountId,
      type
    );

    transactions.forEach((x: TransactionModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    return transactions;
  }

  async getExpendituresFrom(
    mainAccountId: number,
    fromDate: Date,
    currency: string
  ) {
    const transactions = await this.idbHelper.getExpendituresFrom(
      mainAccountId,
      fromDate
    );

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

    transaction.convertedAmount = this.currenciesService.convert(
      transaction.amount,
      transaction.currency,
      currency
    );
    return transaction;
  }

  async create(transaction: TransactionModel, password: string): Promise<void> {
    if (!transaction.fromAccountId && !transaction.toAccountId) {
      throw "AccountId is missing.";
    }

    transaction.amount = parseFloat(<any>transaction.amount);

    if (transaction.description) {
      transaction.description = transaction.description
        .replace(/(\r\n|\r|\n){3,}/g, "$1\n")
        .trim();
    }

    if (transaction.isEncrypted) {
      const result = await this.encryptionService.encrypt(
        transaction.description,
        password
      );
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

    if (transaction.description) {
      transaction.description = transaction.description
        .replace(/(\r\n|\r|\n){3,}/g, "$1\n")
        .trim();
    }

    if (transaction.isEncrypted) {
      const result = await this.encryptionService.encrypt(
        transaction.description,
        password
      );
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

  async adjust(
    accountId: number,
    amount: number,
    description: string,
    currency: string
  ) {
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
