export class SelectOption {
	constructor(
		public id: number | null,
		public name: string
	) {}
}

export class SelectOptionExtended<T> extends SelectOption {
	constructor(
		public id: number | null,
		public name: string,
		public data: T
	) {
		super(id, name);
	}
}
