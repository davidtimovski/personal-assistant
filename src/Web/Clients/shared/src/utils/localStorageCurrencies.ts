import { LocalStorageBase } from "./localStorageBase";

export class LocalStorageCurrencies extends LocalStorageBase {
  private readonly defaultCurrency = "EUR";
  private readonly defaultCurrencyRates = { EUR: 1 };

  initialize() {
    super.initialize();

    const currency = window.localStorage.getItem("currency");
    if (!currency) {
      this.setCurrency(this.defaultCurrency);
    }

    const currencyRates = window.localStorage.getItem("currencyRates");
    if (!currencyRates) {
      this.setCurrencyRates(JSON.stringify(this.defaultCurrencyRates));
    }
  }

  getCurrencyRates(): string {
    return window.localStorage.getItem("currencyRates");
  }
  setCurrencyRates(currencyRates: string) {
    window.localStorage.setItem("currencyRates", currencyRates);
  }

  getCurrency(): string {
    const currency = window.localStorage.getItem("currency");
    if (!currency) {
      return this.defaultCurrency;
    }
    return currency;
  }
  setCurrency(currency: string) {
    window.localStorage.setItem("currency", currency);
  }
}
