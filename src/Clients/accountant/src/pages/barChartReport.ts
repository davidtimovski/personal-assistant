import { inject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import * as Chart from "chart.js";

import { DateHelper } from "../../../shared/src/utils/dateHelper";
import { TransactionsService } from "services/transactionsService";
import { CategoriesService } from "services/categoriesService";
import { AccountsService } from "services/accountsService";
import { LocalStorage } from "utils/localStorage";
import { AmountByMonth } from "models/viewmodels/amountByMonth";
import { FromOption } from "models/viewmodels/fromOption";
import { TransactionType } from "models/viewmodels/transactionType";
import { TransactionModel } from "models/entities/transaction";
import { SelectOption } from "models/viewmodels/selectOption";
import { CategoryType } from "models/entities/category";

@inject(TransactionsService, CategoriesService, AccountsService, LocalStorage, I18N)
export class BarChartReport {
  private mainAccountId: number;
  private currency: string;
  private chart: Chart;
  private fromOptions: Array<FromOption>;
  private categoryOptions: Array<SelectOption>;
  private dataLoaded: boolean;
  private canvasCtx: CanvasRenderingContext2D;
  private fromDate: string;
  private categoryId: number;
  private type = TransactionType.Any;

  constructor(
    private readonly transactionsService: TransactionsService,
    private readonly categoriesService: CategoriesService,
    private readonly accountsService: AccountsService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N
  ) {
    const from = new Date();
    from.setMonth(from.getMonth() - 6);
    from.setDate(1);
    this.fromDate = DateHelper.format(from);

    Chart.defaults.global.defaultFontFamily = '"Didact Gothic", sans-serif';
    Chart.defaults.global.legend.display = false;
    Chart.defaults.global.animation.duration = 800;
  }

  activate() {
    this.currency = this.localStorage.getCurrency();

    const fromOptions = new Array<FromOption>();
    const now = new Date();
    const date = new Date(now.getFullYear(), now.getMonth(), 1);
    for (let i = 1; i <= 6; i++) {
      date.setMonth(date.getMonth() - 6);

      const value = DateHelper.format(date);
      const label = i * 6 + " " + this.i18n.tr("barChartReport.months");

      fromOptions.push(new FromOption(value, label));
    }
    this.fromOptions = fromOptions;
  }

  async attached() {
    const categoryOptions = [
      new SelectOption(0, this.i18n.tr("barChartReport.all")),
    ];
    this.categoriesService
      .getAllAsOptions(
        this.i18n.tr("uncategorized"),
        CategoryType.AllTransactions
      )
      .then((options) => {
        this.categoryOptions = categoryOptions.concat(options);
      });

    this.canvasCtx = (<HTMLCanvasElement>(
      document.getElementById("chart")
    )).getContext("2d");
    this.chart = new Chart(this.canvasCtx, {
      type: "bar",
      data: {
        datasets: [{}],
      },
    });

    this.mainAccountId = await this.accountsService.getMainId();
    this.loadData();
  }

  async loadData() {
    this.chart.data.labels = [];
    this.chart.data.datasets[0].data = [];
    this.chart.update();
    this.dataLoaded = false;

    this.transactionsService
      .getExpensesAndDepositsFromDate(
        this.fromDate,
        this.mainAccountId,
        this.categoryId,
        this.type,
        this.currency
      )
      .then((transactions: Array<TransactionModel>) => {
        let itemGroups = this.groupBy(
          transactions,
          (x: TransactionModel) => x.date.substring(0, 7) // yyyy-MM
        );

        let fromDate = new Date(this.fromDate);
        const now = new Date();

        const monthsDiff =
          now.getMonth() -
          fromDate.getMonth() +
          12 * (now.getFullYear() - fromDate.getFullYear());

        let items = new Array<AmountByMonth>();
        for (let i = 0; i < monthsDiff; i++) {
          const date = DateHelper.formatYYYYMM(fromDate);

          let monthString = this.i18n
            .tr(`months.${fromDate.getMonth()}`)
            .substring(0, 3);
          if (fromDate.getFullYear() < now.getFullYear()) {
            monthString +=
              " " + fromDate.getFullYear().toString().substring(2, 4);
          }

          if (itemGroups.has(date)) {
            let monthTransactions: Array<TransactionModel>;
            for (let key of itemGroups.keys()) {
              if (key === date) {
                monthTransactions = itemGroups.get(key);
                break;
              }
            }

            const item = new AmountByMonth(date, monthString, 0);

            for (const transaction of monthTransactions) {
              if (transaction.fromAccountId) {
                item.amount -= transaction.amount;
              } else {
                item.amount += transaction.amount;
              }
            }
            item.amount = parseFloat(item.amount.toFixed(2));

            items.push(item);
          } else {
            items.push(new AmountByMonth(date, monthString, 0));
          }

          fromDate.setMonth(fromDate.getMonth() + 1);
        }

        const labels = new Array<string>();
        const amounts = new Array<number>();
        for (let i = 0; i < items.length; i++) {
          labels.push(items[i].month);
          amounts.push(items[i].amount);
        }

        this.dataLoaded = true;

        if (labels.length > 0) {
          this.chart.data.labels = labels;

          const expenseColor = "#f55551";
          const depositColor = "#65c565";
          switch (this.type) {
            case TransactionType.Any:
              const colors = new Array<string>();
              for (const amount of amounts) {
                colors.push(amount < 0 ? expenseColor : depositColor);
              }

              this.chart.data.datasets[0].backgroundColor = colors;
              break;
            case TransactionType.Expense:
              this.chart.data.datasets[0].backgroundColor = expenseColor;
              break;
            case TransactionType.Deposit:
              this.chart.data.datasets[0].backgroundColor = depositColor;
              break;
          }

          this.chart.data.datasets[0].data = amounts;
        } else {
          this.chart.data.labels = [];
          this.chart.data.datasets[0].data = [];
        }

        this.chart.update();
      });
  }

  groupBy(
    list: Array<TransactionModel>,
    keyGetter: { (x: TransactionModel): string; (arg0: TransactionModel): any }
  ) {
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
