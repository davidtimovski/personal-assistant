export class EditUpcomingExpenseModel {
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
  ) {}
}
