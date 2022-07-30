export class EarlyRetirementAnswers {
  age: number;
  capital: AmountModel;
  savedPerMonth: AmountModel;
  savingInterestRate: number;
  eligibleForPension = false;
  pensionAge: number;
  pensionPerMonth: AmountModel;
  hasLifeInsurance = false;
  lifeInsuranceAge: number;
  lifeInsuranceReturn: AmountModel;
  upcomingExpenses: LargeUpcomingExpense[];
  retirementIncome: AmountModel;

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
  amount: number;

  constructor(public name: string, public iconClass: string, public currency: string) {}
}

export class AmountModel {
  amount: number;

  constructor(public currency: string) {}
}

export class SummaryItem {
  children: SummaryItem[] = [];

  constructor(public contentHtml: string) {}
}
