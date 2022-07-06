import { inject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import { BarController, BarElement, CategoryScale, Chart, LinearScale } from "chart.js";

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
  private language: string;
  private chart: Chart;
  private fromOptions: FromOption[];
  private categoryOptions: SelectOption[];
  private balanceAverage: number;
  private spentAverage: number;
  private depositedAverage: number;
  private savedAverage: number;
  private dataLoaded: boolean;
  private canvasCtx: CanvasRenderingContext2D;
  private fromDate: string;
  private categoryId = 0;
  private categoryType = CategoryType.AllTransactions;
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

    Chart.register(BarController, BarElement, CategoryScale, LinearScale);
    Chart.defaults.font.family = '"Didact Gothic", sans-serif';
  }

  activate() {
    this.currency = this.localStorage.getCurrency();
    this.language = this.localStorage.getLanguage();

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
    const categoryOptions = [new SelectOption(0, this.i18n.tr("barChartReport.all"))];
    this.categoriesService
      .getAllAsOptions(this.i18n.tr("uncategorized"), CategoryType.AllTransactions)
      .then((options) => {
        this.categoryOptions = categoryOptions.concat(options);
      });

    this.canvasCtx = (<HTMLCanvasElement>document.getElementById("chart")).getContext("2d");
    this.chart = new Chart(this.canvasCtx, {
      type: "bar",
      data: {
        datasets: [{ data: [] }],
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

    if (this.categoryId) {
      const category = await this.categoriesService.get(this.categoryId);
      this.categoryType = category.type;

      if (category.type === CategoryType.DepositOnly) {
        this.type = TransactionType.Deposit;
      } else if (category.type === CategoryType.ExpenseOnly) {
        this.type = TransactionType.Expense;
      }
    } else {
      this.categoryType = CategoryType.AllTransactions;
    }

    const transactions = await this.transactionsService.getForBarChart(
      this.fromDate,
      this.mainAccountId,
      this.categoryId,
      this.type,
      this.currency
    );

    let itemGroups = this.groupBy(
      transactions,
      (x: TransactionModel) => x.date.substring(0, 7) // yyyy-MM
    );

    const fromDate = new Date(this.fromDate);
    const now = new Date();

    const monthsDiff = now.getMonth() - fromDate.getMonth() + 12 * (now.getFullYear() - fromDate.getFullYear());

    let balanceSum = 0;
    let spentSum = 0;
    let depositedSum = 0;
    let savedSum = 0;

    let items = new Array<AmountByMonth>();
    for (let i = 0; i < monthsDiff; i++) {
      const date = DateHelper.formatYYYYMM(fromDate);

      let monthString = DateHelper.getShortMonth(fromDate, this.language);
      if (fromDate.getFullYear() < now.getFullYear()) {
        monthString += " " + fromDate.getFullYear().toString().substring(2, 4);
      }

      if (itemGroups.has(date)) {
        let monthTransactions: TransactionModel[];
        for (const key of itemGroups.keys()) {
          if (key === date) {
            monthTransactions = itemGroups.get(key);
            break;
          }
        }

        const item = new AmountByMonth(date, monthString, 0);

        switch (this.type) {
          case TransactionType.Any:
            {
              for (const transaction of monthTransactions) {
                if (transaction.fromAccountId) {
                  item.amount -= transaction.amount;
                  spentSum += transaction.amount;
                } else {
                  item.amount += transaction.amount;
                  depositedSum += transaction.amount;
                }
              }
              balanceSum += item.amount;
            }
            break;
          case TransactionType.Expense:
            {
              item.amount -= monthTransactions.map((x) => x.amount).reduce((a, b) => a + b, 0);
              spentSum += item.amount;
            }
            break;
          case TransactionType.Deposit:
            {
              item.amount += monthTransactions.map((x) => x.amount).reduce((a, b) => a + b, 0);
              depositedSum += item.amount;
            }
            break;
          case TransactionType.Saving:
            {
              const movedFromMain = monthTransactions.filter((x) => x.fromAccountId === this.mainAccountId);
              const movedToMain = monthTransactions.filter((x) => x.toAccountId === this.mainAccountId);

              item.amount += movedFromMain.map((x) => x.amount).reduce((a, b) => a + b, 0);
              item.amount -= movedToMain.map((x) => x.amount).reduce((a, b) => a + b, 0);

              savedSum += item.amount;
            }
            break;
        }

        item.amount = parseFloat(item.amount.toFixed(2));

        items.push(item);
      } else {
        items.push(new AmountByMonth(date, monthString, 0));
      }

      fromDate.setMonth(fromDate.getMonth() + 1);
    }

    switch (this.type) {
      case TransactionType.Any:
        this.balanceAverage = balanceSum / monthsDiff;
        this.spentAverage = Math.abs(spentSum) / monthsDiff;
        this.depositedAverage = depositedSum / monthsDiff;
        break;
      case TransactionType.Expense:
        this.spentAverage = Math.abs(spentSum) / monthsDiff;
        break;
      case TransactionType.Deposit:
        this.depositedAverage = depositedSum / monthsDiff;
        break;
      case TransactionType.Saving:
        this.savedAverage = savedSum / monthsDiff;
        break;
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
        case TransactionType.Saving:
          {
            const colors = new Array<string>();
            for (const amount of amounts) {
              colors.push(amount < 0 ? expenseColor : depositColor);
            }

            this.chart.data.datasets[0].backgroundColor = colors;
          }
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
  }

  groupBy(list: TransactionModel[], keyGetter: { (x: TransactionModel): string; (arg0: TransactionModel): any }) {
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
