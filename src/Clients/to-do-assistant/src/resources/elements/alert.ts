import { inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";

import { AlertEvents } from "../../../../shared/src/models/enums/alertEvents";

@inject(EventAggregator, I18N)
export class AlertCustomElement {
  private type: string;
  private message: string;
  private fading = false;
  private refreshLink: HTMLAnchorElement;
  private hideTimeout = 0;

  constructor(private readonly eventAggregator: EventAggregator, private readonly i18n: I18N) {
    this.eventAggregator.subscribe(AlertEvents.ShowError, (errors: any) => {
      this.type = "error";

      if (errors.constructor === Array) {
        this.message = errors.join("<br>");
      } else {
        const translationKey = errors;

        if (this.refreshLink) {
          this.refreshLink.style.display = translationKey === "unexpectedError" ? "block" : "none";
        }

        this.message = this.i18n.tr(translationKey);
      }
    });

    this.eventAggregator.subscribe(AlertEvents.ShowSuccess, (translationKey: string) => {
      this.reset();

      this.type = "success";
      this.message = this.i18n.tr(translationKey);

      this.hideTimeout = window.setTimeout(() => {
        this.hide();
      }, 5000);
    });

    this.eventAggregator.subscribe(AlertEvents.HideError, () => {
      if (this.type === "error") {
        this.hide();
      }
    });
  }

  reset() {
    this.message = null;
    this.fading = false;
    this.refreshLink.style.display = "none";
  }

  hide() {
    if (this.hideTimeout !== 0) {
      window.clearTimeout(this.hideTimeout);
      this.hideTimeout = 0;
    }

    this.reset();
    this.eventAggregator.publish(AlertEvents.OnHidden);
  }

  refresh() {
    window.location.reload();
  }
}
