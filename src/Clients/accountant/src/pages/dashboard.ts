import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { ProgressBar } from "../../../shared/src/models/progressBar";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

import * as environment from "../../config/environment.json";
import { CapitalService } from "services/capitalService";
import { UsersService } from "services/usersService";
import { LocalStorage } from "utils/localStorage";
import { Capital } from "models/capital";
import { DashboardModel } from "models/viewmodels/dashboard";
import { AmountByCategory } from "models/viewmodels/amountByCategory";
import { SearchFilters } from "models/viewmodels/searchFilters";
import * as Actions from "utils/state/actions";
import { TransactionType } from "models/viewmodels/transactionType";
import { AppEvents } from "models/appEvents";

@inject(Router, CapitalService, UsersService, LocalStorage, I18N, EventAggregator, ConnectionTracker)
@connectTo()
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
    this.eventAggregator.subscribe(AppEvents.SyncStarted, () => {
      this.progressBar.start();
    });
    this.eventAggregator.subscribe(AppEvents.SyncFinished, () => {
      this.progressBar.finish();
      this.getCapital();
    });
  }

  activate() {
    this.imageUri = this.localStorage.getProfileImageUri();
    this.showUpcomingExpenses = this.localStorage.showUpcomingExpensesOnDashboard;
    this.showDebt = this.localStorage.showDebtOnDashboard;
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
      .get(this.i18n.tr("uncategorized"), this.showUpcomingExpenses, this.showDebt, this.currency)
      .then((capital: Capital) => {
        if (!capital) {
          return;
        }

        const model = new DashboardModel(
          capital.upcoming,
          capital.expenditures,
          capital.upcomingExpenses,
          capital.debt
        );
        this.available = capital.available;
        this.spent = capital.spent;
        this.balance = capital.balance;
        this.model = model;
      });
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

  goToTransactions(item: AmountByCategory) {
    const from = new Date();
    from.setDate(1);
    const fromDate = DateHelper.format(from);

    const toDate = DateHelper.format(new Date());

    Actions.changeFilters(new SearchFilters(1, 15, fromDate, toDate, 0, item.categoryId, TransactionType.Any, null));

    this.router.navigateToRoute("transactions");
  }

  sync() {
    if (!this.progressBar.active) {
      this.eventAggregator.publish(AppEvents.Sync);

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
