import { Ingredient } from "./ingredient";
import { NutritionSummary } from "./nutritionSummary";
import { CostSummary } from "./costSummary";

export class ViewRecipe {
  constructor(
    public id: number,
    public name: string,
    public description: string,
    public ingredients: Array<Ingredient>,
    public instructions: string,
    public prepDuration: string,
    public cookDuration: string,
    public servings: number,
    public imageUri: string,
    public videoUrl: string,
    public lastOpenedDate: Date,
    public nutritionSummary: NutritionSummary,
    public costSummary: CostSummary
  ) {
    this.ingredients = ingredients || new Array<Ingredient>();
  }
}
