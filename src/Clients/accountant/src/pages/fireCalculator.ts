import { inject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

import { CurrenciesService } from "../../../shared/src/services/currenciesService";

import { LocalStorage } from "utils/localStorage";
import { AmountModel, FireAnswers, LargeUpcomingExpense, SummaryItem } from "models/viewmodels/fireAnswers";
import { MoneyFormattingHelper } from "utils/moneyFormattingHelper";
import { AccountsService } from "services/accountsService";

@inject(LocalStorage, I18N, CurrenciesService, AccountsService)
export class FireCalculator {
  private answers: FireAnswers;
  private currentSection = "start";
  private sections = [
    "start",
    "age",
    "capital",
    "saving",
    "pension-eligibility",
    "pension",
    "have-life-insurance",
    "life-insurance",
    "upcoming-expenses",
    "retirement-income",
    "summary",
    "result",
  ];
  private currency: string;
  private ageIsInvalid: boolean;
  private pensionAgeIsInvalid: boolean;
  private pensionPerMonthIsInvalid: boolean;
  private lifeInsuranceAgeIsInvalid: boolean;
  private lifeInsuranceReturnIsInvalid: boolean;
  private preferredRetirementIncomeIsInvalid: boolean;
  private fireRetirementAge: number;
  private summaryItems: SummaryItem[];

  constructor(
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N,
    private readonly currenciesService: CurrenciesService,
    private readonly accountsService: AccountsService
  ) {
    this.currency = this.localStorage.getCurrency();

    this.answers = new FireAnswers(
      [
        new LargeUpcomingExpense(this.i18n.tr("fireCalculator.home"), "fas fa-home", this.currency),
        new LargeUpcomingExpense(this.i18n.tr("fireCalculator.car"), "fas fa-car", this.currency),
        new LargeUpcomingExpense(this.i18n.tr("fireCalculator.kids"), "fas fa-baby", this.currency),
      ],
      this.currency
    );
  }

  attached() {
    this.accountsService.getAllWithBalance(this.currency).then((accounts) => {
      const balance = accounts.map((x) => x.balance).reduce((a: number, b: number) => a + b, 0);
      this.answers.capital.amount = Math.floor(balance);
    });

    this.accountsService.getAverageMonthlySavingsFromThePastYear(this.currency).then((savingsPerMonth) => {
      this.answers.savedPerMonth.amount = Math.floor(savingsPerMonth);
    });
  }

  addUpcomingExpense() {
    this.answers.upcomingExpenses.push(new LargeUpcomingExpense("", "fas fa-wallet", this.currency));
  }

  removeUpcomingExpense(upcomingExpense: LargeUpcomingExpense) {
    this.answers.upcomingExpenses.splice(this.answers.upcomingExpenses.indexOf(upcomingExpense), 1);
  }

  start() {
    this.currentSection = "age";
  }

  goToCapital() {
    this.ageIsInvalid = false;

    if (!this.answers.age || this.answers.age.toString().trim() === "") {
      this.ageIsInvalid = true;
      return;
    }

    this.currentSection = "capital";
  }

  goToSaving() {
    this.currentSection = "saving";
  }

  goToPensionEligibility() {
    this.currentSection = "pension-eligibility";
  }

  goToPension() {
    if (this.answers.eligibleForPension) {
      this.currentSection = "pension";
    } else {
      this.currentSection = "have-life-insurance";
    }
  }

  goToHaveLifeInsurance() {
    this.pensionAgeIsInvalid = false;
    this.pensionPerMonthIsInvalid = false;

    if (!this.answers.pensionAge || this.answers.pensionAge.toString().trim() === "") {
      this.pensionAgeIsInvalid = true;
      return;
    }
    if (!this.answers.pensionPerMonth.amount || this.answers.pensionPerMonth.amount.toString().trim() === "") {
      this.pensionPerMonthIsInvalid = true;
      return;
    }

    this.currentSection = "have-life-insurance";
  }

  goToLifeInsurance() {
    if (this.answers.hasLifeInsurance) {
      this.currentSection = "life-insurance";
    } else {
      this.currentSection = "upcoming-expenses";
    }
  }

  goToUpcomingExpenses() {
    this.lifeInsuranceAgeIsInvalid = false;
    this.lifeInsuranceReturnIsInvalid = false;

    if (!this.answers.lifeInsuranceAge || this.answers.lifeInsuranceAge.toString().trim() === "") {
      this.lifeInsuranceAgeIsInvalid = true;
      return;
    }
    if (!this.answers.lifeInsuranceReturn.amount || this.answers.lifeInsuranceReturn.amount.toString().trim() === "") {
      this.lifeInsuranceReturnIsInvalid = true;
      return;
    }

    this.currentSection = "upcoming-expenses";
  }

  goToRetirementIncome() {
    this.answers.upcomingExpenses = this.answers.upcomingExpenses.filter((x) => !!x.amount);

    this.currentSection = "retirement-income";
  }

  goToSummary() {
    this.preferredRetirementIncomeIsInvalid = false;

    if (!this.answers.retirementIncome.amount || this.answers.retirementIncome.amount.toString().trim() === "") {
      this.preferredRetirementIncomeIsInvalid = true;
      return;
    }

    this.summaryItems = [];

    this.summaryItems.push(new SummaryItem(this.i18n.tr("fireCalculator.summaryItem1", { age: this.answers.age })));

    if (this.answers.capital.amount && parseFloat(this.answers.capital.amount.toString()) > 0) {
      this.summaryItems.push(
        new SummaryItem(
          this.i18n.tr("fireCalculator.summaryItem2a", {
            capital: MoneyFormattingHelper.format(this.answers.capital.amount, this.answers.capital.currency),
          })
        )
      );
    } else {
      this.summaryItems.push(new SummaryItem(this.i18n.tr("fireCalculator.summaryItem2b")));
    }

    if (this.answers.savedPerMonth.amount && parseFloat(this.answers.savedPerMonth.amount.toString()) > 0) {
      this.summaryItems.push(
        new SummaryItem(
          this.i18n.tr("fireCalculator.summaryItem3a", {
            savedPerMonth: MoneyFormattingHelper.format(
              this.answers.savedPerMonth.amount,
              this.answers.savedPerMonth.currency
            ),
            savingInterestRate: this.answers.savingInterestRate,
          })
        )
      );
    } else {
      this.summaryItems.push(new SummaryItem(this.i18n.tr("fireCalculator.summaryItem3b")));
    }

    if (this.answers.eligibleForPension) {
      this.summaryItems.push(
        new SummaryItem(
          this.i18n.tr("fireCalculator.summaryItem4a", {
            pensionAge: this.answers.pensionAge,
            pensionPerMonth: MoneyFormattingHelper.format(
              this.answers.pensionPerMonth.amount,
              this.answers.pensionPerMonth.currency
            ),
          })
        )
      );
    } else {
      this.summaryItems.push(new SummaryItem(this.i18n.tr("fireCalculator.summaryItem4b")));
    }

    if (this.answers.hasLifeInsurance) {
      this.summaryItems.push(
        new SummaryItem(
          this.i18n.tr("fireCalculator.summaryItem5a", {
            lifeInsuranceReturn: MoneyFormattingHelper.format(
              this.answers.lifeInsuranceReturn.amount,
              this.answers.lifeInsuranceReturn.currency
            ),
            lifeInsuranceAge: this.answers.lifeInsuranceAge,
          })
        )
      );
    } else {
      this.summaryItems.push(new SummaryItem(this.i18n.tr("fireCalculator.summaryItem5b")));
    }

    if (this.answers.upcomingExpenses.length > 0) {
      const expensesSummaryItem = new SummaryItem(this.i18n.tr("fireCalculator.summaryItem6"));

      for (const expense of this.answers.upcomingExpenses) {
        expensesSummaryItem.children.push(
          new SummaryItem(
            this.i18n.tr("fireCalculator.summaryItem6a", {
              amount: MoneyFormattingHelper.format(expense.amount, expense.currency),
              expense: expense.name,
            })
          )
        );
      }

      this.summaryItems.push(expensesSummaryItem);
    }

    this.summaryItems.push(
      new SummaryItem(
        this.i18n.tr("fireCalculator.summaryItem7", {
          retirementIncome: MoneyFormattingHelper.format(
            this.answers.retirementIncome.amount,
            this.answers.retirementIncome.currency
          ),
        })
      )
    );

    this.summaryItems.push(new SummaryItem(this.i18n.tr("fireCalculator.summaryItem8")));

    this.currentSection = "summary";
  }

  back() {
    const currentIndex = this.sections.indexOf(this.currentSection);

    switch (this.currentSection) {
      case "have-life-insurance":
        {
          if (this.answers.eligibleForPension) {
            this.currentSection = this.sections[currentIndex - 1];
          } else {
            this.currentSection = this.sections[currentIndex - 2];
          }
        }
        break;
      case "upcoming-expenses":
        {
          if (this.answers.hasLifeInsurance) {
            this.currentSection = this.sections[currentIndex - 1];
          } else {
            this.currentSection = this.sections[currentIndex - 2];
          }
        }
        break;
      default:
        this.currentSection = this.sections[currentIndex - 1];
    }
  }

  calculate() {
    const retirementIncomeX25 = this.getConvertedValueOrZero(this.answers.retirementIncome) * 12 * 25;
    const largeUpcomingExpensesSum = this.answers.upcomingExpenses
      .map((x) => this.getConvertedValueOrZero(x))
      .reduce((a: number, b: number) => a + b, 0);
    const requiredCapital = retirementIncomeX25 + largeUpcomingExpensesSum;

    const pensionAge = this.getIntValueOrZero(this.answers.pensionAge);
    const pensionPerMonth = this.getConvertedValueOrZero(this.answers.pensionPerMonth);

    const lifeInsuranceAge = this.getIntValueOrZero(this.answers.lifeInsuranceAge);
    const lifeInsuranceReturn = this.getConvertedValueOrZero(this.answers.lifeInsuranceReturn);

    const monthlyInflationFactor = 1 - 0.017 / 12;
    const savingInterestMonthlyFactor = 1 + this.getFloatValueOrZero(this.answers.savingInterestRate) / 100 / 12;
    const savedPerMonth = this.getConvertedValueOrZero(this.answers.savedPerMonth);

    let currentAge = parseInt(this.answers.age.toString(), 10);
    let saved = this.getConvertedValueOrZero(this.answers.capital);

    while (true) {
      if (saved >= requiredCapital) {
        break;
      }

      for (let i = 0; i < 12; i++) {
        if (this.answers.eligibleForPension && currentAge >= pensionAge) {
          saved += pensionPerMonth;
        }

        if (this.answers.hasLifeInsurance && currentAge === lifeInsuranceAge) {
          saved += lifeInsuranceReturn;
        }

        saved *= monthlyInflationFactor;
        saved *= savingInterestMonthlyFactor;
        saved += savedPerMonth;
      }

      currentAge++;

      if (currentAge === 100) {
        break;
      }
    }

    this.fireRetirementAge = currentAge;

    const currentIndex = this.sections.indexOf(this.currentSection);
    this.currentSection = this.sections[currentIndex + 1];
  }

  private getIntValueOrZero(value: number): number {
    if (!value || value.toString().trim() === "") {
      return 0;
    }

    return parseInt(value.toString(), 10);
  }

  private getFloatValueOrZero(value: number): number {
    if (!value || value.toString().trim() === "") {
      return 0;
    }

    return parseFloat(value.toString());
  }

  private getConvertedValueOrZero(value: AmountModel): number {
    if (!value.amount || value.amount.toString().trim() === "") {
      return 0;
    }

    const parsed = parseFloat(value.amount.toString());

    return this.currenciesService.convert(parsed, value.currency, this.currency);
  }
}
