import { NotificationsServiceBase } from "../../../shared/src/services/notificationsServiceBase";
import { Notification } from "models/viewmodels/notification";

export class NotificationsService extends NotificationsServiceBase {
  getAll(): Promise<Array<Notification>> {
    return this.ajax<Array<Notification>>("notifications");
  }

  getUnseenNotificationsCount(): Promise<number> {
    return this.ajax<number>("notifications/unseen-notifications-count");
  }
}
