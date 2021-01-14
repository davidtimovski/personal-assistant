import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { CategoriesService } from "services/categoriesService";
import { AccountsService } from "services/accountsService";
import { TransactionsService } from "services/transactionsService";
import { EncryptionService } from "services/encryptionService";
import { LocalStorage } from "utils/localStorage";
import { TransactionModel } from "models/entities/transaction";
import { ViewTransaction } from "models/viewmodels/viewTransaction";
import { TransactionType } from "models/viewmodels/transactionType";

@inject(
  Router,
  CategoriesService,
  AccountsService,
  TransactionsService,
  EncryptionService,
  LocalStorage,
  I18N
)
export class Transaction {
  private transactionId: number;
  private fromExpenditureHeatmap: boolean;
  private model: ViewTransaction;
  private currency: string;
  private passwordInput: HTMLInputElement;
  private passwordShown = false;
  private decryptButtonIsLoading = false;
  private decryptionPasswordIsInvalid: boolean;
  private typeStringLookup: Array<string>;

  constructor(
    private readonly router: Router,
    private readonly categoriesService: CategoriesService,
    private readonly accountsService: AccountsService,
    private readonly transactionsService: TransactionsService,
    private readonly encryptionService: EncryptionService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N
  ) {
    this.typeStringLookup = [
      this.i18n.tr("transaction.expense"),
      this.i18n.tr("transaction.deposit"),
      this.i18n.tr("transaction.transfer"),
    ];
  }

  activate(params: any) {
    this.transactionId = parseInt(params.id, 10);
    this.fromExpenditureHeatmap = <boolean>params.fromExpenditureHeatmap;

    this.currency = this.localStorage.getCurrency();
  }

  attached() {
    this.transactionsService
      .getForViewing(this.transactionId, this.currency)
      .then(async (transaction: TransactionModel) => {
        if (transaction === null) {
          this.router.navigateToRoute("notFound");
        }

        const model = new ViewTransaction(
          null,
          null,
          null,
          transaction.convertedAmount,
          transaction.currency,
          transaction.amount,
          null,
          null,
          null,
          transaction.isEncrypted,
          transaction.encryptedDescription,
          transaction.salt,
          transaction.nonce,
          null
        );

        const type = this.getType(
          transaction.fromAccountId,
          transaction.toAccountId
        );
        model.type = this.typeStringLookup[type - 1];

        if (transaction.categoryId) {
          const category = await this.categoriesService.get(
            transaction.categoryId
          );
          if (category.parent) {
            model.category = `${category.parent}/${category.name}`;
          } else {
            model.category = category.name;
          }
        } else {
          model.category = this.i18n.tr("uncategorized");
        }

        if (type === TransactionType.Transfer) {
          const fromAccount = await this.accountsService.get(
            transaction.fromAccountId
          );
          const toAccount = await this.accountsService.get(
            transaction.toAccountId
          );
          model.accountLabel = this.i18n.tr("transaction.accounts");
          model.accountValue = this.i18n.tr("transaction.to", {
            from: fromAccount.name,
            to: toAccount.name,
          });
        } else if (type === TransactionType.Deposit) {
          const toAccount = await this.accountsService.get(
            transaction.toAccountId
          );
          model.accountLabel = this.i18n.tr("transaction.toAccount");
          model.accountValue = toAccount.name;
        } else {
          const fromAccount = await this.accountsService.get(
            transaction.fromAccountId
          );
          model.accountLabel = this.i18n.tr("transaction.fromAccount");
          model.accountValue = fromAccount.name;
        }

        model.descriptionInHtml = this.formatDescription(
          transaction.description
        );
        model.date = this.formatOcccurrenceDate(transaction.date);

        this.model = model;
      });
  }

  formatDescription(description: string): string {
    if (!description) {
      return null;
    }

    description = description.replace(/(?:\r\n|\r|\n)/g, "<br>");

    return description;
  }

  getType(fromAccountId: number, toAccountId: number): TransactionType {
    if (fromAccountId && toAccountId) {
      return TransactionType.Transfer;
    }

    if (fromAccountId && !toAccountId) {
      return TransactionType.Expense;
    }

    return TransactionType.Deposit;
  }

  formatOcccurrenceDate(occcurrenceDateString: string): string {
    const date = new Date(Date.parse(occcurrenceDateString));
    const month = this.i18n.tr(`months.${date.getMonth()}`);

    const now = new Date();
    if (now.getFullYear() === date.getFullYear()) {
      return `${month} ${date.getDate()}`;
    }

    return `${month} ${date.getDate()}, ${date.getFullYear()}`;
  }

  togglePasswordShow() {
    if (this.passwordShown) {
      this.passwordInput.type = "password";
    } else {
      this.passwordInput.type = "text";
    }

    this.passwordShown = !this.passwordShown;
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

      this.model.isEncrypted = false;
      this.model.descriptionInHtml = this.formatDescription(
        decryptedDescription
      );
      this.decryptButtonIsLoading = false;
    } catch {
      this.decryptionPasswordIsInvalid = true;
      this.decryptButtonIsLoading = false;
    }
  }

  back() {
    if (this.fromExpenditureHeatmap) {
      this.router.navigateToRoute("expenditureHeatmap");
    } else {
      this.router.navigateToRoute("transactions");
    }
  }
}
