export class CreateDebt {
	constructor(
		public person: string,
		public amount: number,
		public currency: string,
		public description: string | null,
		public userIsDebtor: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}

export class UpdateDebt {
	constructor(
		public id: number,
		public person: string,
		public amount: number,
		public currency: string,
		public description: string | null,
		public userIsDebtor: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}
