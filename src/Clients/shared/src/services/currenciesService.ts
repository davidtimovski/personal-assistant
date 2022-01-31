import { inject } from "aurelia-framework";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "./authService";
import { HttpProxyBase } from "../utils/httpProxyBase";
import { LocalStorageCurrencies } from "../utils/localStorageCurrencies";
import { DateHelper } from "../utils/dateHelper";
import { AlertEvents } from "../models/enums/alertEvents";

@inject(AuthService, HttpClient, EventAggregator, LocalStorageCurrencies)
export class CurrenciesService extends HttpProxyBase {
  private currencyRates: any;

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly localStorage: LocalStorageCurrencies
  ) {
    super(authService, httpClient, eventAggregator);
  }

  async loadRates(): Promise<void> {
    const today = DateHelper.format(new Date());

    this.currencyRates = await this.ajax<string>(`currencies/${today}`);

    this.localStorage.setCurrencyRates(JSON.stringify(this.currencyRates));
  }

  getCurrencies(): Array<string> {
    const currencies = new Array<string>();

    const currencyRates = this.currencyRates ? this.currencyRates : JSON.parse(this.localStorage.getCurrencyRates());

    for (const currency in currencyRates) {
      currencies.push(currency);
    }

    return currencies.sort();
  }

  convert(amount: number, fromCurrency: string, toCurrency: string): number {
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
  }

  private getRate(currency: string): number {
    if (!this.currencyRates) {
      const currencyRates = this.localStorage.getCurrencyRates();
      if (currencyRates) {
        this.currencyRates = JSON.parse(currencyRates);
        return this.currencyRates[currency];
      } else {
        this.eventAggregator.publish(AlertEvents.ShowError, "currencyConversionError");
      }
    }
    return this.currencyRates[currency];
  }

  private toFixed(num: number, fixed: number): number {
    const re = new RegExp("^-?\\d+(?:.\\d{0," + (fixed || -1) + "})?");
    return parseFloat(num.toString().match(re)[0]);
  }
}
