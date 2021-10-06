import { inject, observable } from "aurelia-framework";
import { LocalStorage } from "utils/localStorage";

@inject(LocalStorage)
export class Preferences {
  @observable() private showUpcomingExpensesOnDashboard: boolean;
  @observable() private showDebtOnDashboard: boolean;

  constructor(private readonly localStorage: LocalStorage) {}

  activate() {
    this.showUpcomingExpensesOnDashboard = this.localStorage.getShowUpcomingExpensesOnDashboard();
    this.showDebtOnDashboard = this.localStorage.getShowDebtOnDashboard();
  }

  showUpcomingExpensesOnDashboardChanged() {
    this.localStorage.setShowUpcomingExpensesOnDashboard(this.showUpcomingExpensesOnDashboard);
  }

  showDebtOnDashboardChanged() {
    this.localStorage.setShowDebtOnDashboard(this.showDebtOnDashboard);
  }
}
