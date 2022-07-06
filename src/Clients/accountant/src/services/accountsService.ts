import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { AccountsIDBHelper } from "../utils/accountsIDBHelper";
import { TransactionsIDBHelper } from "../utils/transactionsIDBHelper";
import { Account } from "models/entities/account";
import { SelectOption } from "models/viewmodels/selectOption";
import { TransactionModel } from "models/entities/transaction";
import * as environment from "../../config/environment.json";

@inject(AuthService, HttpClient, EventAggregator, AccountsIDBHelper, TransactionsIDBHelper, CurrenciesService)
export class AccountsService extends HttpProxyBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly idbHelper: AccountsIDBHelper,
    private readonly transactionsIDBHelper: TransactionsIDBHelper,
    private readonly currenciesService: CurrenciesService
  ) {
    super(authService, httpClient, eventAggregator);
  }

  getMainId(): Promise<number> {
    return this.idbHelper.getMainId();
  }

  async getAllWithBalance(currency: string): Promise<Array<Account>> {
    try {
      const accounts = await this.idbHelper.getAll();

      const getBalancePromises = new Array<Promise<void>>();
      for (const account of accounts) {
        if (account.stockPrice === null) {
          const getBalancePromise = this.getBalance(account.id, currency).then((balance: number) => {
            account.balance = balance;
          });
          getBalancePromises.push(getBalancePromise);
        } else {
          const getBalancePromise = this.getBalanceAndStocks(account, currency).then(([balance, stocks]) => {
            account.balance = balance;
            account.stocks = stocks;
          });
          getBalancePromises.push(getBalancePromise);
        }
      }

      await Promise.all(getBalancePromises);

      return accounts;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async getAllAsOptions(): Promise<Array<SelectOption>> {
    try {
      const accounts = await this.idbHelper.getAllAsOptions();

      const options = new Array<SelectOption>();
      for (const account of accounts) {
        options.push(new SelectOption(account.id, account.name));
      }

      return options;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async getNonInvestmentFundsAsOptions(): Promise<Array<SelectOption>> {
    try {
      const accounts = await this.idbHelper.getAllAsOptions(true);

      const options = new Array<SelectOption>();
      for (const account of accounts) {
        options.push(new SelectOption(account.id, account.name));
      }

      return options;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async get(id: number): Promise<Account> {
    return this.idbHelper.get(id);
  }

  async getBalance(id: number, currency: string): Promise<number> {
    try {
      const transactions = await this.transactionsIDBHelper.getAllForAccount(id);

      let balance = 0;
      transactions.forEach((x: TransactionModel) => {
        if (id === x.fromAccountId) {
          balance -= this.currenciesService.convert(x.amount, x.currency, currency);
        } else if (id === x.toAccountId) {
          balance += this.currenciesService.convert(x.amount, x.currency, currency);
        }
      });

      return parseFloat(balance.toFixed(2));
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async getAverageMonthlySavingsFromThePastYear(currency: string) {
    try {
      const mainAccountId = await this.getMainId();
      const transferTransactions = await this.transactionsIDBHelper.getAllSavingTransactionsInThePastYear(
        mainAccountId
      );

      const movedFromMain = transferTransactions.filter((x) => x.fromAccountId === mainAccountId);
      const movedToMain = transferTransactions.filter((x) => x.toAccountId === mainAccountId);

      let saving = 0;

      saving += movedFromMain
        .map((x) => this.currenciesService.convert(x.amount, x.currency, currency))
        .reduce((a, b) => a + b, 0);
      saving -= movedToMain
        .map((x) => this.currenciesService.convert(x.amount, x.currency, currency))
        .reduce((a, b) => a + b, 0);

      const earliestTransaction = transferTransactions.sort((a: TransactionModel, b: TransactionModel) => {
        const aDate = new Date(a.date);
        const bDate = new Date(b.date);
        if (aDate < bDate) return -1;
        if (aDate > bDate) return 1;

        return 0;
      })[0];

      const now = new Date();
      const earliestTransactionDate = new Date(earliestTransaction.date);
      const monthsPassed =
        now.getMonth() -
        earliestTransactionDate.getMonth() +
        12 * (now.getFullYear() - earliestTransactionDate.getFullYear());
      const savingsPerMonth = saving / monthsPassed;

      return parseFloat(savingsPerMonth.toFixed(2));
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async getBalanceAndStocks(account: Account, currency: string): Promise<[number, number]> {
    try {
      const transactions = await this.transactionsIDBHelper.getAllForAccount(account.id);

      let stocks = 0;
      transactions.forEach((x: TransactionModel) => {
        if (account.id === x.fromAccountId) {
          stocks -= x.fromStocks;
        } else if (account.id === x.toAccountId) {
          stocks += x.toStocks;
        }
      });

      const amount = stocks * account.stockPrice;
      const balance = this.currenciesService.convert(amount, account.currency, currency);

      return [parseFloat(balance.toFixed(2)), parseInt(stocks.toString())];
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async create(account: Account): Promise<number> {
    try {
      const now = DateHelper.adjustForTimeZone(new Date());
      account.createdDate = account.modifiedDate = now;

      if (navigator.onLine) {
        account.id = await this.ajax<number>("accounts", {
          method: "post",
          body: json(account),
        });
        account.synced = true;
      }

      await this.idbHelper.create(account);

      return account.id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async update(account: Account): Promise<void> {
    try {
      account.modifiedDate = DateHelper.adjustForTimeZone(new Date());

      if (navigator.onLine) {
        await this.ajaxExecute("accounts", {
          method: "put",
          body: json(account),
        });
        account.synced = true;
      } else if (await this.idbHelper.isSynced(account.id)) {
        throw "failedToFetchError";
      }

      await this.idbHelper.update(account);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number): Promise<void> {
    try {
      if (navigator.onLine) {
        await this.ajaxExecute(`accounts/${id}`, {
          method: "delete",
        });
      } else if (await this.idbHelper.isSynced(id)) {
        throw "failedToFetchError";
      }
      await this.idbHelper.delete(id);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async hasTransactions(id: number): Promise<boolean> {
    return this.idbHelper.hasTransactions(id);
  }
}
