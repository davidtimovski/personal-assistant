import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { CategoriesService } from "services/categoriesService";
import { UpcomingExpensesService } from "services/upcomingExpensesService";
import { LocalStorage } from "utils/localStorage";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { SelectOption } from "models/viewmodels/selectOption";
import { EditUpcomingExpenseModel } from "models/viewmodels/editUpcomingExpenseModel";
import { CategoryType } from "models/entities/category";

@autoinject
export class EditUpcomingExpense {
  private upcomingExpenseId: number;
  private model: EditUpcomingExpenseModel;
  private categoryOptions: SelectOption[];
  private originalUpcomingExpenseJson: string;
  private isNewUpcomingExpense: boolean;
  private amountInput: HTMLInputElement;
  private amountIsInvalid: boolean;
  private dateIsInvalid: boolean;
  private saveButtonText: string;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;
  private language: string;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly upcomingExpensesService: UpcomingExpensesService,
    private readonly localStorage: LocalStorage,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.amountIsInvalid = false;
      this.dateIsInvalid = false;
    });
  }

  activate(params: any) {
    this.language = this.localStorage.getLanguage();

    this.upcomingExpenseId = parseInt(params.id, 10);
    this.isNewUpcomingExpense = this.upcomingExpenseId === 0;

    if (this.isNewUpcomingExpense) {
      const currency = this.localStorage.getCurrency();

      this.model = new EditUpcomingExpenseModel(null, null, null, currency, null, null, false, null, false);

      this.saveButtonText = this.i18n.tr("create");

      this.setValidationRules();
    } else {
      this.saveButtonText = this.i18n.tr("save");
    }
  }

  attached() {
    this.categoriesService.getAllAsOptions(this.i18n.tr("uncategorized"), CategoryType.ExpenseOnly).then((options) => {
      this.categoryOptions = options;
    });

    if (!this.isNewUpcomingExpense) {
      this.upcomingExpensesService.get(this.upcomingExpenseId).then((upcomingExpense: UpcomingExpense) => {
        if (upcomingExpense === null) {
          this.router.navigateToRoute("notFound");
        }

        this.model = new EditUpcomingExpenseModel(
          upcomingExpense.id,
          upcomingExpense.categoryId,
          upcomingExpense.amount,
          upcomingExpense.currency,
          upcomingExpense.description,
          upcomingExpense.date,
          upcomingExpense.generated,
          upcomingExpense.createdDate,
          upcomingExpense.synced
        );

        this.originalUpcomingExpenseJson = JSON.stringify(this.model);

        this.setValidationRules();
      });
    }
  }

  setValidationRules() {
    let amountTo = 8000000;
    if (this.model.currency === "MKD") {
      amountTo = 450000000;
    }
    ValidationRules.ensure((x: EditUpcomingExpenseModel) => x.amount)
      .between(0, amountTo)
      .on(this.model);
  }

  @computedFrom(
    "model.amount",
    "model.currency",
    "model.categoryId",
    "model.month",
    "model.year",
    "model.description",
    "model.synced",
    "connTracker.isOnline"
  )
  get canSave() {
    return (
      !!this.model.amount &&
      JSON.stringify(this.model) !== this.originalUpcomingExpenseJson &&
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
      if (this.isNewUpcomingExpense) {
        try {
          const upcomingExpense = new UpcomingExpense(
            null,
            this.model.categoryId,
            this.model.amount,
            this.model.currency,
            this.model.description,
            this.model.getDate(),
            false,
            null,
            null
          );
          const id = await this.upcomingExpensesService.create(upcomingExpense);
          this.amountIsInvalid = false;
          this.dateIsInvalid = false;

          this.router.navigateToRoute("upcomingExpensesEdited", {
            editedId: id,
          });
        } catch {
          this.saveButtonIsLoading = false;
        }
      } else {
        try {
          const upcomingExpense = new UpcomingExpense(
            this.model.id,
            this.model.categoryId,
            this.model.amount,
            this.model.currency,
            this.model.description,
            this.model.getDate(),
            this.model.generated,
            this.model.createdDate,
            null
          );

          await this.upcomingExpensesService.update(upcomingExpense);
          this.amountIsInvalid = false;
          this.dateIsInvalid = false;

          this.router.navigateToRoute("upcomingExpensesEdited", {
            editedId: this.upcomingExpenseId,
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
        await this.upcomingExpensesService.delete(this.model.id);

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "editUpcomingExpense.deleteSuccessful");
        this.router.navigateToRoute("upcomingExpenses");
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
      this.router.navigateToRoute("upcomingExpenses");
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
  }
}
