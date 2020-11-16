import { inject } from "aurelia-framework";
import { TransactionsIDBHelper } from "utils/transactionsIDBHelper";
import { UpcomingExpensesIDBHelper } from "utils/upcomingExpensesIDBHelper";
import { DebtsIDBHelper } from "utils/debtsIDBHelper";
import { AccountsService } from "services/accountsService";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { Capital } from "models/capital";
import { TransactionModel } from "models/entities/transaction";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DebtModel } from "models/entities/debt";

@inject(
  TransactionsIDBHelper,
  UpcomingExpensesIDBHelper,
  DebtsIDBHelper,
  AccountsService,
  CurrenciesService
)
export class CapitalService {
  constructor(
    private readonly transactionsIDBHelper: TransactionsIDBHelper,
    private readonly upcomingExpensesIDBHelper: UpcomingExpensesIDBHelper,
    private readonly debtsIDBHelper: DebtsIDBHelper,
    private readonly accountsService: AccountsService,
    private readonly currenciesService: CurrenciesService
  ) {}

  async get(
    includeUpcomingExpenses: boolean,
    includeDebt: boolean,
    currency: string
  ): Promise<Capital> {
    const mainAccountId = await this.accountsService.getMainId();
    if (!mainAccountId) {
      return;
    }

    const capital = new Capital(0, 0, 0, 0, null, [], []);

    const balancePromise = this.accountsService
      .getBalance(mainAccountId, currency)
      .then((balance: number) => {
        capital.balance = balance;
      });

    const expendituresPromise = this.transactionsIDBHelper
      .getExpendituresForCurrentMonth(mainAccountId)
      .then((monthTransactions: Array<TransactionModel>) => {
        capital.transactions = monthTransactions;

        capital.transactions.forEach((x: TransactionModel) => {
          x.amount = this.currenciesService.convert(
            x.amount,
            x.currency,
            currency
          );
          capital.spent += x.amount;
        });
      });

    const upcomingPromise = new Promise((resolve) => {
      if (!includeUpcomingExpenses) {
        resolve();
        return;
      }

      this.upcomingExpensesIDBHelper
        .getAllForMonth()
        .then((monthUpcomingExpenses: Array<UpcomingExpense>) => {
          capital.upcomingExpenses = monthUpcomingExpenses;

          capital.upcomingExpenses.forEach((x: UpcomingExpense) => {
            x.amount = this.currenciesService.convert(
              x.amount,
              x.currency,
              currency
            );
            capital.upcoming += x.amount;
          });

          resolve();
        });
    });

    const debtPromise = new Promise(async (resolve) => {
      if (!includeDebt) {
        resolve();
        return;
      }

      this.debtsIDBHelper.getAll().then((debt: Array<DebtModel>) => {
        capital.debt = debt;

        capital.debt.forEach((x: DebtModel) => {
          x.amount = this.currenciesService.convert(
            x.amount,
            x.currency,
            currency
          );
        });

        resolve();
      });
    });

    await Promise.all([
      balancePromise,
      expendituresPromise,
      upcomingPromise,
      debtPromise,
    ]);

    capital.available = capital.balance - capital.upcoming;

    return capital;
  }
}
