import { Syncable } from "models/sync/syncable";

export class DebtModel implements Syncable {
  synced = false;

  constructor(
    public id: number,
    public person: string,
    public amount: number,
    public currency: string,
    public description: string,
    public userIsDebtor: boolean,
    public createdDate: Date,
    public modifiedDate: Date
  ) {}
}
