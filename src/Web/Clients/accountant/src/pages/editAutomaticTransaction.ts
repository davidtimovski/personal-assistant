import { autoinject, computedFrom, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { CategoriesService } from "services/categoriesService";
import { AutomaticTransactionsService } from "services/automaticTransactionsService";
import { LocalStorage } from "utils/localStorage";
import { AutomaticTransaction } from "models/entities/automaticTransaction";
import { SelectOption } from "models/viewmodels/selectOption";
import { EditAutomaticTransactionModel } from "models/viewmodels/editAutomaticTransactionModel";
import { CategoryType } from "models/entities/category";

@autoinject
export class EditAutomaticTransaction {
  @observable() private isDeposit = false;

  private automaticTransactionId: number;
  private model: EditAutomaticTransactionModel;
  private categoryOptions: SelectOption[];
  private dayInMonthOptions = new Array<SelectOption>();
  private originalJson: string;
  private isNew: boolean;
  private amountInput: HTMLInputElement;
  private amountIsInvalid: boolean;
  private saveButtonText: string;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly automaticTransactionsService: AutomaticTransactionsService,
    private readonly localStorage: LocalStorage,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");

    for (let i = 1; i < 29; i++) {
      this.dayInMonthOptions.push(new SelectOption(i, this.i18n.tr(`dayOrdinal${i}`)));
    }

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.amountIsInvalid = false;
    });
  }

  activate(params: any) {
    this.automaticTransactionId = parseInt(params.id, 10);
    this.isNew = this.automaticTransactionId === 0;

    if (this.isNew) {
      const currency = this.localStorage.getCurrency();

      this.model = new EditAutomaticTransactionModel(null, false, null, null, currency, null, 1, null, false);

      this.saveButtonText = this.i18n.tr("create");
    } else {
      this.saveButtonText = this.i18n.tr("save");
    }
  }

  async attached() {
    if (!this.isNew) {
      const automaticTransaction = await this.automaticTransactionsService.get(this.automaticTransactionId);

      if (automaticTransaction === null) {
        this.router.navigateToRoute("notFound");
      }

      this.model = new EditAutomaticTransactionModel(
        automaticTransaction.id,
        automaticTransaction.isDeposit,
        automaticTransaction.categoryId,
        automaticTransaction.amount,
        automaticTransaction.currency,
        automaticTransaction.description,
        automaticTransaction.dayInMonth,
        automaticTransaction.createdDate,
        automaticTransaction.synced
      );
      this.isDeposit = this.model.isDeposit;

      this.originalJson = JSON.stringify(this.model);
    }

    this.setValidationRules();
  }

  isDepositChanged() {
    const categoryType = this.isDeposit ? CategoryType.DepositOnly : CategoryType.ExpenseOnly;

    this.categoriesService.getAllAsOptions(this.i18n.tr("uncategorized"), categoryType).then((options) => {
      this.categoryOptions = options;
    });

    if (this.model) {
      this.model.isDeposit = this.isDeposit;
    }
  }

  setValidationRules() {
    let amountTo = 8000000;
    if (this.model.currency === "MKD") {
      amountTo = 450000000;
    }
    ValidationRules.ensure((x: EditAutomaticTransactionModel) => x.amount)
      .between(0, amountTo)
      .on(this.model);
  }

  @computedFrom(
    "model.isDeposit",
    "model.amount",
    "model.currency",
    "model.categoryId",
    "model.dayInMonth",
    "model.description",
    "model.synced",
    "connTracker.isOnline"
  )
  get canSave() {
    return (
      !!this.model.amount &&
      JSON.stringify(this.model) !== this.originalJson &&
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
      if (this.isNew) {
        try {
          const automaticTransaction = new AutomaticTransaction(
            null,
            this.model.isDeposit,
            this.model.categoryId,
            this.model.amount,
            this.model.currency,
            this.model.description,
            this.model.dayInMonth,
            null,
            null
          );

          const id = await this.automaticTransactionsService.create(automaticTransaction);
          this.amountIsInvalid = false;

          this.router.navigateToRoute("automaticTransactionsEdited", {
            editedId: id,
          });
        } catch {
          this.saveButtonIsLoading = false;
        }
      } else {
        try {
          const automaticTransaction = new AutomaticTransaction(
            this.model.id,
            this.model.isDeposit,
            this.model.categoryId,
            this.model.amount,
            this.model.currency,
            this.model.description,
            this.model.dayInMonth,
            this.model.createdDate,
            null
          );

          await this.automaticTransactionsService.update(automaticTransaction);
          this.amountIsInvalid = false;

          this.router.navigateToRoute("automaticTransactionsEdited", {
            editedId: this.automaticTransactionId,
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
        await this.automaticTransactionsService.delete(this.model.id);

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "editAutomaticTransaction.deleteSuccessful");
        this.router.navigateToRoute("automaticTransactions");
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
      this.router.navigateToRoute("automaticTransactions");
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
  }
}
