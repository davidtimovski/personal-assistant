import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { connectTo } from "aurelia-store";

import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { TransactionsService } from "services/transactionsService";
import { CategoriesService } from "services/categoriesService";
import { AccountsService } from "services/accountsService";
import { LocalStorage } from "utils/localStorage";
import { TransactionItem } from "models/viewmodels/transactionItem";
import { TransactionModel } from "models/entities/transaction";
import { SelectOption } from "models/viewmodels/selectOption";
import { CategoryType } from "models/entities/category";
import { TransactionType } from "models/viewmodels/transactionType";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";
import { SearchFilters } from "models/viewmodels/searchFilters";

@autoinject
@connectTo()
export class Transactions {
  private transactions: TransactionItem[];
  private currency: string;
  private language: string;
  private lastEditedId: number;
  private viewCategory = false;
  private filters: SearchFilters;
  private pageCount: number;
  private categoryOptions: SelectOption[];
  private accountOptions: SelectOption[];
  state: State;

  constructor(
    private readonly router: Router,
    private readonly transactionsService: TransactionsService,
    private readonly categoriesService: CategoriesService,
    private readonly accountsService: AccountsService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N
  ) {}

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }

    this.currency = this.localStorage.getCurrency();
    this.language = this.localStorage.getLanguage();
  }

  attached() {
    this.filters = this.state.filters;

    const categoryOptionsPromise = new Promise<void>(async (resolve) => {
      const categoryOptions = [new SelectOption(0, this.i18n.tr("transactions.all"))];

      const options = await this.categoriesService.getAllAsOptions(
        this.i18n.tr("uncategorized"),
        CategoryType.AllTransactions
      );
      this.categoryOptions = categoryOptions.concat(options);
      resolve();
    });
    const accountOptionsPromise = new Promise<void>(async (resolve) => {
      const accountOptions = [new SelectOption(0, this.i18n.tr("transactions.all"))];

      const options = await this.accountsService.getAllAsOptions();
      this.accountOptions = accountOptions.concat(options);
      resolve();
    });

    Promise.all([categoryOptionsPromise, accountOptionsPromise]).then(() => {
      this.getTransactions(false);
    });
  }

  getTransactions(filterChanged: boolean) {
    if (filterChanged) {
      this.filters.page = 1;
    }

    if (this.filters.accountId !== 0 && this.filters.type === 3) {
      this.filters.type = 0;
    }

    this.transactions = null;

    const transactionsPromise = this.transactionsService.getAllByPage(this.filters, this.currency);
    const countPromise = this.transactionsService.count(this.filters);

    Promise.all([transactionsPromise, countPromise]).then((value: [TransactionModel[], number]) => {
      const transactions = value[0];
      const count = value[1];

      const transactionItems = new Array<TransactionItem>();

      for (const transaction of transactions) {
        const categoryName = this.categoryOptions.find((x) => x.id === transaction.categoryId).name;

        transactionItems.push(
          new TransactionItem(
            transaction.id,
            transaction.amount,
            this.getType(transaction.fromAccountId, transaction.toAccountId),
            categoryName,
            this.formatDescription(transaction.description, transaction.isEncrypted),
            this.formatDate(transaction.date),
            transaction.synced
          )
        );
      }

      this.transactions = transactionItems;

      if (filterChanged) {
        // Scroll to top of table
        const transactionsTableWrap = document.getElementById("transactions-table-wrap");
        window.scroll({
          top: transactionsTableWrap.getBoundingClientRect().top + window.scrollY,
          behavior: "smooth",
        });
      }

      this.pageCount = Math.ceil(count / this.filters.pageSize);
    });
  }

  getType(fromAccountId: number, toAccountId: number): TransactionType {
    if (this.filters.accountId) {
      if (this.filters.accountId === fromAccountId) {
        return TransactionType.Expense;
      }

      if (this.filters.accountId === toAccountId) {
        return TransactionType.Deposit;
      }

      return TransactionType.Deposit;
    }

    return TransactionsService.getType(fromAccountId, toAccountId);
  }

  formatDescription(description: string, isEncrypted: boolean): string {
    if (isEncrypted) {
      return this.i18n.tr("encryptedPlaceholder");
    }

    if (!description) {
      return "";
    }

    const length = 40;
    if (description.length <= length) {
      return description;
    }

    return description.substring(0, length - 2) + "..";
  }

  formatDate(dateString: string): string {
    const date = new Date(Date.parse(dateString));
    const month = DateHelper.getShortMonth(date, this.language);

    const now = new Date();
    if (now.getFullYear() === date.getFullYear()) {
      return `${month} ${date.getDate()}`;
    }

    return `${month} ${date.getDate()}, ${date.getFullYear()}`;
  }

  toggleViewCategory() {
    this.viewCategory = !this.viewCategory;
  }

  filterChanged() {
    Actions.changeFilters(this.filters);
    this.getTransactions(true);
  }

  descriptionFilterChanged() {
    Actions.changeFilters(this.filters);
    this.getTransactions(true);
  }

  clearDescriptionFilter() {
    this.filters.description = "";
    this.descriptionFilterChanged();
  }

  first() {
    this.filters.page = 1;

    Actions.changeFilters(this.filters);
    this.getTransactions(false);
  }

  last() {
    this.filters.page = this.pageCount;

    Actions.changeFilters(this.filters);
    this.getTransactions(false);
  }

  previous() {
    this.filters.page--;

    Actions.changeFilters(this.filters);
    this.getTransactions(false);
  }

  next() {
    this.filters.page++;

    Actions.changeFilters(this.filters);
    this.getTransactions(false);
  }

  viewTransaction(id: number) {
    this.router.navigateToRoute("transaction", {
      id: id,
    });
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }
}
