import { Syncable } from "models/sync/syncable";
import { Category } from "./category";

export class TransactionModel implements Syncable {
  public accountName: string;
  public category: Category;
  public convertedAmount: number;
  public synced = false;

  constructor(
    public id: number,
    public fromAccountId: number,
    public toAccountId: number,
    public categoryId: number,
    public amount: number,
    public fromStocks: number,
    public toStocks: number,
    public currency: string,
    public description: string,
    public date: string,
    public isEncrypted: boolean,
    public encryptedDescription: string,
    public salt: string,
    public nonce: string,
    public createdDate: Date,
    public modifiedDate: Date
  ) {}

  get isTax() {
    return this.category?.isTax;
  }
}
