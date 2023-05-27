import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../../../shared/src/utils/httpProxy";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { SimpleIngredient } from "models/viewmodels/simpleIngredient";
import { EditIngredientModel } from "models/viewmodels/editIngredientModel";
import { IngredientSuggestion, PublicIngredientSuggestions } from "models/viewmodels/ingredientSuggestions";
import { PriceData } from "models/viewmodels/priceData";
import { TaskSuggestion } from "models/viewmodels/taskSuggestion";
import { ViewIngredientModel } from "models/viewmodels/viewIngredientModel";

@autoinject
export class IngredientsService {
  constructor(private readonly httpProxy: HttpProxy, private readonly logger: ErrorLogger) {}

  getAll(): Promise<SimpleIngredient[]> {
    return this.httpProxy.ajax<SimpleIngredient[]>("api/ingredients");
  }

  getForUpdate(id: number): Promise<EditIngredientModel> {
    return this.httpProxy.ajax<EditIngredientModel>(`api/ingredients/${id}/update`);
  }

  getPublic(id: number): Promise<ViewIngredientModel> {
    return this.httpProxy.ajax<ViewIngredientModel>(`api/ingredients/${id}/public`);
  }

  async update(ingredient: EditIngredientModel): Promise<void> {
    try {
      const priceDataToSend = new PriceData(
        ingredient.priceData.isSet,
        ingredient.priceData.productSize,
        ingredient.priceData.productSizeIsOneUnit,
        ingredient.priceData.price,
        ingredient.priceData.isSet && ingredient.priceData.price ? ingredient.priceData.currency : null
      );

      await this.httpProxy.ajaxExecute("api/ingredients", {
        method: "put",
        body: json({
          id: ingredient.id,
          taskId: ingredient.taskId,
          name: ingredient.name,
          nutritionData: ingredient.nutritionData,
          priceData: priceDataToSend,
        }),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async updatePublic(id: number, taskId: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/ingredients/public", {
        method: "put",
        body: json({
          id: id,
          taskId: taskId,
        }),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(`api/ingredients/${id}`, {
        method: "delete",
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  getTaskSuggestions(): Promise<TaskSuggestion[]> {
    return this.httpProxy.ajax<TaskSuggestion[]>("api/ingredients/task-suggestions");
  }

  getUserIngredientSuggestions(): Promise<IngredientSuggestion[]> {
    return this.httpProxy.ajax<IngredientSuggestion[]>("api/ingredients/user-suggestions");
  }

  getPublicIngredientSuggestions(): Promise<PublicIngredientSuggestions> {
    return this.httpProxy.ajax<PublicIngredientSuggestions>("api/ingredients/public-suggestions");
  }
}
