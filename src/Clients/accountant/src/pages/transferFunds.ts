import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { DateHelper } from "../../../shared/src/utils/dateHelper";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { AccountsService } from "services/accountsService";
import { TransactionsService } from "services/transactionsService";
import { LocalStorage } from "utils/localStorage";
import { TransferFundsModel } from "models/viewmodels/transferFunds";
import { TransactionModel } from "models/entities/transaction";
import { Account } from "models/entities/account";

@inject(Router, AccountsService, TransactionsService, LocalStorage, ValidationController, I18N, EventAggregator)
export class TransferFunds {
  private model = new TransferFundsModel(null, null, null, null, null, null, null);
  private accounts: Account[];
  private mainAccountId: number;
  private fromAccountLabel: string;
  private toAccountLabel: string;
  private amountIsInvalid: boolean;
  private transferButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly accountsService: AccountsService,
    private readonly transactionsService: TransactionsService,
    private readonly localStorage: LocalStorage,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.amountIsInvalid = false;
    });
  }

  activate() {
    this.model.currency = this.localStorage.getCurrency();

    let amountFrom = 0.01;
    let amountTo = 8000001;
    if (this.model.currency === "MKD") {
      amountFrom = 0;
      amountTo = 450000001;
    }
    ValidationRules.ensure((x: TransferFundsModel) => x.amount)
      .between(amountFrom, amountTo)
      .on(this.model);
  }

  async attached() {
    this.mainAccountId = await this.accountsService.getMainId();

    this.accountsService.getAllAsOptions().then((options) => {
      this.model.fromAccountId = this.mainAccountId;
      this.setFromAccount();

      this.model.toAccountId = options[1].id;
      this.setToAccount();

      this.model.accountOptions = options;
    });

    this.accountsService.getAllWithBalance(this.model.currency).then((accounts: Array<Account>) => {
      this.accounts = accounts;
    });
  }

  fromAccountChanged() {
    this.setFromAccount();

    if (this.model.fromAccountId === this.mainAccountId) {
      if (this.model.fromAccountId === this.model.toAccountId) {
        this.model.toAccountId = this.model.accountOptions[1].id;
        this.setToAccount();
      }
    } else {
      if (this.model.fromAccountId === this.model.toAccountId) {
        this.model.toAccountId = this.mainAccountId;
        this.setToAccount();
      }
    }
  }

  toAccountChanged() {
    this.setToAccount();

    if (this.model.toAccountId === this.mainAccountId) {
      if (this.model.toAccountId === this.model.fromAccountId) {
        this.model.fromAccountId = this.model.accountOptions[1].id;
        this.setFromAccount();
      }
    } else {
      if (this.model.toAccountId === this.model.fromAccountId) {
        this.model.fromAccountId = this.mainAccountId;
        this.setFromAccount();
      }
    }
  }

  private setFromAccount() {
    this.model.fromAccount = this.accounts.find((x) => x.id === this.model.fromAccountId);
    this.fromAccountLabel =
      this.model.fromAccount.stockPrice === null
        ? this.i18n.tr("transferFunds.fromAccount")
        : this.i18n.tr("transferFunds.fromInvestmentFund");
  }

  private setToAccount() {
    this.model.toAccount = this.accounts.find((x) => x.id === this.model.toAccountId);
    this.toAccountLabel =
      this.model.toAccount.stockPrice === null
        ? this.i18n.tr("transferFunds.toAccount")
        : this.i18n.tr("transferFunds.toInvestmentFund");
  }

  @computedFrom("model.amount")
  get canTransfer() {
    return !!this.model.amount;
  }

  async transfer() {
    if (!this.canTransfer || this.transferButtonIsLoading) {
      return;
    }

    this.transferButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const result: ControllerValidateResult = await this.validationController.validate();

    this.amountIsInvalid = !result.valid;

    if (result.valid) {
      try {
        const amount = parseFloat(<any>this.model.amount);

        // const fromAccountBalance = await this.accountsService.getBalance(
        //   this.model.fromAccountId,
        //   this.model.currency
        // );
        if (this.model.fromAccount.balance < amount) {
          this.amountIsInvalid = true;
          this.transferButtonIsLoading = false;

          const fromAccount = this.model.accountOptions.find((x) => x.id === this.model.fromAccountId).name;

          let formattedBalance: string;
          if (this.model.currency === "MKD") {
            formattedBalance =
              new Intl.NumberFormat("mk-MK", {
                maximumFractionDigits: 0,
              }).format(this.model.fromAccount.balance) + " MKD";
          } else {
            formattedBalance = new Intl.NumberFormat("de-DE", {
              style: "currency",
              currency: this.model.currency,
            }).format(this.model.fromAccount.balance);
          }

          this.eventAggregator.publish(
            AlertEvents.ShowError,
            this.i18n.tr("transferFunds.accountOnlyHas", {
              account: fromAccount,
              balance: formattedBalance,
            })
          );
          return;
        }

        let fromStocks: number = null;
        const fromAccount = this.accounts.find((x) => x.id === this.model.fromAccountId);
        if (fromAccount.stockPrice !== null) {
          fromStocks = amount / fromAccount.stockPrice;
        }

        let toStocks: number = null;
        const toAccount = this.accounts.find((x) => x.id === this.model.toAccountId);
        if (toAccount.stockPrice !== null) {
          toStocks = amount / toAccount.stockPrice;
        }

        const transaction = new TransactionModel(
          null,
          this.model.fromAccountId,
          this.model.toAccountId,
          null,
          amount,
          fromStocks,
          toStocks,
          this.model.currency,
          null,
          DateHelper.format(new Date()),
          false,
          null,
          null,
          null,
          null,
          null
        );

        await this.transactionsService.create(transaction, null);
        this.amountIsInvalid = false;

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "transferFunds.transferSuccessful");

        this.router.navigateToRoute("accountsEdited", {
          editedId: this.model.fromAccountId,
          editedId2: this.model.toAccountId,
        });
      } catch {
        this.transferButtonIsLoading = false;
      }
    } else {
      this.transferButtonIsLoading = false;
    }
  }
}
