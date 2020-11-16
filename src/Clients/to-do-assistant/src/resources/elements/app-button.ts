import { inject, bindable, bindingMode } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

@inject(I18N)
export class AppButtonCustomElement {
  @bindable({ defaultBindingMode: bindingMode.oneWay }) visible: boolean;
  @bindable({ defaultBindingMode: bindingMode.oneWay }) label: string;
  @bindable({ defaultBindingMode: bindingMode.oneWay }) labelKey: string;
  @bindable({ defaultBindingMode: bindingMode.oneWay }) action: () => void;
  @bindable({ defaultBindingMode: bindingMode.oneWay }) loading: boolean;
  @bindable({ defaultBindingMode: bindingMode.oneWay }) disable: boolean;
  private buttonLabel: string;

  constructor(private readonly i18n: I18N) {}

  bind() {
    if (this.labelKey) {
      this.buttonLabel = this.i18n.tr(this.labelKey);
    } else {
      this.buttonLabel = this.label;
    }
  }

  executeAction() {
    if (!this.disable) {
      this.action();
    }
  }
}
