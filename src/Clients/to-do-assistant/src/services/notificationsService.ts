import { NotificationsServiceBase } from "../../../shared/src/services/notificationsServiceBase";
import { Notification } from "models/viewmodels/notification";

export class NotificationsService extends NotificationsServiceBase {
  async getAll(): Promise<Array<Notification>> {
    const result = await this.ajax<Array<Notification>>("notifications");

    return result;
  }

  async getUnseenNotificationsCount(): Promise<number> {
    const result = await this.ajax<number>("notifications/unseen-notifications-count");

    return result;
  }
}
