import { LocalStorageBase } from '../../../../../Core/shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				//[LocalStorageKeys.SoundsEnabled, true],
				//[LocalStorageKeys.HighPriorityListEnabled, true],
				//[LocalStorageKeys.StaleTasksListEnabled, true]
			])
		);
	}
}

export enum LocalStorageKeys {
	//SoundsEnabled = 'soundsEnabled',
	//HighPriorityListEnabled = 'highPriorityListEnabled',
	//StaleTasksListEnabled = 'staleTasksListEnabled'
}
