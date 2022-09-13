import type { HeatmapExpense } from './heatmapExpense';

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
