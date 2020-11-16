import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { AccountsService } from "services/accountsService";
import { TransactionsService } from "services/transactionsService";
import { LocalStorage } from "utils/localStorage";
import {
  ValidationController,
  validateTrigger,
  ValidationRules,
  ControllerValidateResult,
} from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { TransferFundsModel } from "models/viewmodels/transferFunds";
import { TransactionModel } from "models/entities/transaction";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

@inject(
  Router,
  AccountsService,
  TransactionsService,
  LocalStorage,
  ValidationController,
  I18N,
  EventAggregator
)
export class TransferFunds {
  private model = new TransferFundsModel(null, null, null, null, null);
  private mainAccountId: number;
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

    this.eventAggregator.subscribe("alert-hidden", () => {
      this.amountIsInvalid = false;
    });
  }

  activate() {
    this.model.currency = this.localStorage.getCurrency();

    let amountFrom = 0.01;
    let amountTo = 8000000;
    if (this.model.currency === "MKD") {
      amountFrom = 1;
      amountTo = 450000000;
    }
    ValidationRules.ensure((x: TransferFundsModel) => x.amount)
      .between(amountFrom, amountTo)
      .on(this.model);
  }

  async attached() {
    this.mainAccountId = await this.accountsService.getMainId();

    this.accountsService.getAllAsOptions().then((options) => {
      this.model.fromAccountId = this.mainAccountId;
      this.model.toAccountId = options[1].id;
      this.model.accountOptions = options;
    });
  }

  fromAccountChanged() {
    if (this.model.fromAccountId === this.mainAccountId) {
      if (this.model.fromAccountId === this.model.toAccountId) {
        this.model.toAccountId = this.model.accountOptions[1].id;
      }
    } else {
      if (this.model.fromAccountId === this.model.toAccountId) {
        this.model.toAccountId = this.mainAccountId;
      }
    }
  }

  toAccountChanged() {
    if (this.model.toAccountId === this.mainAccountId) {
      if (this.model.toAccountId === this.model.fromAccountId) {
        this.model.fromAccountId = this.model.accountOptions[1].id;
      }
    } else {
      if (this.model.toAccountId === this.model.fromAccountId) {
        this.model.fromAccountId = this.mainAccountId;
      }
    }
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
    this.eventAggregator.publish("reset-alert-error");

    const result: ControllerValidateResult = await this.validationController.validate();

    this.amountIsInvalid = !result.valid;

    if (result.valid) {
      try {
        const amount = parseFloat(<any>this.model.amount);

        const fromAccountBalance = await this.accountsService.getBalance(
          this.model.fromAccountId,
          this.model.currency
        );
        if (fromAccountBalance < amount) {
          this.amountIsInvalid = true;
          this.transferButtonIsLoading = false;

          const fromAccount = this.model.accountOptions.find(
            (x) => x.id === this.model.fromAccountId
          ).name;

          let formattedBalance: string;
          if (this.model.currency === "MKD") {
            formattedBalance =
              new Intl.NumberFormat("mk-MK", {
                maximumFractionDigits: 0,
              }).format(fromAccountBalance) + " MKD";
          } else {
            formattedBalance = new Intl.NumberFormat("de-DE", {
              style: "currency",
              currency: this.model.currency,
            }).format(fromAccountBalance);
          }

          this.eventAggregator.publish(
            "alert-error",
            this.i18n.tr("transferFunds.accountOnlyHas", {
              account: fromAccount,
              balance: formattedBalance,
            })
          );
          return;
        }

        const transaction = new TransactionModel(
          null,
          this.model.fromAccountId,
          this.model.toAccountId,
          null,
          amount,
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

        this.eventAggregator.publish(
          "alert-success",
          "transferFunds.transferSuccessful"
        );

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
