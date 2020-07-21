import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { UpcomingExpensesIDBHelper } from "../utils/upcomingExpensesIDBHelper";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { UpcomingExpense } from "models/entities/upcomingExpense";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

@inject(
  AuthService,
  HttpClient,
  EventAggregator,
  UpcomingExpensesIDBHelper,
  CurrenciesService
)
export class UpcomingExpensesService extends HttpProxyBase {
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
    const upcomingExpenses = await this.idbHelper.getAll();

    upcomingExpenses.forEach((x: UpcomingExpense) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    return upcomingExpenses;
  }

  async get(id: number): Promise<UpcomingExpense> {
    const upcomingExpense = await this.idbHelper.get(id);
    if (!upcomingExpense) {
      return null;
    }

    return upcomingExpense;
  }

  async create(upcomingExpense: UpcomingExpense): Promise<number> {
    upcomingExpense.amount = parseFloat(<any>upcomingExpense.amount);

    if (upcomingExpense.description) {
      upcomingExpense.description = upcomingExpense.description
        .replace(/(\r\n|\r|\n){3,}/g, "$1\n")
        .trim();
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
  }

  async update(upcomingExpense: UpcomingExpense): Promise<void> {
    upcomingExpense.amount = parseFloat(<any>upcomingExpense.amount);

    if (upcomingExpense.description) {
      upcomingExpense.description = upcomingExpense.description
        .replace(/(\r\n|\r|\n){3,}/g, "$1\n")
        .trim();
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
  }

  async delete(id: number): Promise<void> {
    if (navigator.onLine) {
      await this.ajaxExecute(`upcomingexpenses/${id}`, {
        method: "delete",
      });
    } else if (await this.idbHelper.isSynced(id)) {
      throw "failedToFetchError";
    }

    await this.idbHelper.delete(id);
  }
}
