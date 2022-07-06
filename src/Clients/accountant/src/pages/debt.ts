import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { EventAggregator } from "aurelia-event-aggregator";

import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { DebtsService } from "services/debtsService";
import { LocalStorage } from "utils/localStorage";
import { DebtItem } from "models/viewmodels/debtItem";
import { AppEvents } from "models/appEvents";

@inject(Router, DebtsService, EventAggregator, LocalStorage)
export class Debt {
  private debts: DebtItem[];
  private currency: string;
  private language: string;
  private lastEditedId: number;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly debtsService: DebtsService,
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
    const debts = await this.debtsService.getAll(this.currency);

    const debtItems = new Array<DebtItem>();

    for (const debt of debts) {
      debtItems.push(
        new DebtItem(
          debt.id,
          debt.userIsDebtor,
          debt.amount,
          debt.currency,
          debt.person,
          this.formatCreatedDate(new Date(debt.createdDate)),
          debt.synced
        )
      );
    }

    this.debts = debtItems;
  }

  settleDebt(id: number, userIsDebtor: boolean) {
    const type = userIsDebtor ? 0 : 1;
    this.router.navigateToRoute("newTransaction", { type: type, debtId: id });
  }

  newDebt() {
    if (!this.syncing) {
      this.router.navigateToRoute("editDebt", { id: 0 });
    }
  }

  formatCreatedDate(date: Date): string {
    const month = DateHelper.getLongMonth(date, this.language);

    const now = new Date();
    if (now.getFullYear() === date.getFullYear()) {
      return `${month} ${date.getDate()}`;
    }

    return `${month} ${date.getDate()}, ${date.getFullYear()}`;
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
