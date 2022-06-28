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

@inject(AuthService, HttpClient, EventAggregator, DebtsIDBHelper, CurrenciesService)
export class DebtsService extends HttpProxyBase {
  private readonly mergedDebtSeparator = "----------";

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

  async createOrMerge(debt: DebtModel, mergeDebtPerPerson: boolean): Promise<number> {
    const now = DateHelper.adjustForTimeZone(new Date());
    debt.amount = parseFloat(<any>debt.amount);

    const otherDebtWithPerson = await this.idbHelper.getByPerson(debt.person.trim().toLowerCase());
    if (mergeDebtPerPerson && otherDebtWithPerson.length > 0) {
      let descriptionsArray = new Array<string>();
      let balance = 0;
      for (let otherDebt of otherDebtWithPerson) {
        const convertedAmount = this.currenciesService.convert(otherDebt.amount, otherDebt.currency, debt.currency);
        if (otherDebt.userIsDebtor) {
          balance -= convertedAmount;
        } else {
          balance += convertedAmount;
        }

        let desc = this.getMergedDebtDescription(
          convertedAmount,
          debt.currency,
          otherDebt.userIsDebtor,
          otherDebt.createdDate,
          otherDebt.description
        );
        descriptionsArray.push(desc);
      }

      if (debt.userIsDebtor) {
        balance -= debt.amount;
      } else {
        balance += debt.amount;
      }

      let newDesc = this.getMergedDebtDescription(debt.amount, debt.currency, debt.userIsDebtor, now, debt.description);
      descriptionsArray.push(newDesc);

      const description =
        descriptionsArray.length > 0 ? descriptionsArray.join(`\n${this.mergedDebtSeparator}\n`) : null;

      const mergedDebt = new DebtModel(null, debt.person, balance, debt.currency, description, balance < 0, now, now);

      if (navigator.onLine) {
        mergedDebt.id = await this.ajax<number>("debts/merged", {
          method: "post",
          body: json(mergedDebt),
        });
        mergedDebt.synced = true;
      }

      const otherDebtIds = otherDebtWithPerson.map((x) => x.id);
      await this.idbHelper.createMerged(mergedDebt, otherDebtIds);

      return mergedDebt.id;
    } else {
      if (debt.description) {
        debt.description = debt.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
      }
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
  }

  private getMergedDebtDescription(
    amount: number,
    currency: string,
    userIsDebtor: boolean,
    createdDate: Date,
    description: string
  ) {
    const date = DateHelper.formatForReading(createdDate);

    // start with date
    let result = date + " | ";

    // add amount (positive or negative) and currency
    result += userIsDebtor ? "-" + amount + currency : amount + currency;

    if (description) {
      if (description.includes(this.mergedDebtSeparator)) {
        // use existing description if the debt is already a merged one
        result = description;
      } else {
        result += ` | ${description}`;
      }
    }

    return result;
  }

  async update(debt: DebtModel): Promise<void> {
    debt.amount = parseFloat(<any>debt.amount);

    if (debt.description) {
      debt.description = debt.description.replace(/(\r\n|\r|\n){3,}/g, "$1\n").trim();
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
