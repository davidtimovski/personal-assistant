export class FireAnswers {
  public age: number;
  public capital: AmountModel;
  public savedPerMonth: AmountModel;
  public savingInterestRate: number;
  public eligibleForPension = false;
  public pensionAge: number;
  public pensionPerMonth: AmountModel;
  public hasLifeInsurance = false;
  public lifeInsuranceAge: number;
  public lifeInsuranceReturn: AmountModel;
  public upcomingExpenses: Array<LargeUpcomingExpense>;
  public retirementIncome: AmountModel;

  constructor(upcomingExpenses: LargeUpcomingExpense[], currency: string) {
    this.capital = new AmountModel(currency);
    this.savedPerMonth = new AmountModel(currency);
    this.pensionPerMonth = new AmountModel(currency);
    this.lifeInsuranceReturn = new AmountModel(currency);
    this.upcomingExpenses = upcomingExpenses;
    this.retirementIncome = new AmountModel(currency);
  }
}

export class LargeUpcomingExpense {
  public amount: number;

  constructor(public name: string, public iconClass: string, public currency: string) {}
}

export class AmountModel {
  public amount: number;

  constructor(public currency: string) {}
}

export class SummaryItem {
  public children: SummaryItem[] = [];

  constructor(public contentHtml: string) {}
}
