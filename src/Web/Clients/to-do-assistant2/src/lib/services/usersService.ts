import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';

import { user } from '$lib/stores';
import Variables from '$lib/variables';

export class UsersService extends UsersServiceBase {
	constructor() {
		super('To Do Assistant');
	}

	async updateNotificationsEnabled(enabled: boolean): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/users/to-do-notifications-enabled`, {
				method: 'put',
				body: window.JSON.stringify({
					toDoNotificationsEnabled: enabled
				})
			});

			user.update((x) => {
				x.toDoNotificationsEnabled = enabled;
				this.cache(x);
				return x;
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}
}
