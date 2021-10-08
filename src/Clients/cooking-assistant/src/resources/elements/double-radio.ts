import { bindable, bindingMode } from "aurelia-framework";

export class DoubleRadioCustomElement {
  @bindable({ defaultBindingMode: bindingMode.toView }) name: string;
  @bindable({ defaultBindingMode: bindingMode.toView }) leftLabelKey: string;
  @bindable({ defaultBindingMode: bindingMode.toView }) rightLabelKey: string;
  @bindable({ defaultBindingMode: bindingMode.toView }) leftValue: any;
  @bindable({ defaultBindingMode: bindingMode.toView }) rightValue: any;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) bindTo: any;
}
