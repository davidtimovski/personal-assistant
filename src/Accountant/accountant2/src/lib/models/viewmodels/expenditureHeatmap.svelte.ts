export class HeatmapDay {
	backgroundColor: string | null = $state(null);
	textColor: string | null = $state(null);

	constructor(
		public day: number,
		public date: string,
		public formattedDate: string,
		public isToday: boolean,
		public spent: number,
		public spentPercentage: number,
		backgroundColor: string | null,
		textColor: string | null,
		public expenditures: HeatmapExpense[]
	) {
		this.backgroundColor = backgroundColor;
		this.textColor = textColor;
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
