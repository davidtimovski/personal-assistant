import { inject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { UsersService } from "services/usersService";
import { NotificationsService } from "services/notificationsService";
import { LocalStorage } from "utils/localStorage";
import { PreferencesModel } from "models/preferencesModel";

@inject(UsersService, NotificationsService, LocalStorage, I18N, EventAggregator)
export class Preferences {
  private readonly notificationsVapidKey =
    "BCL8HRDvXuYjw011VypF_TtfmklYFmqXAADY7pV3WB9vL609d8wNK0zTUs4hB0V3uAnCTpzOd2pANBmsMQoUhD0";
  private preferences: PreferencesModel;
  private notificationsState: string;
  private notificationsCheckboxChecked = false;
  private readonly notificationIconSrc = "/images/icons/app-icon-96x96.png";
  private notificationsAreSupported = false;
  private soundsEnabled: boolean;

  constructor(
    private readonly usersService: UsersService,
    private readonly notificationsService: NotificationsService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {}

  async activate() {
    this.preferences = await this.usersService.getPreferences();

    if ("Notification" in window) {
      this.notificationsAreSupported = true;

      switch ((Notification as any).permission) {
        case "granted":
          this.notificationsState = this.preferences.notificationsEnabled
            ? "checked"
            : "unchecked";
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

    this.soundsEnabled = this.localStorage.getSoundsEnabled();
  }

  async notificationsChanged() {
    const previousNotificationsPermission = (Notification as any).permission;

    if (previousNotificationsPermission === "denied") {
      this.eventAggregator.publish(
        AlertEvents.ShowError,
        "preferences.notificationsUnpermitted"
      );
    } else {
      await Notification.requestPermission(async (result) => {
        switch (result) {
          case "granted":
            const previousNotificationState = this.notificationsState;

            if (previousNotificationsPermission === "granted") {
              this.preferences.notificationsEnabled = !this.preferences
                .notificationsEnabled;
              this.notificationsState = this.preferences.notificationsEnabled
                ? "checked"
                : "unchecked";
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

      await this.usersService.updateNotificationsEnabled(
        this.preferences.notificationsEnabled
      );
    }
  }

  async subscribeToPushNotifications() {
    const swReg = await navigator.serviceWorker.ready;

    const sub = await swReg.pushManager.getSubscription();
    if (sub === null) {
      const newSub = await swReg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: NotificationsService.getApplicationServerKey(
          this.notificationsVapidKey
        ),
      });

      await this.notificationsService.createSubscription(
        "To Do Assistant",
        newSub
      );

      await swReg.showNotification("To Do Assistant", {
        body: this.i18n.tr("preferences.notificationsWereEnabled"),
        icon: this.notificationIconSrc,
        tag: "notifications-enabled",
      });
    }
  }

  soundsChanged() {
    this.localStorage.setSoundsEnabled(this.soundsEnabled);

    if (this.soundsEnabled) {
      new Audio("/audio/bleep.mp3").play();
    }
  }
}
