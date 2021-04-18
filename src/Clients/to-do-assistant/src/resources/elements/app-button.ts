import { inject, bindable, bindingMode } from "aurelia-framework";
import { I18N } from "aurelia-i18n";

@inject(I18N)
export class AppButtonCustomElement {
  @bindable({ defaultBindingMode: bindingMode.toView }) visible: boolean;
  @bindable({ defaultBindingMode: bindingMode.toView }) label: string;
  @bindable({ defaultBindingMode: bindingMode.toView }) labelKey: string;
  @bindable({ defaultBindingMode: bindingMode.toView }) action: () => void;
  @bindable({ defaultBindingMode: bindingMode.toView }) loading: boolean;
  @bindable({ defaultBindingMode: bindingMode.toView }) disable: boolean;
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
