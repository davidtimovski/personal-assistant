import { inject } from "aurelia-framework";
import autocomplete, { AutocompleteResult } from "autocompleter";

import { CurrenciesService } from "../../../../shared/src/services/currenciesService";
import { CurrencySuggestion } from "../../../../shared/src/models/viewmodels/currencySuggestion";

import { LocalStorage } from "utils/localStorage";

@inject(CurrenciesService, LocalStorage)
export class CurrencySelector {
  private currency: string;
  private currencySuggestions: Array<CurrencySuggestion>;
  private changing = false;
  private autocomplete: AutocompleteResult;
  private selectCurrencyInput: HTMLInputElement;

  constructor(private readonly currenciesService: CurrenciesService, private readonly localStorage: LocalStorage) {}

  activate() {
    this.currency = this.localStorage.getCurrency();
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
        const suggestions = this.currencySuggestions.filter((i) => i.name.toUpperCase().startsWith(text.toUpperCase()));
        update(suggestions);
      },
      onSelect: (suggestion: CurrencySuggestion) => {
        this.changing = false;
        this.currency = suggestion.name;

        this.localStorage.setCurrency(this.currency);
      },
      className: "currency-autocomplete-customizations",
    });
  }

  toggleChangeCurrency() {
    if (!this.changing) {
      this.selectCurrencyInput.value = "";
      this.changing = true;
      this.selectCurrencyInput.focus();
    } else {
      this.changing = false;
    }
  }
}
