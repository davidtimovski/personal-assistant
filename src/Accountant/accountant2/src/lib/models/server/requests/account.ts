export class CreateAccount {
	constructor(
		public name: string,
		public currency: string,
		public stockPrice: number | null,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}

export class UpdateAccount {
	constructor(
		public id: number,
		public name: string,
		public currency: string,
		public stockPrice: number | null,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}
