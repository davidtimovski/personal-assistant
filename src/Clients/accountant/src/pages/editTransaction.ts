import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { DateHelper } from "../../../shared/src/utils/dateHelper";
import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { CategoriesService } from "services/categoriesService";
import { TransactionsService } from "services/transactionsService";
import { EncryptionService } from "services/encryptionService";
import { TransactionModel } from "models/entities/transaction";
import { SelectOption } from "models/viewmodels/selectOption";
import { EditTransactionModel } from "models/viewmodels/editTransactionModel";
import { CategoryType } from "models/entities/category";

@inject(
  Router,
  CategoriesService,
  TransactionsService,
  EncryptionService,
  ValidationController,
  I18N,
  EventAggregator,
  ConnectionTracker
)
export class EditTransaction {
  private transactionId: number;
  private model: EditTransactionModel;
  private categoryOptions: Array<SelectOption>;
  private originalTransactionJson: string;
  private readonly maxDate: string;
  private decPasswordInput: HTMLInputElement;
  private decPasswordShown = false;
  private encPasswordInput: HTMLInputElement;
  private encPasswordShown = false;
  private amountIsInvalid: boolean;
  private dateIsInvalid: boolean;
  private decryptionPasswordIsInvalid: boolean;
  private encryptionPasswordIsInvalid: boolean;
  private decryptButtonIsLoading = false;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;
  private passwordShowIconLabel: string;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly transactionsService: TransactionsService,
    private readonly encryptionService: EncryptionService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");

    this.maxDate = DateHelper.format(new Date());

