import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { AccountsService } from "services/accountsService";
import { LocalStorage } from "utils/localStorage";
import { Account } from "models/entities/account";
import { EventAggregator } from "aurelia-event-aggregator";
import { AccountItem } from "models/viewmodels/accountItem";

@inject(Router, AccountsService, LocalStorage, EventAggregator)
export class Accounts {
  private accounts: Array<AccountItem>;
  private sum: number;
  private currency: string;
  private lastEditedId: number;
  private lastEditedId2: number;
  private syncing = false;

  constructor(
    private readonly router: Router,
    private readonly accountsService: AccountsService,
    private readonly localStorage: LocalStorage,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe("sync-started", () => {
      this.syncing = true;
    });
    this.eventAggregator.subscribe("sync-finished", () => {
      this.syncing = false;
    });
  }

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }
    if (params.editedId2) {
      this.lastEditedId2 = parseInt(params.editedId2, 10);
    }

    this.currency = this.localStorage.getCurrency();
  }

  attached() {
    this.accountsService
      .getAllWithBalance(this.currency)
      .then((accounts: Array<Account>) => {
        const accountItems = new Array<AccountItem>();
        let sum = 0;

        for (const account of accounts) {
          accountItems.push(
            new AccountItem(
              account.id,
              account.name,
              account.balance,
              account.synced
            )
          );

          sum += account.balance;
        }

        this.accounts = accountItems;
        this.sum = sum;
      });
  }

  newAccount() {
    if (!this.syncing) {
      this.router.navigateToRoute("editAccount", { id: 0 });
    }
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }

  @computedFrom("lastEditedId2")
  get getEditedId2() {
    return this.lastEditedId2;
  }
}
