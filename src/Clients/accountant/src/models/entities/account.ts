import { Syncable } from "models/sync/syncable";

export class Account implements Syncable {
  id: number;
  isMain: boolean;
  stockPrice: number;
  stocks: number;
  balance: number;
  synced = false;

  constructor(public name: string, public currency: string, public createdDate: Date, public modifiedDate: Date) {}
}
