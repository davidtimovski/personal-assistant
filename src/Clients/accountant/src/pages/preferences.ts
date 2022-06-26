import { inject, observable } from "aurelia-framework";
import { LocalStorage } from "utils/localStorage";

@inject(LocalStorage)
export class Preferences {
  @observable() private showUpcomingExpensesOnDashboard: boolean;
  @observable() private showDebtOnDashboard: boolean;
  @observable() private mergeDebtPerPerson: boolean;

  constructor(private readonly localStorage: LocalStorage) {}

  activate() {
    this.showUpcomingExpensesOnDashboard = this.localStorage.showUpcomingExpensesOnDashboard;
    this.showDebtOnDashboard = this.localStorage.showDebtOnDashboard;
    this.mergeDebtPerPerson = this.localStorage.mergeDebtPerPerson;
  }

  showUpcomingExpensesOnDashboardChanged() {
    this.localStorage.setShowUpcomingExpensesOnDashboard(this.showUpcomingExpensesOnDashboard);
  }

  showDebtOnDashboardChanged() {
    this.localStorage.setShowDebtOnDashboard(this.showDebtOnDashboard);
  }

  mergeDebtPerPersonChanged() {
    this.localStorage.setMergeDebtPerPerson(this.mergeDebtPerPerson);
  }
}
