import { Syncable } from "models/sync/syncable";

export class UpcomingExpense implements Syncable {
  public categoryName: string;
  public synced = false;

  constructor(
    public id: number,
    public categoryId: number,
    public amount: number,
    public currency: string,
    public description: string,
    public date: string,
    public generated: boolean,
    public createdDate: Date,
    public modifiedDate: Date
  ) {}
}
