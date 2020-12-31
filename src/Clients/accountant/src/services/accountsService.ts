import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { AccountsIDBHelper } from "../utils/accountsIDBHelper";
import { TransactionsIDBHelper } from "../utils/transactionsIDBHelper";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { Account } from "models/entities/account";
import { SelectOption } from "models/viewmodels/selectOption";
import { TransactionModel } from "models/entities/transaction";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

@inject(
  AuthService,
  HttpClient,
  EventAggregator,
  AccountsIDBHelper,
  TransactionsIDBHelper,
  CurrenciesService
)
export class AccountsService extends HttpProxyBase {
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
    const accounts = await this.idbHelper.getAll();

    const getBalancePromises = new Array<Promise<void>>();
    for (const account of accounts) {
      if (account.stockPrice === null) {
        const getBalancePromise = this.getBalance(account.id, currency).then(
          (balance: number) => {
            account.balance = balance;
          }
        );
        getBalancePromises.push(getBalancePromise);
      } else {
        const getBalancePromise = this.getBalanceAndStocks(account, currency).then(
          ([balance, stocks]) => {
            account.balance = balance;
            account.stocks = stocks;
          }
        );
        getBalancePromises.push(getBalancePromise);
      }
    }

    await Promise.all(getBalancePromises);

    return accounts;
  }

  async getAllAsOptions(): Promise<Array<SelectOption>> {
    const accounts = await this.idbHelper.getAllAsOptions();

    const options = new Array<SelectOption>();
    for (const account of accounts) {
      options.push(new SelectOption(account.id, account.name));
    }

    return options;
  }

  async get(id: number): Promise<Account> {
    return this.idbHelper.get(id);
  }

  async getBalance(id: number, currency: string): Promise<number> {
    const transactions = await this.transactionsIDBHelper.getAllForAccount(id);

    let balance = 0;
    transactions.forEach((x: TransactionModel) => {
      if (id === x.fromAccountId) {
        balance -= this.currenciesService.convert(
          x.amount,
          x.currency,
          currency
        );
      } else if (id === x.toAccountId) {
        balance += this.currenciesService.convert(
          x.amount,
          x.currency,
          currency
        );
      }
    });
    
    return parseFloat(balance.toFixed(2));
  }

  async getBalanceAndStocks(account: Account, currency: string): Promise<[number, number]> {
    const transactions = await this.transactionsIDBHelper.getAllForAccount(account.id);

    let balance = 0;
    let stocks = 0;
    transactions.forEach((x: TransactionModel) => {
      if (account.id === x.fromAccountId) {
        stocks -= x.fromStocks;
      } else if (account.id === x.toAccountId) {
        stocks += x.toStocks;
      }
    });

    const amount = stocks * account.stockPrice;
    balance += this.currenciesService.convert(
      amount,
      account.currency,
      currency
    );
    
    return [parseFloat(balance.toFixed(2)), parseInt(stocks.toString())];
  }

  async create(account: Account): Promise<number> {
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
  }

  async update(account: Account): Promise<void> {
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
  }

  async delete(id: number): Promise<void> {
    if (navigator.onLine) {
      await this.ajaxExecute(`accounts/${id}`, {
        method: "delete",
      });
    } else if (await this.idbHelper.isSynced(id)) {
      throw "failedToFetchError";
    }
    await this.idbHelper.delete(id);
  }

  async hasTransactions(id: number): Promise<boolean> {
    return this.idbHelper.hasTransactions(id);
  }
}
