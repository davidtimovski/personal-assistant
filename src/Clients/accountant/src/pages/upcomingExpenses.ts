import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { UpcomingExpensesService } from "services/upcomingExpensesService";
import { LocalStorage } from "utils/localStorage";
import { UpcomingExpenseItem } from "models/viewmodels/upcomingExpenseItem";
import { AppEvents } from "models/appEvents";

@autoinject
export class UpcomingExpenses {
  private upcomingExpenses: UpcomingExpenseItem[];
  private currency: string;
  private language: string;
  private lastEditedId: number;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly upcomingExpensesService: UpcomingExpensesService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly localStorage: LocalStorage
  ) {
    this.eventAggregator.subscribe(AppEvents.SyncStarted, () => {
      this.syncing = true;
    });
    this.eventAggregator.subscribe(AppEvents.SyncFinished, () => {
      this.syncing = false;
    });
  }

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }

    this.currency = this.localStorage.getCurrency();
    this.language = this.localStorage.getLanguage();
  }

  async attached() {
    const upcomingExpenses = await this.upcomingExpensesService.getAll(this.currency);

    const upcomingExpenseItems = new Array<UpcomingExpenseItem>();

    for (const upcomingExpense of upcomingExpenses) {
      upcomingExpenseItems.push(
        new UpcomingExpenseItem(
          upcomingExpense.id,
          upcomingExpense.amount,
          upcomingExpense.categoryName || this.i18n.tr("uncategorized"),
          this.formatDate(upcomingExpense.date),
          upcomingExpense.synced
        )
      );
    }

    this.upcomingExpenses = upcomingExpenseItems;
  }

  formatDate(dateString: string): string {
    const date = new Date(Date.parse(dateString));
    const month = DateHelper.getLongMonth(date, this.language);

    const now = new Date();
    if (now.getFullYear() === date.getFullYear()) {
      return month;
    }

    return `${month} ${date.getFullYear()}`;
  }

  newUpcomingExpense() {
    if (!this.syncing) {
      this.router.navigateToRoute("editUpcomingExpense", { id: 0 });
    }
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
