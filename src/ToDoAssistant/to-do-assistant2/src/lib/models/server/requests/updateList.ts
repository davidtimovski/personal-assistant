export class UpdateList {
	constructor(
		public readonly id: number,
		public readonly name: string,
		public readonly icon: string,
		public readonly isOneTimeToggleDefault: boolean,
		public readonly notificationsEnabled: boolean
	) {}
}
