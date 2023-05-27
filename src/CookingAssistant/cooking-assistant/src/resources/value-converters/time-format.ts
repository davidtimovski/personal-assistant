import { autoinject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

@autoinject
export class TimeFormatValueConverter {
  constructor(private readonly i18n: I18N) {}

  toView(value: string) {
    const hours = parseInt(value.substring(0, 2), 10);
    const minutes = parseInt(value.substring(3, 5), 10);

    if (hours === 0) {
      return minutes + this.i18n.tr("minutesLetter");
    } else if (minutes === 0) {
      return hours + this.i18n.tr("hoursLetter");
    }

    return hours + this.i18n.tr("hoursLetter") + " " + minutes + this.i18n.tr("minutesLetter");
  }
}
