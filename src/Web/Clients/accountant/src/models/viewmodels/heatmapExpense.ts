export class HeatmapExpense {
  constructor(
    public transactionId: number,
    public category: string,
    public description: string,
    public amount: number
  ) {}
}
