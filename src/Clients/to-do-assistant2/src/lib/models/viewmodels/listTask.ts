import type { Task } from '$lib/models/entities';
import type { AssignedUser } from '$lib/models/viewmodels/assignedUser';

export class ListTask {
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

	static fromTask(task: Task) {
		return new ListTask(
			task.id,
			task.listId,
			task.name,
			task.isCompleted,
			task.isOneTime,
			task.isHighPriority,
			task.isPrivate,
			task.assignedUser,
			task.order,
			task.modifiedDate
		);
	}
}
