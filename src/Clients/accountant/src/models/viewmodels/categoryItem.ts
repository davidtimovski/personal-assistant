import { Syncable } from "models/sync/syncable";

export class CategoryItem implements Syncable {
  constructor(
    public id: number,
    public name: string,
    public generateUpcomingExpense: boolean,
    public synced: boolean,
    public subCategories: Array<CategoryItem>
  ) {}
}
