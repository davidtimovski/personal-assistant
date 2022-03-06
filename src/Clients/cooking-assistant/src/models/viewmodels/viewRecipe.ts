import { Ingredient } from "./ingredient";
import { NutritionSummary } from "./nutritionSummary";
import { CostSummary } from "./costSummary";
import { SharingState } from "./sharingState";

export class ViewRecipe {
  id: number;
  name: string;
  description: string;
  ingredients: Array<Ingredient>;
  instructions: string;
  prepDuration: string;
  cookDuration: string;
  servings: number;
  imageUri: string;
  videoUrl: string;
  lastOpenedDate: Date;
  nutritionSummary: NutritionSummary;
  costSummary: CostSummary;
  sharingState: SharingState;

  constructor(ingredients: Array<Ingredient>) {
    this.ingredients = ingredients || new Array<Ingredient>();
  }
}
