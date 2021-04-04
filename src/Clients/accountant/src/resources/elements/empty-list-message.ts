import { bindable, bindingMode } from "aurelia-framework";

export class EmptyListMessageCustomElement {
  @bindable({ defaultBindingMode: bindingMode.toView }) message: string;
}
