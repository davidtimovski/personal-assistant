export class IngredientReviewSuggestion {
  constructor(
    public id: number,
    public name: string,
    public label: string,
    public hasNutritionData: boolean,
    public hasPriceData: boolean
  ) {}
}
