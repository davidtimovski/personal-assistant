import { inject } from "aurelia-framework";
import { CurrenciesService } from "../../../../shared/src/services/currenciesService";
import autocomplete, { AutocompleteResult } from "autocompleter";
import { CurrencySuggestion } from "../../../../shared/src/models/viewmodels/currencySuggestion";

@inject(CurrenciesService)
export class StockPriceInput {
  private currencySuggestions: Array<CurrencySuggestion>;
  private changing = false;
  private autocomplete: AutocompleteResult;
  private selectCurrencyInput: HTMLInputElement;
  private stockPriceInput: HTMLInputElement;
  private fund: any;

  constructor(private readonly currenciesService: CurrenciesService) {}

  activate(fund: any) {
    this.fund = fund;
  }

  attached() {
    const currencies = this.currenciesService.getCurrencies();
    this.currencySuggestions = currencies.map((currency: string) => {
      return new CurrencySuggestion(currency, currency, null);
    });

    this.autocomplete = autocomplete({
      input: this.selectCurrencyInput,
      minLength: 1,
      fetch: (text: string, update: (items: CurrencySuggestion[]) => void) => {
        const suggestions = this.currencySuggestions.filter((i) =>
          i.name.toUpperCase().startsWith(text.toUpperCase())
        );
        update(suggestions);
      },
      onSelect: (suggestion: CurrencySuggestion) => {
        this.changing = false;
        this.fund.currency = suggestion.name;
      },
      className: "currency-autocomplete-customizations",
    });

    if (!this.fund.id && !this.fund.debtId) {
      this.stockPriceInput.focus();
    }
  }

  toggleChangeCurrency() {
    if (!this.changing) {
      this.selectCurrencyInput.value = "";
      this.changing = true;

      // Focus after input element appears
      window.setTimeout(() => {
        this.selectCurrencyInput.focus();
      }, 0);
    } else {
      this.changing = false;
    }
  }
}
