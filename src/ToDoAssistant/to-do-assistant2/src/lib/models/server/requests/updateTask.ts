export class UpdateTask {
	constructor(
		public readonly id: number,
		public readonly listId: number,
		public readonly name: string,
		public readonly url: string | null,
		public readonly isOneTime: boolean,
		public readonly isHighPriority: boolean,
		public readonly isPrivate: boolean | null,
		public readonly assignedToUserId: number | null
	) {}
}
