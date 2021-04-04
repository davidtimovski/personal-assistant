import { inject, bindable, bindingMode } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

import { TooltipsService } from "../../../../shared/src/services/tooltipsService";
import { Tooltip } from "../../../../shared/src/models/tooltip";

@inject(TooltipsService, I18N)
export class TooltipCustomElement {
  @bindable({ defaultBindingMode: bindingMode.toView }) key: string;
  private tooltip: Tooltip;
  private isVisible = false;
  private isOpen = false;
  private isDismissed = false;
  private questionSpan: HTMLSpanElement;

  constructor(
    private readonly tooltipsService: TooltipsService,
    private readonly i18n: I18N
  ) {}

  async attached() {
    this.tooltip = await this.tooltipsService.getByKey(this.key, "Accountant");
    if (!this.tooltip.isDismissed) {
      this.tooltip.question = this.i18n.tr(`tooltips.${this.key}.question`);
      this.tooltip.answer = this.i18n.tr(`tooltips.${this.key}.answer`);
      this.isVisible = true;
    }
  }

  toggleOpen() {
    this.isOpen = !this.isOpen;
    this.questionSpan.classList.remove("glow");
  }

  async dismiss() {
    this.isDismissed = true;
    await this.tooltipsService.toggleDismissed(this.tooltip.key, "Accountant", true);
  }
}
