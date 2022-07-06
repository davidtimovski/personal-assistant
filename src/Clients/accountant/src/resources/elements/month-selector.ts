import { bindable, bindingMode } from "aurelia-framework";
import { DateHelper } from "../../../../shared/src/utils/dateHelper";
import { SelectOption } from "models/viewmodels/selectOption";

export class MonthSelectorCustomElement {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) month: number;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) year: number;
  @bindable({ defaultBindingMode: bindingMode.toView }) disabled: boolean;
  @bindable({ defaultBindingMode: bindingMode.toView }) language: string;

  private monthOptions: SelectOption[];
  private yearOptions = new Array<number>();
  private currentMonth: number;
  private currentYear: number;

  constructor() {
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
      const month = new Date(1, i, 1);
      const option = new SelectOption(i, DateHelper.getLongMonth(month, this.language));
      this.monthOptions.push(option);
    }
  }
}
