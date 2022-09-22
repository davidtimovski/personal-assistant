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
		public computedListType: string,
		public tasks: Task[],
		public modifiedDate: string | null
	) {}
}

export class Task {
	id: number;
	listId: number;
	name: string;
	isCompleted: boolean;
	isOneTime: boolean;
	isHighPriority: boolean;
	isPrivate: boolean;
	assignedUser: AssignedUser;
	order: number;
}
