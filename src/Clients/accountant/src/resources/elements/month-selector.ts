import { bindable, bindingMode, inject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

import { SelectOption } from "models/viewmodels/selectOption";

@inject(I18N)
export class MonthSelectorCustomElement {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) month: number;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) year: number;
  @bindable({ defaultBindingMode: bindingMode.toView }) disabled: boolean;

  private monthOptions: Array<SelectOption>;
  private yearOptions: Array<number> = [];
  private currentMonth: number;
  private currentYear: number;

  constructor(private readonly i18n: I18N) {
    const now = new Date();
    this.currentMonth = now.getMonth();
    this.currentYear = now.getFullYear();
  }

  attached() {
    if (this.month === undefined) {
      this.month = this.currentMonth;
    }
    if (this.year === undefined) {
      this.year = this.currentYear;
    }

    this.yearOptions = [this.currentYear, this.currentYear + 1];
  }

  yearChanged() {
    let startingMonthOption = 0;

    if (this.year === this.currentYear) {
      startingMonthOption = this.currentMonth;

      if (this.month < this.currentMonth) {
        this.month = this.currentMonth;
      }
    }

    this.monthOptions = [];
    for (let i = startingMonthOption; i < 12; i++) {
      const option = new SelectOption(i, this.i18n.tr(`months.${i}`));
      this.monthOptions.push(option);
    }
  }
}
