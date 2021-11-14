import { inject } from "aurelia-framework";

import { CurrenciesService } from "../../../shared/src/services/currenciesService";

import { TransactionsIDBHelper } from "utils/transactionsIDBHelper";
import { UpcomingExpensesIDBHelper } from "utils/upcomingExpensesIDBHelper";
import { DebtsIDBHelper } from "utils/debtsIDBHelper";
import { AccountsService } from "services/accountsService";
import { Capital } from "models/capital";
import { TransactionModel } from "models/entities/transaction";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DebtModel } from "models/entities/debt";
import { TransactionsService } from "./transactionsService";
import { UpcomingExpenseDashboard } from "models/viewmodels/upcomingExpenseDashboard";
import { DebtDashboard } from "models/viewmodels/debtDashboard";

@inject(
  TransactionsIDBHelper,
  UpcomingExpensesIDBHelper,
  DebtsIDBHelper,
  TransactionsService,
  AccountsService,
  CurrenciesService
)
export class CapitalService {
  constructor(
    private readonly transactionsIDBHelper: TransactionsIDBHelper,
    private readonly upcomingExpensesIDBHelper: UpcomingExpensesIDBHelper,
    private readonly debtsIDBHelper: DebtsIDBHelper,
    private readonly transactionsService: TransactionsService,
    private readonly accountsService: AccountsService,
    private readonly currenciesService: CurrenciesService
  ) {}

  async get(
    uncategorizedLabel: string,
    includeUpcomingExpenses: boolean,
    includeDebt: boolean,
    currency: string
  ): Promise<Capital> {
    const mainAccountId = await this.accountsService.getMainId();
    if (!mainAccountId) {
      return;
    }

    const capital = new Capital(0, 0, 0, 0, null, [], []);

    const balancePromise = this.accountsService.getBalance(mainAccountId, currency).then((balance: number) => {
      capital.balance = balance;
    });

    const expendituresPromise = this.transactionsIDBHelper
      .getExpendituresForCurrentMonth(mainAccountId)
      .then(async (transactions: Array<TransactionModel>) => {
        capital.expenditures = await this.transactionsService.getByCategory(transactions, currency, uncategorizedLabel);

        capital.spent = transactions
          .filter((x) => !x.isTax)
          .map((e) => e.amount)
          .reduce((prev, curr) => prev + curr, 0);
      });

    const upcomingPromise = new Promise<void>((resolve) => {
      if (!includeUpcomingExpenses) {
        resolve();
        return;
      }

      this.upcomingExpensesIDBHelper.getAllForMonth().then((monthUpcomingExpenses: Array<UpcomingExpense>) => {
        monthUpcomingExpenses.forEach((x: UpcomingExpense) => {
          x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
          capital.upcoming += x.amount;
        });

        const upcomingExpenses = new Array<UpcomingExpenseDashboard>();
        for (const upcomingExpense of monthUpcomingExpenses) {
          const trimmedDescription = this.trimDescription(upcomingExpense.description);
          upcomingExpenses.push(
            new UpcomingExpenseDashboard(
              upcomingExpense.categoryName || uncategorizedLabel,
              trimmedDescription,
              upcomingExpense.amount
            )
          );
        }
        capital.upcomingExpenses = upcomingExpenses.sort((a: UpcomingExpenseDashboard, b: UpcomingExpenseDashboard) => {
          return b.amount - a.amount;
        });

        resolve();
      });
    });

    const debtPromise = new Promise<void>(async (resolve) => {
      if (!includeDebt) {
        resolve();
        return;
      }

      this.debtsIDBHelper.getAll().then((debt: Array<DebtModel>) => {
        debt.forEach((x: DebtModel) => {
          x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
        });

        const debtDashboard = new Array<DebtDashboard>();
        for (const debtItem of debt) {
          const trimmedDescription = this.trimDescription(debtItem.description);
          debtDashboard.push(
            new DebtDashboard(debtItem.person, debtItem.userIsDebtor, trimmedDescription, debtItem.amount)
          );
        }
        capital.debt = debtDashboard.sort((a: DebtDashboard, b: DebtDashboard) => {
          return b.amount - a.amount;
        });

        resolve();
      });
    });

    await Promise.all([balancePromise, expendituresPromise, upcomingPromise, debtPromise]);

    capital.available = capital.balance - capital.upcoming;

    return capital;
  }

  private trimDescription(description: string): string {
    if (!description) {
      return "";
    }

    const length = 25;
    if (description.length <= length) {
      return description;
    }

    return description.substring(0, length - 2) + "..";
  }
}
