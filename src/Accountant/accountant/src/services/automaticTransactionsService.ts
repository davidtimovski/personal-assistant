import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../../../shared/src/utils/httpProxy";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { AutomaticTransactionsIDBHelper } from "utils/automaticTransactionsIDBHelper";
import { AutomaticTransaction } from "models/entities/automaticTransaction";

@autoinject
export class AutomaticTransactionsService {
  constructor(
    private readonly httpProxy: HttpProxy,
    private readonly idbHelper: AutomaticTransactionsIDBHelper,
    private readonly currenciesService: CurrenciesService,
    private readonly logger: ErrorLogger
  ) {}

  async getAll(currency: string): Promise<Array<AutomaticTransaction>> {
    try {
      const automaticTransactions = await this.idbHelper.getAll();

      automaticTransactions.forEach((x: AutomaticTransaction) => {
        x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
      });

      return automaticTransactions;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  get(id: number): Promise<AutomaticTransaction> {
    return this.idbHelper.get(id);
  }

  async create(automaticTransaction: AutomaticTransaction): Promise<number> {
    try {
      automaticTransaction.amount = parseFloat(<any>automaticTransaction.amount);

      if (automaticTransaction.description) {
        automaticTransaction.description = automaticTransaction.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
      }
      const now = DateHelper.adjustForTimeZone(new Date());
      automaticTransaction.createdDate = automaticTransaction.modifiedDate = now;

      if (navigator.onLine) {
        automaticTransaction.id = await this.httpProxy.ajax<number>("api/automatictransactions", {
          method: "post",
          body: json(automaticTransaction),
        });
        automaticTransaction.synced = true;
      }

      await this.idbHelper.create(automaticTransaction);

      return automaticTransaction.id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async update(automaticTransaction: AutomaticTransaction): Promise<void> {
    try {
      automaticTransaction.amount = parseFloat(<any>automaticTransaction.amount);

      if (automaticTransaction.description) {
        automaticTransaction.description = automaticTransaction.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
      }
      automaticTransaction.modifiedDate = DateHelper.adjustForTimeZone(new Date());

      if (navigator.onLine) {
        await this.httpProxy.ajaxExecute("api/automatictransactions", {
          method: "put",
          body: json(automaticTransaction),
        });
        automaticTransaction.synced = true;
      } else if (await this.idbHelper.isSynced(automaticTransaction.id)) {
        throw "failedToFetchError";
      }

      await this.idbHelper.update(automaticTransaction);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number): Promise<void> {
    try {
      if (navigator.onLine) {
        await this.httpProxy.ajaxExecute(`api/automatictransactions/${id}`, {
          method: "delete",
        });
      } else if (await this.idbHelper.isSynced(id)) {
        throw "failedToFetchError";
      }

      await this.idbHelper.delete(id);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
