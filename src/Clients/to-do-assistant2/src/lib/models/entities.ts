import type { SharingState } from '$lib/models/viewmodels/sharingState';
import type { AssignedUser } from '$lib/models/viewmodels/assignedUser';

export class List {
	constructor(
		public id: number,
		public name: string | null,
		public icon: string | null,
		public notificationsEnabled: boolean,
		public isOneTimeToggleDefault: boolean,
		public sharingState: SharingState,
		public order: number,
		public isArchived: boolean,
		public derivedListType: string,
		public tasks: Task[],
		public modifiedDate: string | null
	) {}
}

export class Task {
	constructor(
		public id: number,
		public listId: number,
		public name: string,
		public isCompleted: boolean,
		public isOneTime: boolean,
		public isHighPriority: boolean,
		public isPrivate: boolean,
		public assignedUser: AssignedUser,
		public order: number,
		public modifiedDate: string
	) {}
}
