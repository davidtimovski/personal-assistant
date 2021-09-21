export class EditUpcomingExpenseModel {
  public month: number;
  public year: number;

  constructor(
    public id: number,
    public categoryId: number,
    public amount: number,
    public currency: string,
    public description: string,
    public date: string,
    public generated: boolean,
    public createdDate: Date,
    public synced: boolean
  ) {
    if (this.date) {
      this.month = parseInt(this.date.slice(5, 7), 10) - 1;
      this.year = parseInt(this.date.slice(0, 4), 10);
    }
  }

  getDate() {
    return `${this.year}-${this.month + 1}-01`;
  }
}
