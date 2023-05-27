import { autoinject, computedFrom, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { AccountsService } from "services/accountsService";
import { LocalStorage } from "utils/localStorage";
import { Account } from "models/entities/account";

@autoinject
export class EditAccount {
  private accountId: number;
  private account: Account;
  private originalJson: string;
  private currency: string;
  private isNew: boolean;
  private isMainAccount: boolean;
  private nameInput: HTMLInputElement;
  private nameIsInvalid: boolean;
  private saveButtonText: string;
  private transactionsWarningVisible = false;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;

  @observable()
  private investmentFund: boolean;

  constructor(
    private readonly router: Router,
    private readonly accountsService: AccountsService,
    private readonly localStorage: LocalStorage,
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
  }

  activate(params: any) {
    this.accountId = parseInt(params.id, 10);
    this.isNew = this.accountId === 0;

    this.currency = this.localStorage.getCurrency();

    if (this.isNew) {
      this.account = new Account("", this.currency, null, null);
      this.investmentFund = false;
      this.saveButtonText = this.i18n.tr("create");
    } else {
      this.saveButtonText = this.i18n.tr("save");
    }
  }

  async attached() {
    if (this.isNew) {
      this.nameInput.focus();
    } else {
      const mainId = await this.accountsService.getMainId();
      this.isMainAccount = this.accountId === mainId;

      const account = await this.accountsService.get(this.accountId);
      if (account === null) {
        this.router.navigateToRoute("notFound");
      }

      this.account = account;
      this.investmentFund = !!account.stockPrice;

      this.originalJson = JSON.stringify(this.account);
    }

    this.setValidationRules();
  }

  setValidationRules() {
    ValidationRules.ensure((x: Account) => x.name)
      .required()
      .on(this.account);
  }

  investmentFundChanged() {
    if (this.investmentFund) {
      if (!this.account.stockPrice) {
        this.account.currency = this.currency;
      }
    } else {
      this.account.stockPrice = null;
    }
  }

  @computedFrom("account.name", "account.currency", "account.stockPrice", "account.synced", "connTracker.isOnline")
  get canSave() {
    return (
      !ValidationUtil.isEmptyOrWhitespace(this.account.name) &&
      (!this.investmentFund || !!this.account.stockPrice) &&
      JSON.stringify(this.account) !== this.originalJson &&
      !(!this.connTracker.isOnline && this.account.synced)
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
      if (this.isNew) {
        try {
          const id = await this.accountsService.create(this.account);
          this.nameIsInvalid = false;

          this.router.navigateToRoute("accountsEdited", {
            editedId: id,
          });
        } catch {
          this.saveButtonIsLoading = false;
        }
      } else {
        try {
          await this.accountsService.update(this.account);
          this.nameIsInvalid = false;
          this.router.navigateToRoute("accountsEdited", {
            editedId: this.account.id,
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
        await this.accountsService.delete(this.account.id);

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "editAccount.deleteSuccessful");
        this.router.navigateToRoute("accounts");
      } catch {
        this.deleteButtonText = this.i18n.tr("delete");
        this.deleteInProgress = false;
        this.deleteButtonIsLoading = false;
      }
    } else {
      if (await this.accountsService.hasTransactions(this.account.id)) {
        this.transactionsWarningVisible = true;
        this.deleteButtonText = this.i18n.tr("editAccount.okayDelete");
      } else {
        this.deleteButtonText = this.i18n.tr("sure");
      }

      this.deleteInProgress = true;
    }
  }

  cancel() {
    if (!this.deleteInProgress) {
      this.router.navigateToRoute("accounts");
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
  }
}
