export class CreateList {
	constructor(
		public readonly name: string,
		public readonly icon: string,
		public readonly isOneTimeToggleDefault: boolean,
		public readonly tasksText: string | null
	) {}
}
