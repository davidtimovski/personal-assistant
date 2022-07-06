import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { UpcomingExpensesIDBHelper } from "../utils/upcomingExpensesIDBHelper";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import * as environment from "../../config/environment.json";

@inject(AuthService, HttpClient, EventAggregator, UpcomingExpensesIDBHelper, CurrenciesService)
export class UpcomingExpensesService extends HttpProxyBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly idbHelper: UpcomingExpensesIDBHelper,
    private readonly currenciesService: CurrenciesService
  ) {
    super(authService, httpClient, eventAggregator);
  }

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
        upcomingExpense.id = await this.ajax<number>("upcomingexpenses", {
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
        await this.ajaxExecute("upcomingexpenses", {
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
        await this.ajaxExecute(`upcomingexpenses/${id}`, {
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
