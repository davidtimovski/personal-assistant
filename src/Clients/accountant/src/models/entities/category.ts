import { Syncable } from "models/sync/syncable";

export class Category implements Syncable {
  synced = false;
  parent: Category;

  constructor(
    public id: number,
    public parentId: number,
    public name: string,
    public type: CategoryType,
    public generateUpcomingExpense: boolean,
    public isTax: boolean,
    public createdDate: Date,
    public modifiedDate: Date
  ) {}

  get fullName(): string {
    return this.parent ? `${this.parent.name}/${this.name}` : this.name;
  }
}

export enum CategoryType {
  AllTransactions,
  DepositOnly,
  ExpenseOnly,
}
