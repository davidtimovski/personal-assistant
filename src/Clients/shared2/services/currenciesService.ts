import { ErrorLogger } from "./errorLogger";
import { HttpProxy } from "../services/httpProxy";
import { LocalStorageCurrencies } from "../utils/localStorageCurrencies";
import { DateHelper } from "../utils/dateHelper";
import Variables from "$lib/variables";

export class CurrenciesService {
  private currencyRates: any;

  private readonly httpProxy: HttpProxy;
  private readonly localStorage = new LocalStorageCurrencies();
  private readonly logger: ErrorLogger;

  constructor(application: string, client: string) {
    this.httpProxy = new HttpProxy(client);
    this.logger = new ErrorLogger(application, client);
  }

  async loadRates(): Promise<void> {
    try {
      const today = DateHelper.format(new Date());

      this.currencyRates = await this.httpProxy.ajax<string>(
        `${Variables.urls.api}/api/currencies/${today}`
      );

      this.localStorage.setCurrencyRates(JSON.stringify(this.currencyRates));
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  getCurrencies(): string[] {
    try {
      const currencies = new Array<string>();

      const currencyRates = this.currencyRates
        ? this.currencyRates
        : JSON.parse(this.localStorage.getCurrencyRates());

      for (const currency in currencyRates) {
        currencies.push(currency);
      }

      return currencies.sort();
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  convert(amount: number, fromCurrency: string, toCurrency: string): number {
    try {
      amount = parseFloat(<any>amount);

      if (fromCurrency === toCurrency) {
        return amount;
      }

      const fromRate = this.getRate(fromCurrency);
      const amountFixed = this.toFixed(amount, 2);
      const eurAmount = parseFloat((amountFixed / fromRate).toFixed(2));

      const toRate = this.getRate(toCurrency);
      if (toCurrency === "MKD") {
        return parseFloat((eurAmount * toRate).toFixed(0));
      }

      return parseFloat((eurAmount * toRate).toFixed(2));
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  release() {
    this.httpProxy.release();
  }

  private getRate(currency: string): number {
    try {
      if (!this.currencyRates) {
        const currencyRates = this.localStorage.getCurrencyRates();
        if (currencyRates) {
          this.currencyRates = JSON.parse(currencyRates);
          return this.currencyRates[currency];
        }

        throw new Error(`Could not get rate for currency ${currency}`);
      }

      return this.currencyRates[currency];
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  private toFixed(num: number, fixed: number): number {
    const re = new RegExp("^-?\\d+(?:.\\d{0," + (fixed || -1) + "})?");
    let matches = num.toString().match(re);

    if (!matches) {
      throw new Error("Could not match");
    }

    return parseFloat(matches[0]);
  }
}
