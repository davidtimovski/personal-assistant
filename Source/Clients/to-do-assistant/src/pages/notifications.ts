import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { NotificationsService } from "services/notificationsService";
import { Notification } from "models/viewmodels/notification";
import { LocalStorage } from "utils/localStorage";
import { I18N } from "aurelia-i18n";

@inject(Router, NotificationsService, LocalStorage, I18N)
export class Notifications {
  private highlightedId: number;
  private unseenNotifications: Array<Notification>;
  private seenNotifications: Array<Notification>;
  private language: string;
  private emptyListMessage: string;
  private seenNotificationsVisible = false;

  constructor(
    private readonly router: Router,
    private readonly notificationsService: NotificationsService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N
  ) {
    this.language = this.localStorage.getLanguage();
  }

  async activate(params: any) {
    if (params.id) {
      this.highlightedId = parseInt(params.id, 10);
    }
  }

  attached() {
    this.notificationsService
      .getAll()
      .then((allNotifications: Array<Notification>) => {
        for (let notification of allNotifications) {
          notification.formattedCreatedDate = this.formatCreatedDate(
            notification.createdDate
          );
        }

        this.unseenNotifications = allNotifications
          .filter(notification => {
            return !notification.isSeen;
          })
          .map(this.replacePlaceholders);
        this.seenNotifications = allNotifications
          .filter(notification => {
            return notification.isSeen;
          })
          .map(this.replacePlaceholders);

        this.emptyListMessage = this.i18n.tr("notifications.emptyListMessage");
      });
  }

  open(notification: Notification) {
    if (notification.listId && notification.taskId) {
      this.router.navigateToRoute("listEdited", {
        id: notification.listId,
        editedId: notification.taskId
      });
    } else if (notification.listId) {
      this.router.navigateToRoute("list", { id: notification.listId });
    }
  }

  showSeenNotifications() {
    this.seenNotificationsVisible = true;
  }

  @computedFrom("highlightedId")
  get getHighlightedId() {
    return this.highlightedId;
  }

  replacePlaceholders(notification: Notification) {
    notification.message = notification.message
      .replace(/#\[/g, '<span class="colored-text">')
      .replace(/\]#/g, "</span>");
    return notification;
  }

  formatCreatedDate(createdDateString: string): string {
    const date = new Date(createdDateString);
    const weekday = this.i18n.tr(`weekdays.${date.getDay()}`);
    const month = this.i18n.tr(`months.${date.getMonth()}`);

    const now = new Date();
    if (now.getFullYear() === date.getFullYear()) {
      return `${weekday}, ${month} ${date.getDate()} ${date
        .toLocaleTimeString(this.language)
        .slice(0, -6)}`;
    }

    return `${weekday}, ${month} ${date.getDate()}, ${date.getFullYear()} ${date
      .toLocaleTimeString(this.language)
      .slice(0, -6)}`;
  }
}
