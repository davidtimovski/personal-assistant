import { bindable, bindingMode } from "aurelia-framework";

export class CheckboxCustomElement {
  @bindable({ defaultBindingMode: bindingMode.toView }) labelKey: string;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) value: boolean;
  @bindable({ defaultBindingMode: bindingMode.toView }) disabled: boolean;
}
