import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";
import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";

import { SimpleIngredient } from "models/viewmodels/simpleIngredient";
import { EditIngredientModel } from "models/viewmodels/editIngredientModel";
import { IngredientSuggestions } from "models/viewmodels/ingredientSuggestions";
import { IngredientSuggestion } from "models/viewmodels/ingredientSuggestion";
import { PriceData } from "models/viewmodels/priceData";

@inject(AuthService, HttpClient, EventAggregator)
export class IngredientsService extends HttpProxyBase {
  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator
  ) {
    super(authService, httpClient, eventAggregator);
  }

  async getAll(): Promise<Array<SimpleIngredient>> {
    const result = await this.ajax<Array<SimpleIngredient>>("ingredients");
    return result;
  }

  async get(id: number): Promise<EditIngredientModel> {
    const result = await this.ajax<EditIngredientModel>(`ingredients/${id}`);
    return result;
  }

  async update(ingredient: EditIngredientModel): Promise<void> {
    const priceDataToSend = new PriceData(
      ingredient.priceData.isSet,
      ingredient.priceData.productSize,
      ingredient.priceData.productSizeIsOneUnit,
      ingredient.priceData.price,
      ingredient.priceData.isSet && ingredient.priceData.price
        ? ingredient.priceData.currency
        : null
    );

    await this.ajaxExecute("ingredients", {
      method: "put",
      body: json({
        id: ingredient.id,
        taskId: ingredient.taskId,
        name: ingredient.name,
        nutritionData: ingredient.nutritionData,
        priceData: priceDataToSend,
      }),
    });
  }

  async delete(id: number): Promise<void> {
    await this.ajaxExecute(`ingredients/${id}`, {
      method: "delete",
    });
  }

  async getTaskSuggestions(): Promise<Array<IngredientSuggestion>> {
    const result = await this.ajax<Array<IngredientSuggestion>>(
      "ingredients/task-suggestions"
    );

    return result;
  }

  async getSuggestionsForRecipe(
    recipeId: number
  ): Promise<IngredientSuggestions> {
    const result = await this.ajax<IngredientSuggestions>(
      `ingredients/suggestions/${recipeId}`
    );

    return result;
  }
}
