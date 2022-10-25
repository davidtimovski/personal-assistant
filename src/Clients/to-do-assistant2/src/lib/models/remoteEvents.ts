export class RemoteEvent {
	constructor(public type: RemoteEventType, public data?: any) {}
}

export enum RemoteEventType {
	None = 'remote:none',
	TaskCompletedRemotely = 'app:task-completed-remotely',
	TaskUncompletedRemotely = 'app:task-uncompleted-remotely',
	TaskDeletedRemotely = 'app:task-deleted-remotely',
	TaskReorderedRemotely = 'app:task-reordered-remotely'
}
