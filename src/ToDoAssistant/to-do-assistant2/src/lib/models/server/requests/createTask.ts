export class CreateTask {
	constructor(
		public readonly listId: number,
		public readonly name: string,
		public readonly url: string | null,
		public readonly isOneTime: boolean,
		public readonly isPrivate: boolean | null
	) {}
}
