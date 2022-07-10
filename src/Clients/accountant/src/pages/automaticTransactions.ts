import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { AutomaticTransactionsService } from "services/automaticTransactionsService";
import { LocalStorage } from "utils/localStorage";
import { AutomaticTransactionItem } from "models/viewmodels/automaticTransactionItem";
import { AppEvents } from "models/appEvents";

@autoinject
export class AutomaticTransactions {
  private automaticTransactions: AutomaticTransactionItem[];
  private currency: string;
  private language: string;
  private lastEditedId: number;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly automaticTransactionsService: AutomaticTransactionsService,
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
    const automaticTransactions = await this.automaticTransactionsService.getAll(this.currency);

    const automaticTransactionItems = new Array<AutomaticTransactionItem>();

    for (const automaticTransaction of automaticTransactions) {
      automaticTransactionItems.push(
        new AutomaticTransactionItem(
          automaticTransaction.id,
          automaticTransaction.isDeposit,
          automaticTransaction.amount,
          automaticTransaction.currency,
          automaticTransaction.categoryName || this.i18n.tr("uncategorized"),
          this.i18n.tr(`dayOrdinal${automaticTransaction.dayInMonth}`),
          automaticTransaction.synced
        )
      );
    }

    this.automaticTransactions = automaticTransactionItems;
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

  newAutomaticTransaction() {
    if (!this.syncing) {
      this.router.navigateToRoute("editAutomaticTransaction", { id: 0 });
    }
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
