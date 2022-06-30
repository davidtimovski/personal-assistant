import { inject, observable } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { UsersService } from "services/usersService";
import { NotificationsService } from "services/notificationsService";
import { ListsService } from "services/listsService";
import { PreferencesModel } from "models/preferencesModel";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";
import { SoundPlayer } from "utils/soundPlayer";

@inject(UsersService, NotificationsService, ListsService, I18N, EventAggregator)
@connectTo()
export class Preferences {
  private readonly notificationsVapidKey =
    "BCL8HRDvXuYjw011VypF_TtfmklYFmqXAADY7pV3WB9vL609d8wNK0zTUs4hB0V3uAnCTpzOd2pANBmsMQoUhD0";
  private preferences: PreferencesModel;
  private notificationsState: string;
  private readonly notificationIconSrc = "/images/icons/app-icon-96x96.png";
  private notificationsAreSupported = false;
  private readonly soundPlayer = new SoundPlayer();
  private preferencesInitialized = false;
  state: State;

  @observable() private notificationsCheckboxChecked;
  @observable() private soundsEnabled: boolean;
  @observable() private highPriorityListEnabled: boolean;

  constructor(
    private readonly usersService: UsersService,
    private readonly notificationsService: NotificationsService,
    private readonly listsService: ListsService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {}

  async activate() {
    this.preferences = await this.usersService.getPreferences();

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
  }

  attached() {
    this.soundsEnabled = this.state.soundsEnabled;
    this.highPriorityListEnabled = this.state.highPriorityListEnabled;
    this.preferencesInitialized = true;
  }

  async notificationsCheckboxCheckedChanged() {
    if (!this.preferencesInitialized) {
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
        applicationServerKey: NotificationsService.getApplicationServerKey(this.notificationsVapidKey),
      });

      await this.notificationsService.createSubscription("To Do Assistant", newSub);

      await swReg.showNotification("To Do Assistant", {
        body: this.i18n.tr("preferences.notificationsWereEnabled"),
        icon: this.notificationIconSrc,
        tag: "notifications-enabled",
      });
    }
  }

  async soundsEnabledChanged() {
    if (!this.preferencesInitialized) {
      return;
    }

    Actions.updatePreference("soundsEnabled", this.soundsEnabled);

    if (this.soundsEnabled) {
      await this.soundPlayer.initialize();
      this.soundPlayer.playBleep();
    }
  }

  highPriorityListEnabledChanged() {
    if (!this.preferencesInitialized) {
      return;
    }

    Actions.updatePreference("highPriorityListEnabled", this.highPriorityListEnabled);

    Actions.getLists(this.listsService);
  }
}
