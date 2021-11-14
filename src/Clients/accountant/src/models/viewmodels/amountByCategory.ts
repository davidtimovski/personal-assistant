export class AmountByCategory {
  public subItems: Array<AmountByCategory>;

  constructor(
    public categoryId: number,
    public parentCategoryId: number,
    public categoryName: string,
    public amount: number
  ) {
    this.subItems = [];
  }
}
