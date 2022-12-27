import { NutritionData } from "./nutritionData";
import { PriceData } from "./priceData";
import { SimpleRecipe } from "./simpleRecipe";

export class ViewIngredientModel {
  id: number;
  taskId: number;
  taskName: string;
  taskList: string;
  name: string;
  nutritionData: NutritionData;
  priceData: PriceData;
  recipes: SimpleRecipe[];
}
