export class UpcomingExpenseItem {
  constructor(
    public id: number,
    public amount: number,
    public currency: string,
    public category: string,
    public date: string,
    public synced: boolean
  ) {}
}
