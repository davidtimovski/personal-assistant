export class BulkCreate {
	constructor(
		public readonly listId: number,
		public readonly tasksText: string,
		public readonly tasksAreOneTime: boolean,
		public readonly tasksArePrivate: boolean
	) {}
}
