import { UsersServiceBase } from '../../../../../Core/shared2/services/usersServiceBase';

import { user } from '$lib/stores';
import Variables from '$lib/variables';

export class UsersService extends UsersServiceBase {
	constructor() {
		super('Chef');
	}

	async updateNotificationsEnabled(enabled: boolean): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/users/chef-notifications-enabled`, {
				method: 'put',
				body: window.JSON.stringify({
					chefNotificationsEnabled: enabled
				})
			});

			user.update((x) => {
				x.chefNotificationsEnabled = enabled;
				this.cache(x);
				return x;
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}
}
