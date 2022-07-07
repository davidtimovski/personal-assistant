import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { CategoriesService } from "services/categoriesService";
import { Category, CategoryType } from "models/entities/category";
import { SelectOption } from "models/viewmodels/selectOption";

@autoinject
export class EditCategory {
  private categoryId: number;
  private category: Category;
  private originalCategoryJson: string;
  private isNewCategory: boolean;
  private isParent: boolean;
  private parentCategoryOptions: SelectOption[];
  private typeOptions: SelectOption[];
  private nameInput: HTMLInputElement;
  private nameIsInvalid: boolean;
  private saveButtonText: string;
  private transactionsWarningVisible = false;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.nameIsInvalid = false;
    });

    this.typeOptions = [
      new SelectOption(CategoryType.AllTransactions, this.i18n.tr("editCategory.allTransactions")),
      new SelectOption(CategoryType.DepositOnly, this.i18n.tr("editCategory.depositOnly")),
      new SelectOption(CategoryType.ExpenseOnly, this.i18n.tr("editCategory.expenseOnly")),
    ];
  }

  activate(params: any) {
    this.categoryId = parseInt(params.id, 10);
    this.isNewCategory = this.categoryId === 0;

    if (this.isNewCategory) {
      this.category = new Category(0, null, "", CategoryType.AllTransactions, false, false, null, null);
      this.saveButtonText = this.i18n.tr("create");

      this.setValidationRules();
    } else {
      this.saveButtonText = this.i18n.tr("save");
    }
  }

  attached() {
    if (this.isNewCategory) {
      this.nameInput.focus();
    } else {
      this.categoriesService.isParent(this.categoryId).then((isParent: boolean) => {
        this.isParent = isParent;
      });

      this.categoriesService.get(this.categoryId).then((category: Category) => {
        if (category === null) {
          this.router.navigateToRoute("notFound");
        }
        this.category = category;

        this.originalCategoryJson = JSON.stringify(this.category);

        this.setValidationRules();
      });
    }

    this.categoriesService.getParentAsOptions(this.i18n.tr("editCategory.none"), this.categoryId).then((options) => {
      this.parentCategoryOptions = options;
    });
  }

  setValidationRules() {
    ValidationRules.ensure((x: Category) => x.name)
      .required()
      .on(this.category);
  }

  typeChanged() {
    if (this.category.type !== CategoryType.ExpenseOnly) {
      this.category.generateUpcomingExpense = false;
      this.category.isTax = false;
    }
  }

  @computedFrom(
    "category.name",
    "category.parentId",
    "category.type",
    "category.generateUpcomingExpense",
    "category.isTax",
    "category.synced",
    "connTracker.isOnline"
  )
  get canSave() {
    return (
      !ValidationUtil.isEmptyOrWhitespace(this.category.name) &&
      JSON.stringify(this.category) !== this.originalCategoryJson &&
      !(!this.connTracker.isOnline && this.category.synced)
    );
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const result: ControllerValidateResult = await this.validationController.validate();

    this.nameIsInvalid = !result.valid;

    if (result.valid) {
      if (this.isNewCategory) {
        try {
          const id = await this.categoriesService.create(this.category);
          this.nameIsInvalid = false;

          this.router.navigateToRoute("categoriesEdited", {
            editedId: id,
          });
        } catch {
          this.saveButtonIsLoading = false;
        }
      } else {
        try {
          await this.categoriesService.update(this.category);
          this.nameIsInvalid = false;
          this.router.navigateToRoute("categoriesEdited", {
            editedId: this.category.id,
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
        await this.categoriesService.delete(this.category.id);
        this.eventAggregator.publish(AlertEvents.ShowSuccess, "editCategory.deleteSuccessful");
        this.router.navigateToRoute("categories");
      } catch (e) {
        this.eventAggregator.publish(AlertEvents.ShowError, e);

        this.deleteButtonText = this.i18n.tr("delete");
        this.deleteInProgress = false;
        this.deleteButtonIsLoading = false;
        return;
      }
    } else {
      if (await this.categoriesService.hasTransactions(this.category.id)) {
        this.transactionsWarningVisible = true;
        this.deleteButtonText = this.i18n.tr("editCategory.okayDelete");
      } else {
        this.deleteButtonText = this.i18n.tr("sure");
      }

      this.deleteInProgress = true;
    }
  }

  cancel() {
    if (!this.deleteInProgress) {
      this.router.navigateToRoute("categories");
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
    this.transactionsWarningVisible = false;
  }
}
