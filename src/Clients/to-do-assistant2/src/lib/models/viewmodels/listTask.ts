import type { Task } from '$lib/models/entities';
import type { Assignee } from '$lib/models/viewmodels/assignee';

export class ListTask {
	active = false;

	constructor(
		public id: number,
		public listId: number,
		public name: string,
		public url: string,
		public isCompleted: boolean,
		public isOneTime: boolean,
		public isHighPriority: boolean,
		public isPrivate: boolean,
		public assignedUser: Assignee,
		public order: number,
		public modifiedDate: string,
		public disabled: boolean
	) {}

	static fromTask(task: Task, disabled: boolean) {
		return new ListTask(
			task.id,
			task.listId,
			task.name,
			task.url,
			task.isCompleted,
			task.isOneTime,
			task.isHighPriority,
			task.isPrivate,
			task.assignedUser,
			task.order,
			task.modifiedDate,
			disabled
		);
	}
}
