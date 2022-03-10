import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { TransactionsService } from "services/transactionsService";
import { AccountsService } from "services/accountsService";
import { LocalStorage } from "utils/localStorage";
import { Adjustment } from "models/viewmodels/adjustmentModel";
import { SelectOption } from "models/viewmodels/selectOption";

@inject(Router, TransactionsService, AccountsService, LocalStorage, ValidationController, I18N, EventAggregator)
export class BalanceAdjustment {
  private model: Adjustment;
  private originalBalance: number;
  private currency: string;
  private originalAdjustmentJson: string;
  private accountOptions: Array<SelectOption>;
  private balanceIsInvalid: boolean;
  private adjustButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly transactionsService: TransactionsService,
    private readonly accountsService: AccountsService,
    private readonly localStorage: LocalStorage,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.balanceIsInvalid = false;
    });
  }

  activate() {
    this.currency = this.localStorage.getCurrency();
  }

  async attached() {
    this.accountOptions = await this.accountsService.getNonInvestmentFundsAsOptions();
    const mainAccountId = this.accountOptions[0].id;

    const balance = await this.accountsService.getBalance(mainAccountId, this.currency);
    this.originalBalance = balance;

    this.model = new Adjustment(mainAccountId, balance, this.i18n.tr("balanceAdjustment.balanceAdjustment"));

    this.originalAdjustmentJson = JSON.stringify(this.model);

    let min = 0.01;
    if (this.currency === "MKD") {
      min = 1;
    }
    ValidationRules.ensure((x: Adjustment) => Math.abs(x.balance))
      .min(min)
      .on(this.model);
  }

  async accountChanged() {
    const balance = await this.accountsService.getBalance(this.model.accountId, this.currency);

    this.originalBalance = this.model.balance = balance;
  }

  @computedFrom("model.balance")
  get adjustedBy() {
    return this.model.balance - this.originalBalance;
  }

  @computedFrom("model.balance")
  get canAdjust() {
    return !!this.model.balance && JSON.stringify(this.model) !== this.originalAdjustmentJson;
  }

  async adjust() {
    if (!this.canAdjust || this.adjustButtonIsLoading) {
      return;
    }

    this.adjustButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const result: ControllerValidateResult = await this.validationController.validate();

    if (result.valid) {
      try {
        const amount = parseFloat(<any>this.model.balance) - this.originalBalance;

        await this.transactionsService.adjust(this.model.accountId, amount, this.model.description, this.currency);
        this.balanceIsInvalid = false;

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "balanceAdjustment.adjustmentSuccessful");
        this.router.navigateToRoute("dashboard");
      } catch {
        this.adjustButtonIsLoading = false;
      }
    } else {
      this.adjustButtonIsLoading = false;
    }
  }
}
