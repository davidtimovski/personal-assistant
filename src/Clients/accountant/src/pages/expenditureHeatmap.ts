import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { TransactionsService } from "services/transactionsService";
import { AccountsService } from "services/accountsService";
import { I18N } from "aurelia-i18n";
import { LocalStorage } from "utils/localStorage";
import { HeatmapDay } from "models/viewmodels/heatmapDay";
import { TransactionModel } from "models/entities/transaction";
import { DateHelper } from "../../../shared/src/utils/dateHelper";
import { HeatmapExpense } from "models/viewmodels/heatmapExpense";

@inject(Router, TransactionsService, AccountsService, I18N, LocalStorage)
export class ExpenditureHeatmap {
  private days: Array<HeatmapDay>;
  private selectedDay: HeatmapDay;
  private selectedExpenditureCaret = 0;
  private maxSpent: number;
  private minSpent: number;
  private loaded = false;
  private readonly colors = [
    "#241432",
    "#2b1637",
    "#33183c",
    "#381a40",
    "#401b44",
    "#481c48",
    "#4e1d4b",
    "#561e4f",
    "#5c1e51",
    "#641f54",
    "#6d1f56",
    "#731f58",
    "#7b1f59",
    "#841e5a",
    "#8b1d5b",
    "#931c5b",
    "#9a1b5b",
    "#a3195b",
    "#ab185a",
    "#b21758",
    "#ba1656",
    "#c11754",
    "#c81951",
    "#cf1e4d",
    "#d5224a",
    "#db2946",
    "#e03143",
    "#e43841",
    "#e8403e",
    "#eb483e",
    "#ee523f",
    "#f05c42",
    "#f16445",
    "#f26d4b",
    "#f37450",
    "#f47d57",
    "#f4865e",
    "#f58d64",
    "#f5966c",
    "#f69e75",
    "#f6a47c",
    "#f6ad85",
    "#f6b38d",
    "#f6bb97",
    "#f7c2a2",
    "#f7c9aa",
    "#f7d0b5",
    "#f8d7c0",
    "#f9ddc9",
    "#f9e5d4",
  ];
  private currency: string;

  constructor(
    private readonly router: Router,
    private readonly transactionsService: TransactionsService,
    private readonly accountsService: AccountsService,
    private readonly i18n: I18N,
    private readonly localStorage: LocalStorage
  ) {}

  activate() {
    this.currency = this.localStorage.getCurrency();
  }

  async attached() {
    const now = new Date();
    const month = now.getMonth() - 1;
    const aMonthAgo = new Date(now.getFullYear(), month, now.getDate());
    const weekday = aMonthAgo.getDay();

    if (weekday > 1 || weekday === 0) {
      const diff = weekday - 1;
      aMonthAgo.setDate(aMonthAgo.getDate() - diff);
    }

    const fromDate = new Date(
      aMonthAgo.getFullYear(),
      aMonthAgo.getMonth(),
      aMonthAgo.getDate()
    );

    const todayString = DateHelper.format(now);

    const days = new Array<HeatmapDay>();
    while (aMonthAgo < now) {
      for (let i = 0; i < 7; i++) {
        const dateString = DateHelper.format(aMonthAgo);
        const isToday = dateString === todayString;

        const day = new HeatmapDay(
          aMonthAgo.getDate(),
          dateString,
          this.formatDate(aMonthAgo),
          isToday,
          0,
          0,
          null,
          null,
          null
        );

        days.push(day);

        if (isToday) {
          this.selectedDay = day;
        }

        aMonthAgo.setDate(aMonthAgo.getDate() + 1);
      }
    }
    this.days = days;

    const mainAccountId = await this.accountsService.getMainId();
    if (!mainAccountId) {
      return;
    }

    this.transactionsService
      .getExpendituresFrom(mainAccountId, fromDate, this.currency)
      .then((transactions: Array<TransactionModel>) => {
        let maxSpent = 0;
        let minSpent = 10000000;

        for (const day of this.days) {
          transactions.forEach((x) => {
            if (day.date === x.date.slice(0, 10)) {
              const category = x.categoryName || this.i18n.tr("uncategorized");
              const trimmedDescription = this.formatDescription(
                x.description,
                x.isEncrypted
              );

              day.spent += x.amount;
              day.expenditures.push(
                new HeatmapExpense(x.id, category, trimmedDescription, x.amount)
              );
            }
          });

          if (day.spent > maxSpent) {
            maxSpent = day.spent;
          }
          if (day.spent < minSpent) {
            minSpent = day.spent;
          }
        }

        for (const day of this.days) {
          day.spentPercentage = (day.spent / maxSpent) * 100;

          if (day.spent === maxSpent) {
            day.backgroundColor = this.colors[49];
            day.textColor = "initial";
          } else {
            const index = Math.floor(day.spentPercentage / 2);
            day.backgroundColor = this.colors[index];
            day.textColor = index < 30 ? "#eee" : "initial";
          }
        }

        this.maxSpent = maxSpent;
        this.minSpent = minSpent;
        this.loaded = true;
      });
  }

  formatDate(date: Date): string {
    const day = date.getDate();
    const month = this.i18n.tr(`months.${date.getMonth()}`);
    return `${day} ${month}`;
  }

  formatDescription(description: string, isEncrypted: boolean): string {
    if (isEncrypted) {
      return this.i18n.tr("encryptedPlaceholder");
    }

    if (!description) {
      return "";
    }

    const length = 25;
    if (description.length <= length) {
      return description;
    }

    return description.substring(0, length - 2) + "..";
  }

  select(day: HeatmapDay) {
    this.selectedDay = day;
    this.selectedExpenditureCaret = day.spentPercentage;
  }

  viewTransaction(id: number) {
    this.router.navigateToRoute("transaction", {
      id: id,
      fromExpenditureHeatmap: true,
    });
  }

  @computedFrom("selectedDay")
  get getSelectedDayDate() {
    return this.selectedDay?.date;
  }
}
