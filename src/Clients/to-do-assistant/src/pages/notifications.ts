import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";

import { DateHelper } from "../../../shared/src/utils/dateHelper";

import { NotificationsService } from "services/notificationsService";
import { Notification } from "models/viewmodels/notification";
import { LocalStorage } from "utils/localStorage";

@autoinject
export class Notifications {
  private highlightedId: number;
  private unseenNotifications: Array<Notification>;
  private seenNotifications: Array<Notification>;
  private language: string;
  private seenNotificationsVisible = false;

  constructor(
    private readonly router: Router,
    private readonly notificationsService: NotificationsService,
    private readonly localStorage: LocalStorage
  ) {}

  activate(params: any) {
    if (params.id) {
      this.highlightedId = parseInt(params.id, 10);
    }

    this.language = this.localStorage.getLanguage();
  }

  async attached() {
    const allNotifications = await this.notificationsService.getAll();

    for (let notification of allNotifications) {
      notification.formattedCreatedDate = DateHelper.formatWeekdayTime(
        new Date(notification.createdDate),
        this.language
      );
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
}
