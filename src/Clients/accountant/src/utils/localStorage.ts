import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";

export class LocalStorage extends LocalStorageCurrencies {
  private readonly defaultLastSynced = "1970-01-01T00:00:00.000Z";

  initialize() {
    super.initialize();

    const lastSynced = window.localStorage.getItem("lastSynced");
    if (!lastSynced) {
      this.setLastSynced(this.defaultLastSynced);
    }

    const showUpcomingExpensesOnDashboard = window.localStorage.getItem(
      "showUpcomingExpensesOnDashboard"
    );
    if (!showUpcomingExpensesOnDashboard) {
      this.setShowUpcomingExpensesOnDashboard(true);
    }

    const showDebtOnDashboard = window.localStorage.getItem(
      "showDebtOnDashboard"
    );
    if (!showDebtOnDashboard) {
      this.setShowDebtOnDashboard(true);
    }
  }

  getLastSynced(): string {
    const lasySynced = window.localStorage.getItem("lastSynced");
    return lasySynced ? lasySynced : this.defaultLastSynced;
  }
  setLastSynced(lastSynced: string) {
    window.localStorage.setItem("lastSynced", lastSynced);
  }

  getShowUpcomingExpensesOnDashboard(): boolean {
    const showUpcomingExpensesOnDashboard = window.localStorage.getItem(
      "showUpcomingExpensesOnDashboard"
    );
    return showUpcomingExpensesOnDashboard === "true";
  }
  setShowUpcomingExpensesOnDashboard(showUpcomingExpensesOnDashboard: boolean) {
    window.localStorage.setItem(
      "showUpcomingExpensesOnDashboard",
      showUpcomingExpensesOnDashboard.toString()
    );
  }

  getShowDebtOnDashboard(): boolean {
    const showDebtOnDashboard = window.localStorage.getItem(
      "showDebtOnDashboard"
    );
    return showDebtOnDashboard === "true";
  }
  setShowDebtOnDashboard(showDebtOnDashboard: boolean) {
    window.localStorage.setItem(
      "showDebtOnDashboard",
      showDebtOnDashboard.toString()
    );
  }
}
