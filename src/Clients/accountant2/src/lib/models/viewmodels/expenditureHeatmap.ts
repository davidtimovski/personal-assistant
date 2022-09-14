export class HeatmapDay {
	constructor(
		public day: number,
		public date: string,
		public formattedDate: string,
		public isToday: boolean,
		public spent: number,
		public spentPercentage: number,
		public backgroundColor: string | null,
		public textColor: string | null,
		public expenditures: HeatmapExpense[]
	) {
		this.expenditures = [];
	}
}

export class HeatmapExpense {
	constructor(
		public transactionId: number,
		public category: string,
		public description: string,
		public amount: number
	) {}
}
