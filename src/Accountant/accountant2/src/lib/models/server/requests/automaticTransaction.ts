export class CreateAutomaticTransaction {
	constructor(
		public isDeposit: boolean,
		public categoryId: number | null,
		public amount: number,
		public currency: string,
		public description: string | null,
		public dayInMonth: number,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}

export class UpdateAutomaticTransaction {
	constructor(
		public id: number,
		public isDeposit: boolean,
		public categoryId: number | null,
		public amount: number,
		public currency: string,
		public description: string | null,
		public dayInMonth: number,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}
