import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";

@inject(Router, EventAggregator, I18N)
export class AlertCustomElement {
  private type: string;
  private message: string;
  private fading = false;
  private refreshLink: HTMLAnchorElement;
  private hideTimeout = 0;

  constructor(
    private readonly router: Router,
    private readonly eventAggregator: EventAggregator,
    private readonly i18n: I18N
  ) {
    this.eventAggregator.subscribe("alert-error", (errors: any) => {
      this.type = "error";

      if (errors.constructor === Array) {
        this.message = errors.join("<br>");
      } else {
        const translationKey = errors;

        if (this.refreshLink) {
          this.refreshLink.style.display =
            translationKey === "unexpectedError" ? "block" : "none";
        }

        this.message = this.i18n.tr(translationKey);
      }
    });

    this.eventAggregator.subscribe(
      "alert-success",
      (translationKey: string) => {
        this.reset();

        this.type = "success";
        this.message = this.i18n.tr(translationKey);

        this.hideTimeout = window.setTimeout(() => {
          this.hide();
        }, 5000);
      }
    );

    this.eventAggregator.subscribe("reset-alert-error", () => {
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
    this.eventAggregator.publish("alert-hidden");
  }

  refresh() {
    window.location.reload();
  }

  totalSync() {
    this.reset();
    this.router.navigateToRoute("totalSync");
  }
}
