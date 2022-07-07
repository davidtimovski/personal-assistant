import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { DebtsService } from "services/debtsService";
import { LocalStorage } from "utils/localStorage";
import { DebtModel } from "models/entities/debt";
import { EditDebtModel } from "models/viewmodels/editDebtModel";

@autoinject
export class EditDebt {
  private debtId: number;
  private model = new EditDebtModel(null, null, null, null, null, false, null, false);
  private originalDebtJson: string;
  private isNewDebt: boolean;
  private personInput: HTMLInputElement;
  private personIsInvalid: boolean;
  private amountIsInvalid: boolean;
  private saveButtonText: string;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly debtsService: DebtsService,
    private readonly localStorage: LocalStorage,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.personIsInvalid = false;
      this.amountIsInvalid = false;
    });
  }

  activate(params: any) {
    this.debtId = parseInt(params.id, 10);
    this.isNewDebt = this.debtId === 0;

    if (this.isNewDebt) {
      this.model.currency = this.localStorage.getCurrency();

      this.saveButtonText = this.i18n.tr("create");

      this.setValidationRules();
    } else {
      this.saveButtonText = this.i18n.tr("save");
    }
  }

  async attached() {
    if (this.isNewDebt) {
      this.personInput.focus();
    } else {
      const debt = await this.debtsService.get(this.debtId);
      if (debt === null) {
        this.router.navigateToRoute("notFound");
      }

      this.model = new EditDebtModel(
        debt.id,
        debt.person,
        debt.amount,
        debt.currency,
        debt.description,
        debt.userIsDebtor,
        debt.createdDate,
        debt.synced
      );

      this.originalDebtJson = JSON.stringify(this.model);

      this.setValidationRules();
    }
  }

  setValidationRules() {
    let amountTo = 8000000;
    if (this.model.currency === "MKD") {
      amountTo = 450000000;
    }
    ValidationRules.ensure((x: EditDebtModel) => x.amount)
      .between(0, amountTo)
      .on(this.model);
  }

  @computedFrom(
    "model.person",
    "model.amount",
    "model.currency",
    "model.userIsDebtor",
    "model.description",
    "model.synced",
    "connTracker.isOnline"
  )
  get canSave() {
    return (
      !ValidationUtil.isEmptyOrWhitespace(this.model.person) &&
      !!this.model.amount &&
      JSON.stringify(this.model) !== this.originalDebtJson &&
      !(!this.connTracker.isOnline && this.model.synced)
    );
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const result: ControllerValidateResult = await this.validationController.validate();

    if (result.valid) {
      if (this.isNewDebt) {
        try {
          const debt = new DebtModel(
            null,
            this.model.person,
            this.model.amount,
            this.model.currency,
            this.model.description,
            this.model.userIsDebtor,
            null,
            null
          );
          const id = await this.debtsService.createOrMerge(debt, this.localStorage.mergeDebtPerPerson);
          this.personIsInvalid = false;
          this.amountIsInvalid = false;

          this.router.navigateToRoute("debtEdited", {
            editedId: id,
          });
        } catch {
          this.saveButtonIsLoading = false;
        }
      } else {
        try {
          const debt = new DebtModel(
            this.model.id,
            this.model.person,
            this.model.amount,
            this.model.currency,
            this.model.description,
            this.model.userIsDebtor,
            this.model.createdDate,
            null
          );

          await this.debtsService.update(debt);
          this.personIsInvalid = false;
          this.amountIsInvalid = false;

          this.router.navigateToRoute("debtEdited", {
            editedId: this.debtId,
          });
        } catch {
          this.saveButtonIsLoading = false;
        }
      }
    } else {
      this.saveButtonIsLoading = false;
    }
  }

  async delete() {
    if (this.deleteButtonIsLoading) {
      return;
    }

    if (this.deleteInProgress) {
      this.deleteButtonIsLoading = true;

      try {
        await this.debtsService.delete(this.model.id);

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "editDebt.deleteSuccessful");
        this.router.navigateToRoute("debt");
      } catch {
        this.deleteButtonText = this.i18n.tr("delete");
        this.deleteInProgress = false;
        this.deleteButtonIsLoading = false;
      }
    } else {
      this.deleteButtonText = this.i18n.tr("sure");
      this.deleteInProgress = true;
    }
  }

  cancel() {
    if (!this.deleteInProgress) {
      this.router.navigateToRoute("debt");
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
  }
}
