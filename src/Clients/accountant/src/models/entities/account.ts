import { Syncable } from "models/sync/syncable";

export class Account implements Syncable {
  public id: number;
  public isMain: boolean;
  public stockPrice: number;
  public stocks: number;
  public balance: number;
  public synced = false;

  constructor(
    public name: string,
    public currency: string,
    public createdDate: Date,
    public modifiedDate: Date
  ) {}
}
