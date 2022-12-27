import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../../../shared/src/utils/httpProxy";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { UpcomingExpensesIDBHelper } from "utils/upcomingExpensesIDBHelper";
import { UpcomingExpense } from "models/entities/upcomingExpense";

@autoinject
export class UpcomingExpensesService {
  constructor(
    private readonly httpProxy: HttpProxy,
    private readonly idbHelper: UpcomingExpensesIDBHelper,
    private readonly currenciesService: CurrenciesService,
    private readonly logger: ErrorLogger
  ) {}

  async getAll(currency: string): Promise<Array<UpcomingExpense>> {
    try {
      const upcomingExpenses = await this.idbHelper.getAll();

      upcomingExpenses.forEach((x: UpcomingExpense) => {
        x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
      });

      return upcomingExpenses;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  get(id: number): Promise<UpcomingExpense> {
    return this.idbHelper.get(id);
  }

  async create(upcomingExpense: UpcomingExpense): Promise<number> {
    try {
      upcomingExpense.amount = parseFloat(<any>upcomingExpense.amount);

      if (upcomingExpense.description) {
        upcomingExpense.description = upcomingExpense.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
      }
      const now = DateHelper.adjustForTimeZone(new Date());
      upcomingExpense.createdDate = upcomingExpense.modifiedDate = now;

      if (navigator.onLine) {
        upcomingExpense.id = await this.httpProxy.ajax<number>("api/upcomingexpenses", {
          method: "post",
          body: json(upcomingExpense),
        });
        upcomingExpense.synced = true;
      }

      await this.idbHelper.create(upcomingExpense);

      return upcomingExpense.id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async update(upcomingExpense: UpcomingExpense): Promise<void> {
    try {
      upcomingExpense.amount = parseFloat(<any>upcomingExpense.amount);

      if (upcomingExpense.description) {
        upcomingExpense.description = upcomingExpense.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
      }
      upcomingExpense.modifiedDate = DateHelper.adjustForTimeZone(new Date());

      if (navigator.onLine) {
        await this.httpProxy.ajaxExecute("api/upcomingexpenses", {
          method: "put",
          body: json(upcomingExpense),
        });
        upcomingExpense.synced = true;
      } else if (await this.idbHelper.isSynced(upcomingExpense.id)) {
        throw "failedToFetchError";
      }

      await this.idbHelper.update(upcomingExpense);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number): Promise<void> {
    try {
      if (navigator.onLine) {
        await this.httpProxy.ajaxExecute(`api/upcomingexpenses/${id}`, {
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
