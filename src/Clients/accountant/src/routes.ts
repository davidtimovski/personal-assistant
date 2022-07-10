import { PLATFORM } from "aurelia-pal";

import { Language } from "../../shared/src/models/enums/language";

export default [
  {
    route: ["", Language.English, Language.Macedonian],
    redirect: "dashboard",
  },
  {
    route: "dashboard",
    name: "dashboard",
    moduleId: PLATFORM.moduleName("./pages/dashboard"),
  },
  {
    route: "new-transaction/:type",
    name: "newTransaction",
    moduleId: PLATFORM.moduleName("./pages/newTransaction"),
  },
  {
    route: "new-transaction/:type/:debtId",
    name: "newTransaction",
    moduleId: PLATFORM.moduleName("./pages/newTransaction"),
  },
  {
    route: "transaction/:id",
    name: "transaction",
    moduleId: PLATFORM.moduleName("./pages/transaction"),
  },
  {
    route: "transaction/:id/:fromExpenditureHeatmap",
    name: "transaction",
    moduleId: PLATFORM.moduleName("./pages/transaction"),
  },
  {
    route: "edit-transaction/:id",
    name: "editTransaction",
    moduleId: PLATFORM.moduleName("./pages/editTransaction"),
  },
  {
    route: "balanceAdjustment",
    name: "balanceAdjustment",
    moduleId: PLATFORM.moduleName("./pages/balanceAdjustment"),
  },
  {
    route: "automatic-transactions",
    name: "automaticTransactions",
    moduleId: PLATFORM.moduleName("./pages/automaticTransactions"),
  },
  {
    route: "automatic-transactions/:editedId",
    name: "automaticTransactionsEdited",
    moduleId: PLATFORM.moduleName("./pages/automaticTransactions"),
  },
  {
    route: "automatic-transactions-expense/:id",
    name: "editAutomaticTransaction",
    moduleId: PLATFORM.moduleName("./pages/editAutomaticTransaction"),
  },
  {
    route: "transactions",
    name: "transactions",
    moduleId: PLATFORM.moduleName("./pages/transactions"),
  },
  {
    route: "transactions/:editedId",
    name: "transactionsEdited",
    moduleId: PLATFORM.moduleName("./pages/transactions"),
  },
  {
    route: "upcoming-expenses",
    name: "upcomingExpenses",
    moduleId: PLATFORM.moduleName("./pages/upcomingExpenses"),
  },
  {
    route: "upcoming-expenses/:editedId",
    name: "upcomingExpensesEdited",
    moduleId: PLATFORM.moduleName("./pages/upcomingExpenses"),
  },
  {
    route: "edit-upcoming-expense/:id",
    name: "editUpcomingExpense",
    moduleId: PLATFORM.moduleName("./pages/editUpcomingExpense"),
  },
  {
    route: "debt",
    name: "debt",
    moduleId: PLATFORM.moduleName("./pages/debt"),
  },
  {
    route: "debt/:editedId",
    name: "debtEdited",
    moduleId: PLATFORM.moduleName("./pages/debt"),
  },
  {
    route: "edit-debt/:id",
    name: "editDebt",
    moduleId: PLATFORM.moduleName("./pages/editDebt"),
  },
  {
    route: "categories",
    name: "categories",
    moduleId: PLATFORM.moduleName("./pages/categories"),
  },
  {
    route: "categories/:editedId",
    name: "categoriesEdited",
    moduleId: PLATFORM.moduleName("./pages/categories"),
  },
  {
    route: "edit-category/:id",
    name: "editCategory",
    moduleId: PLATFORM.moduleName("./pages/editCategory"),
  },
  {
    route: "accounts",
    name: "accounts",
    moduleId: PLATFORM.moduleName("./pages/accounts"),
  },
  {
    route: "accounts/:editedId",
    name: "accountsEdited",
    moduleId: PLATFORM.moduleName("./pages/accounts"),
  },
  {
    route: "accounts/:editedId/:editedId2",
    name: "accountsEdited",
    moduleId: PLATFORM.moduleName("./pages/accounts"),
  },
  {
    route: "transfer-funds",
    name: "transferFunds",
    moduleId: PLATFORM.moduleName("./pages/transferFunds"),
  },
  {
    route: "edit-account/:id",
    name: "editAccount",
    moduleId: PLATFORM.moduleName("./pages/editAccount"),
  },
  {
    route: "pie-chart-report",
    name: "pieChartReport",
    moduleId: PLATFORM.moduleName("./pages/pieChartReport"),
  },
  {
    route: "bar-chart-report",
    name: "barChartReport",
    moduleId: PLATFORM.moduleName("./pages/barChartReport"),
  },
  {
    route: "expenditure-heatmap",
    name: "expenditureHeatmap",
    moduleId: PLATFORM.moduleName("./pages/expenditureHeatmap"),
  },
  {
    route: "early-retirement-calculator",
    name: "earlyRetirementCalculator",
    moduleId: PLATFORM.moduleName("./pages/earlyRetirementCalculator"),
  },
  {
    route: "export",
    name: "export",
    moduleId: PLATFORM.moduleName("./pages/export"),
  },
  {
    route: "total-sync",
    name: "totalSync",
    moduleId: PLATFORM.moduleName("./pages/totalSync"),
  },
  {
    route: "help",
    name: "help",
    moduleId: PLATFORM.moduleName("./pages/help"),
  },
  {
    route: "menu",
    name: "menu",
    moduleId: PLATFORM.moduleName("./pages/menu"),
  },
  {
    route: "preferences",
    name: "preferences",
    moduleId: PLATFORM.moduleName("./pages/preferences"),
  },
  {
    route: "not-found",
    name: "notFound",
    moduleId: PLATFORM.moduleName("./pages/notFound"),
  },
  {
    route: "signin-oidc",
    name: "signinOidc",
    moduleId: PLATFORM.moduleName("./pages/auth/signinOidc"),
    settings: {
      noAuth: true,
    },
  },
];
