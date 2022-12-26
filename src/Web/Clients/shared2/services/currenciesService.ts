import { ErrorLogger } from "./errorLogger";
import { HttpProxy } from "../services/httpProxy";
import { LocalStorageBase } from "../utils/localStorageBase";
import { DateHelper } from "../utils/dateHelper";
import Variables from "$lib/variables";

export class CurrenciesService {
  private readonly httpProxy = new HttpProxy();
  private readonly localStorage = new LocalStorageBase();
  private readonly logger: ErrorLogger;
  private readonly key = "currencyRates";

  private currencyRates: Map<string, number> | null = null;

  constructor(application: string) {
    this.logger = new ErrorLogger(application);
  }

  async loadRates(): Promise<void> {
    try {
      const today = DateHelper.format(new Date());

      const currencyRates = await this.httpProxy.ajax<string>(
        `${Variables.urls.gateway}/core/api/currencies/${today}`
      );

      this.localStorage.set(this.key, window.JSON.stringify(currencyRates));

      this.currencyRates = <Map<string, number>>(
        (<unknown>new Map(Object.entries(currencyRates)))
      );
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
        : window.JSON.parse(this.localStorage.get(this.key));

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
      if (typeof amount === "string") {
        amount = parseFloat(amount);
      }

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
        const currencyRates = this.localStorage.get(this.key);
        if (currencyRates) {
          this.currencyRates = <Map<string, number>>(
            (<unknown>new Map(Object.entries(window.JSON.parse(currencyRates))))
          );
          return <number>this.currencyRates.get(currency);
        }

        throw new Error(`Could not get rate for currency ${currency}`);
      }

      return <number>this.currencyRates.get(currency);
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
