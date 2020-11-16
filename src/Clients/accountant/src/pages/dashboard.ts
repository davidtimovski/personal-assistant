import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { CapitalService } from "services/capitalService";
import { UsersService } from "services/usersService";
import { LocalStorage } from "utils/localStorage";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { Capital } from "models/capital";
import { ProgressBar } from "../../../shared/src/models/progressBar";
import * as environment from "../../config/environment.json";
import { DashboardModel } from "models/viewmodels/dashboard";
import { TransactionModel } from "models/entities/transaction";
import { ExpenditureByCategory } from "models/viewmodels/expenditureByCategory";
import { UpcomingExpenseDashboard } from "models/viewmodels/upcomingExpenseDashboard";
import { DebtDashboard } from "models/viewmodels/debtDashboard";

@inject(
  Router,
  CapitalService,
  UsersService,
  LocalStorage,
  I18N,
  EventAggregator,
  ConnectionTracker
)
export class Dashboard {
  private imageUri = JSON.parse(<any>environment).defaultProfileImageUri;
  private progressBar = new ProgressBar();
  private model: DashboardModel;
  private available = 0;
  private spent = 0;
  private balance = 0;
  private showUpcomingExpenses = false;
  private showDebt = false;
  private currency: string;
  private menuButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly capitalService: CapitalService,
    private readonly usersService: UsersService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.eventAggregator.subscribe("sync-started", () => {
      this.progressBar.start();
    });
    this.eventAggregator.subscribe("sync-finished", () => {
      this.progressBar.finish();
      this.getCapital();
    });
  }

  activate() {
    this.imageUri = this.localStorage.getProfileImageUri();
    this.showUpcomingExpenses = this.localStorage.getShowUpcomingExpensesOnDashboard();
    this.showDebt = this.localStorage.getShowDebtOnDashboard();
    this.currency = this.localStorage.getCurrency();
  }

  attached() {
    this.getCapital();

    if (this.localStorage.isStale("profileImageUri")) {
      this.usersService.getProfileImageUri().then((imageUri) => {
        if (this.imageUri !== imageUri) {
          this.imageUri = imageUri;
        }
      });
    }
  }

  getCapital() {
    this.capitalService
      .get(this.showUpcomingExpenses, this.showDebt, this.currency)
      .then((capital: Capital) => {
        if (!capital) {
          return;
        }

        const model = new DashboardModel(capital.upcoming, [], [], []);

        const expenditures = new Array<ExpenditureByCategory>();
        const expenditureGroups = this.groupBy(
          capital.transactions,
          (x: TransactionModel) => x.categoryName
        );
        for (const group of expenditureGroups) {
          const expenditure = new ExpenditureByCategory(
            group[0] || this.i18n.tr("uncategorized"),
            0
          );

          for (const transaction of group[1]) {
            expenditure.amount += transaction.amount;
          }

          expenditures.push(expenditure);
        }
        model.expenditures = expenditures.sort(
          (a: ExpenditureByCategory, b: ExpenditureByCategory) => {
            return b.amount - a.amount;
          }
        );

        const upcomingExpenses = new Array<UpcomingExpenseDashboard>();
        for (const upcomingExpense of capital.upcomingExpenses) {
          const trimmedDescription = this.trimDescription(
            upcomingExpense.description
          );
          upcomingExpenses.push(
            new UpcomingExpenseDashboard(
              upcomingExpense.categoryName || this.i18n.tr("uncategorized"),
              trimmedDescription,
              upcomingExpense.amount
            )
          );
        }
        model.upcomingExpenses = upcomingExpenses.sort(
          (a: UpcomingExpenseDashboard, b: UpcomingExpenseDashboard) => {
            return b.amount - a.amount;
          }
        );

        const debt = new Array<DebtDashboard>();
        for (const debtItem of capital.debt) {
          const trimmedDescription = this.trimDescription(debtItem.description);
          debt.push(
            new DebtDashboard(
              debtItem.person,
              debtItem.userIsDebtor,
              trimmedDescription,
              debtItem.amount
            )
          );
        }
        model.debt = debt.sort((a: DebtDashboard, b: DebtDashboard) => {
          return b.amount - a.amount;
        });

        this.available = capital.available;
        this.spent = capital.spent;
        this.balance = capital.balance;
        this.model = model;
      });
  }

  groupBy(
    list: Array<TransactionModel>,
    keyGetter: { (x: TransactionModel): string; (arg0: TransactionModel): any }
  ) {
    const map = new Map();
    list.forEach((item) => {
      const key = keyGetter(item);
      const collection = map.get(key);
      if (!collection) {
        map.set(key, [item]);
      } else {
        collection.push(item);
      }
    });
    return map;
  }

  trimDescription(description: string): string {
    if (!description) {
      return "";
    }

    const length = 25;
    if (description.length <= length) {
      return description;
    }

    return description.substring(0, length - 2) + "..";
  }

  newExpense() {
    if (!this.progressBar.active) {
      this.router.navigateToRoute("newTransaction", { type: 0 });
    }
  }

  newDeposit() {
    if (!this.progressBar.active) {
      this.router.navigateToRoute("newTransaction", { type: 1 });
    }
  }

  sync() {
    if (!this.progressBar.active) {
      this.eventAggregator.publish("sync");

      this.usersService.getProfileImageUri().then((imageUri) => {
        if (this.imageUri !== imageUri) {
          this.imageUri = imageUri;
        }
      });
    }
  }

  @computedFrom("progressBar.progress")
  get getProgress() {
    if (this.progressBar.progress === 100) {
      this.progressBar.visible = false;
    }
    return this.progressBar.progress;
  }

  goToMenu() {
    this.menuButtonIsLoading = true;
    this.router.navigateToRoute("menu");
  }

  detached() {
    window.clearInterval(this.progressBar.intervalId);
  }
}
