export class CreateUpcomingExpense {
	constructor(
		public categoryId: number | null,
		public amount: number,
		public currency: string,
		public description: string | null,
		public date: string,
		public generated: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}

export class UpdateUpcomingExpense {
	constructor(
		public id: number,
		public categoryId: number | null,
		public amount: number,
		public currency: string,
		public description: string | null,
		public date: string,
		public generated: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}
