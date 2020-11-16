export class ReviewIngredient {
  constructor(
    public id: number,
    public name: string,
    public hasNutritionData: boolean,
    public hasPriceData: boolean,
    public replacementId: number,
    public replacementName: string,
    public transferNutritionData: boolean,
    public transferPriceData: boolean
  ) {}
}
