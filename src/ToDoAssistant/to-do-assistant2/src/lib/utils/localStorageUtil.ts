import { LocalStorageBase } from '../../../../../Core/shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				[LocalStorageKeys.SoundsEnabled, true],
				[LocalStorageKeys.HighPriorityListEnabled, true],
				[LocalStorageKeys.StaleTasksListEnabled, true],
				[LocalStorageKeys.ImmediateList, <number | null>null]
			])
		);
	}
}

export enum LocalStorageKeys {
	SoundsEnabled = 'soundsEnabled',
	HighPriorityListEnabled = 'highPriorityListEnabled',
	StaleTasksListEnabled = 'staleTasksListEnabled',
	ImmediateList = 'immediateList'
}
