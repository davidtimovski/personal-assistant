import { NutritionData } from "./nutritionData";
import { PriceData } from "./priceData";

export class EditIngredientModel {
  constructor(
    public id: number,
    public taskId: number,
    public taskName: string,
    public taskList: string,
    public name: string,
    public nutritionData: NutritionData,
    public priceData: PriceData,
    public recipes: Array<string>
  ) {}
}
