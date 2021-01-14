import { Syncable } from "models/sync/syncable";

export class Category implements Syncable {
  public id: number;
  public synced = false;
  public parent: string;

  constructor(
    public parentId: number,
    public name: string,
    public type: CategoryType,
    public generateUpcomingExpense: boolean,
    public createdDate: Date,
    public modifiedDate: Date
  ) {}
}

export enum CategoryType {
  AllTransactions,
  DepositOnly,
  ExpenseOnly,
}
