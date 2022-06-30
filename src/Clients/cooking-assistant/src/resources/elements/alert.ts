import { inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";

import { AlertEvents } from "../../../../shared/src/models/enums/alertEvents";

@inject(EventAggregator, I18N)
export class AlertCustomElement {
  private type: string;
  private message: string;
  private refreshLink: HTMLAnchorElement;
  private shown = false;
  private hideTimeout = 0;
  private resetMessageTimeout = 0;

  constructor(private readonly eventAggregator: EventAggregator, private readonly i18n: I18N) {
    this.eventAggregator.subscribe(AlertEvents.ShowError, (errors: any) => {
      let message: string;

      if (errors.constructor === Array) {
        message = errors.join("<br>");
      } else {
        const translationKey = errors;

        if (this.refreshLink) {
          this.refreshLink.style.display = translationKey === "unexpectedError" ? "block" : "none";
        }

        message = this.i18n.tr(translationKey);
      }

      this.show("error", message);
    });

    this.eventAggregator.subscribe(AlertEvents.ShowSuccess, (translationKey: string) => {
      this.showTemporary("success", this.i18n.tr(translationKey));
    });

    this.eventAggregator.subscribe(AlertEvents.HideError, () => {
      if (this.type === "error") {
        this.hide();
      }
    });
  }

  show(type: string, message: string) {
    if (this.resetMessageTimeout) {
      window.clearTimeout(this.resetMessageTimeout);
      this.resetMessageTimeout = 0;
    }

    this.type = type;
    this.message = message;
    this.shown = true;
  }

  showTemporary(type: string, message: string) {
    this.show(type, message);

    if (this.hideTimeout) {
      window.clearTimeout(this.hideTimeout);
      this.hideTimeout = 0;
    }

    this.hideTimeout = window.setTimeout(() => {
      this.hide();
    }, 5000);
  }

  hide() {
    this.shown = false;

    this.resetMessageTimeout = window.setTimeout(() => {
      this.reset();
    }, 1000);

    this.eventAggregator.publish(AlertEvents.OnHidden);
  }

  reset() {
    this.type = null;
    this.message = null;
    this.refreshLink.style.display = "none";
  }

  refresh() {
    window.location.reload();
  }
}
