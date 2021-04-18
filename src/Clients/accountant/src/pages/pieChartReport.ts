import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import * as Chart from "chart.js";
import { connectTo } from "aurelia-store";

import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { TransactionsService } from "services/transactionsService";
import { AccountsService } from "services/accountsService";
import { LocalStorage } from "utils/localStorage";
import { AmountByCategory } from "models/viewmodels/amountByCategory";
import { TransactionType } from "models/viewmodels/transactionType";
import { TransactionModel } from "models/entities/transaction";
import { SearchFilters } from "models/viewmodels/searchFilters";
import * as Actions from "utils/state/actions";

@inject(Router, TransactionsService, AccountsService, LocalStorage, I18N)
@connectTo()
export class PieChartReport {
  private mainAccountId: number;
  private currency: string;
  private chart: Chart;
  private items: Array<AmountByCategory>;
  private sum: number;
  private canvasCtx: CanvasRenderingContext2D;
  private fromDate: string;
  private toDate: string;
  private maxDate: string;
  private type = TransactionType.Expense;
  private readonly colors = ["#7a79e6", "#dbd829", "#49e09b", "#e88042", "#5aacf1", "#f55551", "#b6ca53"];
  private showTable = false;

  constructor(
    private readonly router: Router,
    private readonly transactionsService: TransactionsService,
    private readonly accountsService: AccountsService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N
  ) {
    const from = new Date();
    from.setDate(1);
    this.fromDate = DateHelper.format(from);

    this.toDate = this.maxDate = DateHelper.format(new Date());

    Chart.defaults.global.defaultFontFamily = '"Didact Gothic", sans-serif';
    Chart.defaults.global.legend.display = false;
    Chart.defaults.global.animation.duration = 800;
  }

  activate() {
    this.currency = this.localStorage.getCurrency();
  }

  async attached() {
    this.canvasCtx = (<HTMLCanvasElement>document.getElementById("chart")).getContext("2d");
    this.chart = new Chart(this.canvasCtx, {
      type: "pie",
      data: {
        datasets: [
          {
            backgroundColor: this.colors,
          },
        ],
      },
      options: {
        animation: {
          onComplete: () => {
            this.showTable = this.items?.length > 0;
          },
        },
      },
    });

    this.mainAccountId = await this.accountsService.getMainId();
    this.loadData();
  }

  async loadData() {
    this.showTable = false;
    this.chart.data.labels = [];
    this.chart.data.datasets[0].data = [];
    this.chart.update();
    this.items = null;

    this.transactionsService
      .getExpendituresAndDepositsByCategory(
        this.fromDate,
        this.toDate,
        this.mainAccountId,
        this.type,
        this.currency,
        this.i18n.tr("uncategorized")
      )
      .then((items: Array<AmountByCategory>) => {
        const labels = new Array<string>();
        const amounts = new Array<number>();
        let sum = 0;

        const flatItems = new Array<AmountByCategory>();
        for (const item of items) {
          flatItems.push(item);
          flatItems.push(...item.subItems);

          sum += item.amount;
        }

        this.sum = sum;
        this.items = items;

        for (let i = 0; i < flatItems.length; i++) {
          (<any>flatItems[i]).color = i < this.colors.length ? this.colors[i] : "#e0e0e0";

          labels.push(flatItems[i].categoryName);
          amounts.push(flatItems[i].amount);
        }

        if (flatItems.length > 0) {
          this.chart.data.labels = labels;
          this.chart.data.datasets[0].data = amounts;
        } else {
          this.chart.data.labels = [];
          this.chart.data.datasets[0].data = [];
        }

        this.chart.update();
      });
  }

  goToTransactions(item: AmountByCategory) {
    Actions.changeFilters(new SearchFilters(1, 15, this.fromDate, this.toDate, 0, item.categoryId, this.type, null));

    this.router.navigateToRoute("transactions");
  }

  groupBy(list: Array<TransactionModel>, keyGetter: { (x: TransactionModel): string; (arg0: TransactionModel): any }) {
    const map = new Map();
    list.forEach((item) => {
      const key = keyGetter(item);
      const collection = map.get(key);
      if (!collection) {
        map.set(key, [item]);
      } else {
        collection.push(item);
      }
    });
    return map;
  }
}
