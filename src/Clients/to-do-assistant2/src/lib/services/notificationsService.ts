import { NotificationsServiceBase } from '../../../../shared2/services/notificationsServiceBase';

export class NotificationsService extends NotificationsServiceBase {
	constructor() {
		super('ToDoAssistant', 'to-do-assistant2');
	}

	getAll(): Promise<Array<Notification>> {
		return this.httpProxy.ajax<Array<Notification>>('api/notifications');
	}

	getUnseenNotificationsCount(): Promise<number> {
		return this.httpProxy.ajax<number>('api/notifications/unseen-notifications-count');
	}
}
