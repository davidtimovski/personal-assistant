import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { DebtsService } from "services/debtsService";
import { DebtModel } from "models/entities/debt";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { LocalStorage } from "utils/localStorage";
import { DebtItem } from "models/viewmodels/debtItem";

@inject(Router, DebtsService, I18N, EventAggregator, LocalStorage)
export class Debt {
  private debts: Array<DebtItem>;
  private currency: string;
  private lastEditedId: number;
  private emptyListMessage: string;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly debtsService: DebtsService,
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
    this.debtsService.getAll(this.currency).then((debts: Array<DebtModel>) => {
      const debtItems = new Array<DebtItem>();

      for (const debt of debts) {
        debtItems.push(
          new DebtItem(
            debt.id,
            debt.userIsDebtor,
            debt.amount,
            debt.currency,
            debt.person,
            debt.synced
          )
        );
      }

      this.debts = debtItems;

      this.emptyListMessage = this.i18n.tr("debt.emptyListMessage");
    });
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

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
