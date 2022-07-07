import { NotificationsServiceBase } from "../../../shared/src/services/notificationsServiceBase";
import { Notification } from "models/viewmodels/notification";

export class NotificationsService extends NotificationsServiceBase {
  getAll(): Promise<Array<Notification>> {
    return this.httpProxy.ajax<Array<Notification>>("api/notifications");
  }

  getUnseenNotificationsCount(): Promise<number> {
    return this.httpProxy.ajax<number>("api/notifications/unseen-notifications-count");
  }
}
