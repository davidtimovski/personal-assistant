import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { AuthService } from "../../../shared/src/services/authService";
import { DebtsIDBHelper } from "../utils/debtsIDBHelper";
import { CurrenciesService } from "../../../shared/src/services/currenciesService";
import { DebtModel } from "models/entities/debt";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

@inject(
  AuthService,
  HttpClient,
  EventAggregator,
  DebtsIDBHelper,
  CurrenciesService
)
export class DebtsService extends HttpProxyBase {
  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly idbHelper: DebtsIDBHelper,
    private readonly currenciesService: CurrenciesService
  ) {
    super(authService, httpClient, eventAggregator);
  }

  async getAll(currency: string): Promise<Array<DebtModel>> {
    const debts = await this.idbHelper.getAll();

    debts.forEach((x: DebtModel) => {
      x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
    });

    return debts;
  }

  async get(id: number): Promise<DebtModel> {
    const debt = await this.idbHelper.get(id);
    if (!debt) {
      return null;
    }

    return debt;
  }

  async create(debt: DebtModel): Promise<number> {
    debt.amount = parseFloat(<any>debt.amount);

    if (debt.description) {
      debt.description = debt.description
        .replace(/(\r\n|\r|\n){3,}/g, "$1\n")
        .trim();
    }
    const now = DateHelper.adjustForTimeZone(new Date());
    debt.createdDate = debt.modifiedDate = now;

    if (navigator.onLine) {
      debt.id = await this.ajax<number>("debts", {
        method: "post",
        body: json(debt),
      });
      debt.synced = true;
    }

    await this.idbHelper.create(debt);

    return debt.id;
  }

  async update(debt: DebtModel): Promise<void> {
    debt.amount = parseFloat(<any>debt.amount);

    if (debt.description) {
      debt.description = debt.description
        .replace(/(\r\n|\r|\n){3,}/g, "$1\n")
        .trim();
    }
    debt.modifiedDate = DateHelper.adjustForTimeZone(new Date());

    if (navigator.onLine) {
      await this.ajaxExecute("debts", {
        method: "put",
        body: json(debt),
      });
      debt.synced = true;
    } else if (await this.idbHelper.isSynced(debt.id)) {
      throw "failedToFetchError";
    }

    await this.idbHelper.update(debt);
  }

  async delete(id: number): Promise<void> {
    if (navigator.onLine) {
      await this.ajaxExecute(`debts/${id}`, {
        method: "delete",
      });
    } else if (await this.idbHelper.isSynced(id)) {
      throw "failedToFetchError";
    }

    await this.idbHelper.delete(id);
  }
}
