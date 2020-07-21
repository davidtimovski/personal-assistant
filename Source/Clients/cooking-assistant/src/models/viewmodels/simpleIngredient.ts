export class SimpleIngredient {
  constructor(
    public id: number,
    public taskId: number,
    public name: string,
    public hasNutritionData: boolean,
    public hasPriceData: boolean
  ) {}
}
