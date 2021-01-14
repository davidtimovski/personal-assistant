import { HeatmapExpense } from "./heatmapExpense";

export class HeatmapDay {
  constructor(
    public day: number,
    public date: string,
    public formattedDate: string,
    public isToday: boolean,
    public spent: number,
    public spentPercentage: number,
    public backgroundColor: string,
    public textColor: string,
    public expenditures: Array<HeatmapExpense>
  ) {
    this.expenditures = [];
  }
}
