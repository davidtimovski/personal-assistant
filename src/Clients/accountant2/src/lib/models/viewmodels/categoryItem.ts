import { CategoryType } from "models/entities/category";
import { Syncable } from "models/syncable";

export class CategoryItem implements Syncable {
  constructor(
    public id: number,
    public name: string,
    public type: CategoryType,
    public generateUpcomingExpense: boolean,
    public synced: boolean,
    public subCategories: CategoryItem[]
  ) {}
}