    this.passwordShowIconLabel = this.i18n.tr("showPassword");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.amountIsInvalid = false;
      this.dateIsInvalid = false;
    });
  }

  activate(params: any) {
    this.transactionId = parseInt(params.id, 10);
  }

  attached() {
    this.categoriesService
      .getAllAsOptions(this.i18n.tr("uncategorized"), CategoryType.AllTransactions)
      .then((options) => {
        this.categoryOptions = options;
      });

    this.transactionsService.get(this.transactionId).then(async (transaction: TransactionModel) => {
      if (transaction === null) {
        this.router.navigateToRoute("notFound");
      }

      const model = new EditTransactionModel(
        transaction.id,
        transaction.fromAccountId,
        transaction.toAccountId,
        transaction.categoryId,
        transaction.amount,
        transaction.fromStocks,
        transaction.toStocks,
        transaction.currency,
        transaction.description,
        transaction.date.slice(0, 10),
        transaction.isEncrypted,
        transaction.encryptedDescription,
        transaction.salt,
        transaction.nonce,
        null,
        transaction.isEncrypted,
        null,
        transaction.createdDate,
        transaction.synced
      );

      this.model = model;

      this.originalTransactionJson = JSON.stringify(this.model);

      let amountFrom = 0.01;
      let amountTo = 8000001;
      if (this.model.currency === "MKD") {
        amountFrom = 0;
        amountTo = 450000001;
      }
      ValidationRules.ensure((x: EditTransactionModel) => x.amount)
        .between(amountFrom, amountTo)
        .ensure((x: EditTransactionModel) => x.date)
        .required()
        .withMessage(this.i18n.tr("newTransaction.dateIsRequired"))
        .ensure((x: EditTransactionModel) => x.encryptionPassword)
        .satisfies((value: string, model: EditTransactionModel) => {
          return !model.encrypt || !ValidationUtil.isEmptyOrWhitespace(value);
        })
        .withMessage(this.i18n.tr("newTransaction.passwordIsRequired"))
        .on(this.model);
    });
  }

  @computedFrom("model.description")
  get canEncrypt() {
    const canEncrypt = this.model.description?.trim().length > 0;
    if (!canEncrypt) {
      this.model.encrypt = false;
      this.model.decryptionPassword = null;
    }
    return canEncrypt;
  }

  toggleDecPasswordShow() {
    if (this.decPasswordShown) {
      this.decPasswordInput.type = "password";
      this.passwordShowIconLabel = this.i18n.tr("showPassword");
    } else {
      this.decPasswordInput.type = "text";
      this.passwordShowIconLabel = this.i18n.tr("hidePassword");
    }

    this.decPasswordShown = !this.decPasswordShown;
  }

  toggleEncPasswordShow() {
    if (this.encPasswordShown) {
      this.encPasswordInput.type = "password";
      this.passwordShowIconLabel = this.i18n.tr("showPassword");
    } else {
      this.encPasswordInput.type = "text";
      this.passwordShowIconLabel = this.i18n.tr("hidePassword");
    }

    this.encPasswordShown = !this.encPasswordShown;
  }

  async decrypt() {
    if (ValidationUtil.isEmptyOrWhitespace(this.model.decryptionPassword)) {
      this.decryptionPasswordIsInvalid = true;
      return;
    }

    this.decryptButtonIsLoading = true;

    try {
      const decryptedDescription = await this.encryptionService.decrypt(
        this.model.encryptedDescription,
        this.model.salt,
        this.model.nonce,
        this.model.decryptionPassword
      );

      this.model.description = decryptedDescription;
      this.model.isEncrypted = false;
      this.model.encryptionPassword = this.model.decryptionPassword;
      this.model.decryptionPassword = null;
      this.decryptButtonIsLoading = false;
    } catch {
      this.decryptionPasswordIsInvalid = true;
      this.decryptButtonIsLoading = false;
    }
  }

  erase() {
    this.model.isEncrypted = false;
    this.model.decryptionPassword = null;
  }

  @computedFrom(
    "model.amount",
    "model.fromStocks",
    "model.toStocks",
    "model.currency",
    "model.categoryId",
    "model.date",
    "model.description",
    "model.isEncrypted",
    "model.encrypt",
    "model.synced",
    "connTracker.isOnline"
  )
  get canSave() {
    return (
      !!this.model.amount &&
      JSON.stringify(this.model) !== this.originalTransactionJson &&
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
      try {
        const transaction = new TransactionModel(
          this.model.id,
          this.model.fromAccountId,
          this.model.toAccountId,
          this.model.categoryId,
          this.model.amount,
          this.model.fromStocks,
          this.model.toStocks,
          this.model.currency,
          this.model.description,
          this.model.date,
          this.model.encrypt,
          this.model.encryptedDescription,
          this.model.salt,
          this.model.nonce,
          this.model.createdDate,
          null
        );

        await this.transactionsService.update(transaction, this.model.encryptionPassword);
        this.amountIsInvalid = false;
        this.dateIsInvalid = false;

        this.router.navigateToRoute("transactionsEdited", {
          editedId: this.transactionId,
        });
      } catch {
        this.saveButtonIsLoading = false;
      }
    } else {
      const messages = new Array<string>();

      const invalidDate = result.results.find((p) => {
        return p.propertyName === "date" && !p.valid;
      });
      if (invalidDate) {
        messages.push(invalidDate.message);
      }

      const invalidEncryptionPassword = result.results.find((p) => {
        return p.propertyName === "encryptionPassword" && !p.valid;
      });
      if (invalidEncryptionPassword) {
        messages.push(invalidEncryptionPassword.message);
      }

      if (messages.length > 0) {
        this.dateIsInvalid = !!invalidDate;
        this.encryptionPasswordIsInvalid = !!invalidEncryptionPassword;

        this.eventAggregator.publish(AlertEvents.ShowError, messages);
      }

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
        await this.transactionsService.delete(this.model.id);
        this.eventAggregator.publish(AlertEvents.ShowSuccess, "editTransaction.deleteSuccessful");
        this.router.navigateToRoute("transactions");
      } catch (e) {
        this.eventAggregator.publish(AlertEvents.ShowError, e);

        this.deleteButtonText = this.i18n.tr("delete");
        this.deleteInProgress = false;
        this.deleteButtonIsLoading = false;
        return;
      }
    } else {
      this.deleteButtonText = this.i18n.tr("sure");
      this.deleteInProgress = true;
    }
  }

  cancel() {
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
  }
}
