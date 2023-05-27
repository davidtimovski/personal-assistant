import type { SharingState } from '$lib/models/viewmodels/sharingState';

export class EditListModel {
	constructor(
		public id: number,
		public name: string,
		public icon: string,
		public tasksText: string,
		public notificationsEnabled: boolean,
		public isOneTimeToggleDefault: boolean,
		public isArchived: boolean,
		public sharingState: SharingState
	) {}
}
