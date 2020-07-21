import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { UpcomingExpensesService } from "services/upcomingExpensesService";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { LocalStorage } from "utils/localStorage";
import { UpcomingExpenseItem } from "models/viewmodels/upcomingExpenseItem";

@inject(Router, UpcomingExpensesService, I18N, EventAggregator, LocalStorage)
export class UpcomingExpenses {
  private upcomingExpenses: Array<UpcomingExpenseItem>;
  private currency: string;
  private lastEditedId: number;
  private emptyListMessage: string;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly upcomingExpensesService: UpcomingExpensesService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly localStorage: LocalStorage
  ) {
    this.eventAggregator.subscribe("sync-started", () => {
      this.syncing = true;
    });
    this.eventAggregator.subscribe("sync-finished", () => {
      this.syncing = false;
    });
  }

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }

    this.currency = this.localStorage.getCurrency();
  }

  attached() {
    this.upcomingExpensesService
      .getAll(this.currency)
      .then((upcomingExpenses: Array<UpcomingExpense>) => {
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

        this.emptyListMessage = this.i18n.tr(
          "upcomingExpenses.emptyListMessage"
        );
      });
  }

  formatDate(dateString: string): string {
    const date = new Date(Date.parse(dateString));
    const month = this.i18n.tr(`months.${date.getMonth()}`);

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
