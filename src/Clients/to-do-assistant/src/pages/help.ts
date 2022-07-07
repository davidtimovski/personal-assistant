import { autoinject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

import { TooltipsService } from "../../../shared/src/services/tooltipsService";
import { Tooltip } from "../../../shared/src/models/tooltip";

@autoinject
export class Help {
  private tooltips: Array<Tooltip>;

  constructor(private readonly tooltipsService: TooltipsService, private readonly i18n: I18N) {}

  async activate() {
    const tooltips = await this.tooltipsService.getAll("ToDoAssistant");
    for (let tooltip of tooltips) {
      tooltip.title = this.i18n.tr(`tooltips.${tooltip.key}.title`);
      tooltip.answer = this.i18n.tr(`tooltips.${tooltip.key}.answer`);
    }
    this.tooltips = tooltips;
  }

  async dismiss(tooltip: Tooltip) {
    tooltip.isDismissed = true;
    await this.tooltipsService.toggleDismissed(tooltip.key, "ToDoAssistant", true);
  }

  async retain(tooltip: Tooltip) {
    tooltip.isDismissed = false;
    await this.tooltipsService.toggleDismissed(tooltip.key, "ToDoAssistant", false);
  }
}
