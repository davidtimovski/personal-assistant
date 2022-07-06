import { inject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

import { TooltipsService } from "../../../shared/src/services/tooltipsService";
import { Tooltip } from "../../../shared/src/models/tooltip";

@inject(TooltipsService, I18N)
export class Help {
  private tooltips: Tooltip[];

  constructor(private readonly tooltipsService: TooltipsService, private readonly i18n: I18N) {}

  async activate() {
    const tooltips = await this.tooltipsService.getAll("Accountant");
    for (const tooltip of tooltips) {
      tooltip.title = this.i18n.tr(`tooltips.${tooltip.key}.title`);
      tooltip.answer = this.i18n.tr(`tooltips.${tooltip.key}.answer`);
    }
    this.tooltips = tooltips;
  }

  async dismiss(tooltip: Tooltip) {
    tooltip.isDismissed = true;
    await this.tooltipsService.toggleDismissed(tooltip.key, "Accountant", true);
  }

  async retain(tooltip: Tooltip) {
    tooltip.isDismissed = false;
    await this.tooltipsService.toggleDismissed(tooltip.key, "Accountant", false);
  }
}
