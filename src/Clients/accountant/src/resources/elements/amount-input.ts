import { inject, bindable, bindingMode } from "aurelia-framework";
import autocomplete, { AutocompleteResult } from "autocompleter";

import { CurrenciesService } from "../../../../shared/src/services/currenciesService";
import { CurrencySuggestion } from "../../../../shared/src/models/viewmodels/currencySuggestion";

@inject(CurrenciesService)
export class AmountInputCustomElement {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) amount: number;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) currency: string;
  @bindable({ defaultBindingMode: bindingMode.toView }) invalid: boolean;

  private currencySuggestions: Array<CurrencySuggestion>;
  private changing = false;
  private autocomplete: AutocompleteResult;
  private amountInput: HTMLInputElement;
  private selectCurrencyInput: HTMLInputElement;

  constructor(private readonly currenciesService: CurrenciesService) {}

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

        if (!this.amount) {
          window.setTimeout(() => {
            this.amountInput.focus();
          }, 0);
        }
      },
      className: "currency-autocomplete-customizations",
    });
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
