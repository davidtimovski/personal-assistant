import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";

import { DateHelper } from "../../../shared/src/utils/dateHelper";
import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { CategoriesService } from "services/categoriesService";
import { TransactionsService } from "services/transactionsService";
import { AccountsService } from "services/accountsService";
import { DebtsService } from "services/debtsService";
import { LocalStorage } from "utils/localStorage";
import { TransactionModel } from "models/entities/transaction";
import { SelectOption } from "models/viewmodels/selectOption";
import { NewTransactionModel } from "models/viewmodels/newTransactionModel";
import { CategoryType } from "models/entities/category";
import { DebtModel } from "models/entities/debt";

@inject(
  Router,
  CategoriesService,
  TransactionsService,
  AccountsService,
  DebtsService,
  LocalStorage,
  ValidationController,
  I18N,
  EventAggregator
)
export class NewTransaction {
  private model = new NewTransactionModel(null, null, null, null, null, null, false, null);
  private isExpense: boolean;
  private debtId: number;
  private userIsDebtor: boolean;
  private debtPerson: string;
  private categoryOptions: Array<SelectOption>;
  private originalTransactionJson: string;
  private readonly maxDate: string;
  private readonly encryptedDescriptionTooltipKey = "encryptedDescription";
  private passwordInput: HTMLInputElement;
  private passwordShown = false;
  private amountIsInvalid: boolean;
  private dateIsInvalid: boolean;
  private encryptionPasswordIsInvalid: boolean;
  private submitButtonIsLoading = false;
  private passwordShowIconLabel: string;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly transactionsService: TransactionsService,
    private readonly accountsService: AccountsService,
    private readonly debtsService: DebtsService,
    private readonly localStorage: LocalStorage,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;

    this.maxDate = this.model.date = DateHelper.format(new Date());

    this.passwordShowIconLabel = this.i18n.tr("showPassword");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.amountIsInvalid = false;
      this.dateIsInvalid = false;
      this.encryptionPasswordIsInvalid = false;
    });
  }

  activate(params: any) {
    this.isExpense = params.type === "0";

    if (params.debtId) {
      this.debtId = parseInt(params.debtId, 10);
    }

    this.model.currency = this.localStorage.getCurrency();

    let amountFrom = 0.01;
    let amountTo = 8000001;
    if (this.model.currency === "MKD") {
      amountFrom = 0;
      amountTo = 450000001;
    }
    ValidationRules.ensure((x: NewTransactionModel) => x.amount)
      .between(amountFrom, amountTo)
      .withMessage(this.i18n.tr("amountBetween", { from: amountFrom, to: amountTo }))
      .ensure((x: NewTransactionModel) => x.date)
      .required()
      .withMessage(this.i18n.tr("newTransaction.dateIsRequired"))
      .ensure((x: NewTransactionModel) => x.encryptionPassword)
      .satisfies((value: string, model: NewTransactionModel) => {
        return !model.encrypt || !ValidationUtil.isEmptyOrWhitespace(value);
      })
      .withMessage(this.i18n.tr("newTransaction.passwordIsRequired"))
      .on(this.model);
  }

  attached() {
    const categoryType = this.isExpense ? CategoryType.ExpenseOnly : CategoryType.DepositOnly;

    this.accountsService.getMainId().then((id: number) => {
      this.model.mainAccountId = id;
    });

    this.categoriesService.getAllAsOptions(this.i18n.tr("uncategorized"), categoryType).then((options) => {
      this.categoryOptions = options;
    });

    if (this.debtId) {
      this.debtsService.get(this.debtId).then((debt: DebtModel) => {
        this.userIsDebtor = debt.userIsDebtor;
        this.debtPerson = debt.person;
        this.model.amount = debt.amount;
      });
    }
  }

  @computedFrom("model.description")
  get canEncrypt() {
    const canEncrypt = this.model.description?.trim().length > 0;
    if (!canEncrypt) {
      this.model.encrypt = false;
      this.model.encryptionPassword = null;
    }
    return canEncrypt;
  }

  togglePasswordShow() {
    if (this.passwordShown) {
      this.passwordInput.type = "password";
      this.passwordShowIconLabel = this.i18n.tr("showPassword");
    } else {
      this.passwordInput.type = "text";
      this.passwordShowIconLabel = this.i18n.tr("hidePassword");
    }

    this.passwordShown = !this.passwordShown;
  }

  @computedFrom("model.amount", "model.categoryId", "model.date", "model.description")
  get canSubmit() {
    return !!this.model.amount && JSON.stringify(this.model) !== this.originalTransactionJson;
  }

  async submit() {
    if (!this.canSubmit || this.submitButtonIsLoading) {
      return;
    }

    this.submitButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    if (!this.model.mainAccountId) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      return;
    }

    const result: ControllerValidateResult = await this.validationController.validate();
    if (result.valid) {
      try {
        const transaction = new TransactionModel(
          null,
          null,
          null,
          this.model.categoryId,
          this.model.amount,
          null,
          null,
          this.model.currency,
          this.model.description,
          this.model.date,
          this.model.encrypt,
          null,
          null,
          null,
          null,
          null
        );

        if (this.isExpense) {
          transaction.fromAccountId = this.model.mainAccountId;
        } else {
          transaction.toAccountId = this.model.mainAccountId;
        }

        await this.transactionsService.create(transaction, this.model.encryptionPassword);

        if (this.debtId) {
          await this.debtsService.delete(this.debtId);
        }

        this.amountIsInvalid = false;
        this.dateIsInvalid = false;

        if (this.debtId) {
          this.eventAggregator.publish(AlertEvents.ShowSuccess, "newTransaction.debtSettlingSuccessful");
        } else {
          const messageKey = this.isExpense ? "newTransaction.expenseSubmitted" : "newTransaction.depositSubmitted";

          this.eventAggregator.publish(AlertEvents.ShowSuccess, messageKey);
        }
        this.router.navigateToRoute("dashboard");
      } catch {
        this.submitButtonIsLoading = false;
      }
    } else {
      const messages = new Array<string>();

      const invalidAmount = result.results.find((p) => {
        return p.propertyName === "amount" && !p.valid;
      });
      if (invalidAmount) {
        messages.push(invalidAmount.message);
      }

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
        this.amountIsInvalid = !!invalidAmount;
        this.dateIsInvalid = !!invalidDate;
        this.encryptionPasswordIsInvalid = !!invalidEncryptionPassword;

        this.eventAggregator.publish(AlertEvents.ShowError, messages);
      }

      this.submitButtonIsLoading = false;
    }
  }

  back() {
    if (this.debtId) {
      this.router.navigateToRoute("debt");
    } else {
      this.router.navigateToRoute("dashboard");
    }
  }
}
