import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';

import type { PreferencesModel } from '$lib/models/preferencesModel';
import Variables from '$lib/variables';

export class UsersService extends UsersServiceBase {
	constructor() {
		super('ToDoAssistant');
	}

	async getPreferences(): Promise<PreferencesModel> {
		return await this.httpProxy.ajax<PreferencesModel>(`${Variables.urls.api}/api/users/to-do-preferences`);
	}

	async updateNotificationsEnabled(enabled: boolean): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/users/to-do-notifications-enabled`, {
				method: 'put',
				body: window.JSON.stringify({
					toDoNotificationsEnabled: enabled
				})
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}
}
