import { NotificationsServiceBase } from '../../../../shared2/services/notificationsServiceBase';

import type { Notification } from '$lib/models/viewmodels/notification';
import Variables from '$lib/variables';

export class NotificationsService extends NotificationsServiceBase {
	constructor() {
		super('To Do Assistant');
	}

	getAll(): Promise<Array<Notification>> {
		return this.httpProxy.ajax<Array<Notification>>(`${Variables.urls.api}/api/notifications`);
	}

	getUnseenNotificationsCount(): Promise<number> {
		return this.httpProxy.ajax<number>(`${Variables.urls.api}/api/notifications/unseen-notifications-count`);
	}
}
