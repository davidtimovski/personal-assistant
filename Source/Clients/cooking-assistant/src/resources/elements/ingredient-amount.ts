import { inject, bindable, bindingMode, computedFrom } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

@inject(I18N)
export class IngredientAmountCustomElement {
  private readonly pluralizableUnits = ["cup", "tbsp", "tsp"];
  @bindable({ defaultBindingMode: bindingMode.oneWay }) amount: number;
  @bindable({ defaultBindingMode: bindingMode.oneWay }) unit: string;

  constructor(private readonly i18n: I18N) {}

  @computedFrom("amount", "unit")
  get amountLabel(): string {
    if (!this.amount) {
      return "";
    }

    const amount = this.decimalToFractionString(this.amount);
    if (!this.unit) {
      return amount;
    }

    const unit =
      this.amount > 1 && this.pluralizableUnits.includes(this.unit)
        ? this.i18n.tr(`recipe.${this.unit}Plural`)
        : this.i18n.tr(`recipe.${this.unit}`);

    return amount + " " + unit;
  }

  private decimalToFractionString(number: number): string {
    if (Number.isInteger(number)) {
      return new Intl.NumberFormat().format(number).toString();
    }

    const integerPart = Math.floor(number);
    const decimalPart = parseFloat((number - Math.floor(number)).toFixed(2));
    let fraction: string;

    switch (decimalPart) {
      case 0.5:
        fraction = "1/2";
        break;
      case 0.25:
        fraction = "1/4";
        break;
      case 0.75:
        fraction = "3/4";
        break;
      case 0.33:
        fraction = "1/3";
        break;
      case 0.66:
        fraction = "2/3";
        break;
      case 0.2:
        fraction = "1/5";
        break;
      case 0.4:
        fraction = "2/5";
        break;
      case 0.6:
        fraction = "3/5";
        break;
      case 0.8:
        fraction = "4/5";
        break;
    }

    if (fraction) {
      if (integerPart >= 1) {
        return integerPart + " " + fraction;
      } else {
        return fraction;
      }
    }

    return new Intl.NumberFormat().format(number).toString();
  }
}
