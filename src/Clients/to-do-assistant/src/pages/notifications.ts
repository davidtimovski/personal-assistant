import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";

import { NotificationsService } from "services/notificationsService";
import { Notification } from "models/viewmodels/notification";
import { LocalStorage } from "utils/localStorage";

@inject(Router, NotificationsService, LocalStorage, I18N)
export class Notifications {
  private highlightedId: number;
  private unseenNotifications: Array<Notification>;
  private seenNotifications: Array<Notification>;
  private language: string;
  private seenNotificationsVisible = false;

  constructor(
    private readonly router: Router,
    private readonly notificationsService: NotificationsService,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N
  ) {}

  activate(params: any) {
    if (params.id) {
      this.highlightedId = parseInt(params.id, 10);
    }

    this.language = this.localStorage.getLanguage();
  }

  attached() {
    this.notificationsService.getAll().then((allNotifications: Array<Notification>) => {
      for (let notification of allNotifications) {
        notification.formattedCreatedDate = this.formatCreatedDate(notification.createdDate);
      }

      this.unseenNotifications = allNotifications
        .filter((notification) => {
          return !notification.isSeen;
        })
        .map(this.replacePlaceholders);
      this.seenNotifications = allNotifications
        .filter((notification) => {
          return notification.isSeen;
        })
        .map(this.replacePlaceholders);
    });
  }

  open(notification: Notification) {
    if (notification.listId && notification.taskId) {
      this.router.navigateToRoute("listEdited", {
        id: notification.listId,
        editedId: notification.taskId,
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
    const time = date.toLocaleTimeString(this.language);

    return `${weekday}, ${time}`;
  }
}
