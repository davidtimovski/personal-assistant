import { inject, observable } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { NotificationsServiceBase } from "../../../shared/src/services/notificationsServiceBase";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { UsersService } from "services/usersService";
import { PreferencesModel } from "models/preferencesModel";

@inject(UsersService, NotificationsServiceBase, I18N, EventAggregator)
export class Preferences {
  private readonly notificationsVapidKey =
    "BIWWy4ZjIrLMVBxYwsq4rlixA3miMGeMw0yCqldR5Cpv5mozBw1oQxEbbp5q1I9SL9_zUjaLfYheoqb578becPY";
  private preferences: PreferencesModel;
  private notificationsState: string;
  private readonly notificationIconSrc = "/images/icons/app-icon-96x96.png";
  private notificationsAreSupported = false;
  private notificationsCheckboxReady = false;

  @observable() private notificationsCheckboxChecked = false;
  @observable() private imperialSystem;

  constructor(
    private readonly usersService: UsersService,
    private readonly notificationsService: NotificationsServiceBase,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {}

  async activate() {
    this.preferences = await this.usersService.getPreferences();
    this.imperialSystem = this.preferences.imperialSystem;

    if ("Notification" in window) {
      this.notificationsAreSupported = true;

      switch ((Notification as any).permission) {
        case "granted":
          this.notificationsState = this.preferences.notificationsEnabled ? "checked" : "unchecked";
          if (this.preferences.notificationsEnabled) {
            this.notificationsCheckboxChecked = true;
          }
          break;
        case "denied":
          this.notificationsState = "denied";
          break;
        default:
          this.notificationsState = "default";
      }
    }

    this.notificationsCheckboxReady = true;
  }

  async notificationsCheckboxCheckedChanged() {
    if (!this.notificationsCheckboxReady) {
      return;
    }

    const previousNotificationsPermission = (Notification as any).permission;

    if (previousNotificationsPermission === "denied") {
      this.eventAggregator.publish(AlertEvents.ShowError, "preferences.notificationsUnpermitted");
    } else {
      await Notification.requestPermission(async (result) => {
        switch (result) {
          case "granted":
            const previousNotificationState = this.notificationsState;

            if (previousNotificationsPermission === "granted") {
              this.preferences.notificationsEnabled = !this.preferences.notificationsEnabled;
              this.notificationsState = this.preferences.notificationsEnabled ? "checked" : "unchecked";
            } else {
              this.preferences.notificationsEnabled = this.notificationsCheckboxChecked = true;
              this.notificationsState = "checked";
            }

            if (this.preferences.notificationsEnabled) {
              try {
                await this.subscribeToPushNotifications();
              } catch {
                this.notificationsState = previousNotificationState;
                this.preferences.notificationsEnabled = this.notificationsCheckboxChecked = false;
              }
            }

            break;
          case "denied":
            this.notificationsState = "denied";
            this.preferences.notificationsEnabled = this.notificationsCheckboxChecked = false;
            break;
          default:
            this.preferences.notificationsEnabled = this.notificationsCheckboxChecked = false;
            break;
        }
      });

      await this.usersService.updateNotificationsEnabled(this.preferences.notificationsEnabled);
    }
  }

  async subscribeToPushNotifications() {
    const swReg = await navigator.serviceWorker.ready;

    const sub = await swReg.pushManager.getSubscription();
    if (sub === null) {
      const newSub = await swReg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: NotificationsServiceBase.getApplicationServerKey(this.notificationsVapidKey),
      });

      await this.notificationsService.createSubscription("Cooking Assistant", newSub);

      await swReg.showNotification("Cooking Assistant", {
        body: this.i18n.tr("preferences.notificationsWereEnabled"),
        icon: this.notificationIconSrc,
        tag: "notifications-enabled",
      });
    }
  }

  async imperialSystemChanged() {
    if (!this.preferences || this.preferences.imperialSystem === this.imperialSystem) {
      return;
    }

    await this.usersService.updateImperialSystem(this.imperialSystem);
  }
}
