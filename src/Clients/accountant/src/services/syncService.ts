import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../../../shared/src/utils/httpProxy";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { AccountsIDBHelper } from "utils/accountsIDBHelper";
import { CategoriesIDBHelper } from "utils/categoriesIDBHelper";
import { TransactionsIDBHelper } from "utils/transactionsIDBHelper";
import { UpcomingExpensesIDBHelper } from "utils/upcomingExpensesIDBHelper";
import { DebtsIDBHelper } from "utils/debtsIDBHelper";
import { LocalStorage } from "utils/localStorage";
import { Changed, Create, Created } from "models/sync";

@autoinject
export class SyncService {
  constructor(
    private readonly httpProxy: HttpProxy,
    private readonly accountsIDBHelper: AccountsIDBHelper,
    private readonly categoriesIDBHelper: CategoriesIDBHelper,
    private readonly transactionsIDBHelper: TransactionsIDBHelper,
    private readonly upcomingExpensesIDBHelper: UpcomingExpensesIDBHelper,
    private readonly debtsIDBHelper: DebtsIDBHelper,
    private readonly localStorage: LocalStorage,
    private readonly logger: ErrorLogger
  ) {}

  async sync(lastSynced: string): Promise<string> {
    try {
      let lastSyncedServer = "1970-01-01T00:00:00.000Z";

      await this.upcomingExpensesIDBHelper.deleteOld();

      const changed = await this.httpProxy.ajax<Changed>("api/sync/changes", {
        method: "post",
        body: json({
          lastSynced: lastSynced,
        }),
      });
      lastSyncedServer = changed.lastSynced;

      await this.accountsIDBHelper.sync(changed.deletedAccountIds, changed.accounts);
      await this.categoriesIDBHelper.sync(changed.deletedCategoryIds, changed.categories);
      await this.transactionsIDBHelper.sync(changed.deletedTransactionIds, changed.transactions);
      await this.upcomingExpensesIDBHelper.sync(changed.deletedUpcomingExpenseIds, changed.upcomingExpenses);
      await this.debtsIDBHelper.sync(changed.deletedDebtIds, changed.debts);

      const accountsToCreate = await this.accountsIDBHelper.getForSyncing();
      const categoriesToCreate = await this.categoriesIDBHelper.getForSyncing();
      const transactionsToCreate = await this.transactionsIDBHelper.getForSyncing();
      const upcomingExpensesToCreate = await this.upcomingExpensesIDBHelper.getForSyncing();
      const debtsToCreate = await this.debtsIDBHelper.getForSyncing();
      const create = new Create(
        accountsToCreate,
        categoriesToCreate,
        transactionsToCreate,
        upcomingExpensesToCreate,
        debtsToCreate
      );
      const created = await this.httpProxy.ajax<Created>("api/sync/create-entities", {
        method: "post",
        body: json(create),
      });

      await this.accountsIDBHelper.consolidate(created.accountIdPairs);
      await this.categoriesIDBHelper.consolidate(created.categoryIdPairs);
      await this.transactionsIDBHelper.consolidate(created.transactionIdPairs);
      await this.upcomingExpensesIDBHelper.consolidate(created.upcomingExpenseIdPairs);
      await this.debtsIDBHelper.consolidate(created.debtIdPairs);

      return lastSyncedServer;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async totalSync(): Promise<void> {
    try {
      const deleteContextRequest = window.indexedDB.deleteDatabase("IDBContext");

      deleteContextRequest.onsuccess = () => {
        const deleteDbNamesRequest = window.indexedDB.deleteDatabase("__dbnames");

        deleteDbNamesRequest.onsuccess = () => {
          this.localStorage.setLastSynced("1970-01-01T00:00:00.000Z");

          window.location.href = "/dashboard";
        };
      };
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
