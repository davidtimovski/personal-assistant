import { LocalStorageBase } from '../../../../shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				['soundsEnabled', true],
				['highPriorityListEnabled', true]
			])
		);
	}
}

export enum LocalStorageKeys {
	SoundsEnabled = 'soundsEnabled',
	HighPriorityListEnabled = 'highPriorityListEnabled'
}
