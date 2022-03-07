import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";
import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";

import { SimpleIngredient } from "../models/viewmodels/simpleIngredient";
import { EditIngredientModel } from "../models/viewmodels/editIngredientModel";
import { IngredientSuggestion, PublicIngredientSuggestions } from "../models/viewmodels/ingredientSuggestions";
import { PriceData } from "../models/viewmodels/priceData";
import { TaskSuggestion } from "../models/viewmodels/taskSuggestion";
import { ViewIngredientModel } from "../models/viewmodels/viewIngredientModel";

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

  async getForUpdate(id: number): Promise<EditIngredientModel> {
    const result = await this.ajax<EditIngredientModel>(`ingredients/${id}/update`);
    return result;
  }

  async getPublic(id: number): Promise<ViewIngredientModel> {
    const result = await this.ajax<ViewIngredientModel>(`ingredients/${id}/public`);
    return result;
  }

  async update(ingredient: EditIngredientModel): Promise<void> {
    const priceDataToSend = new PriceData(
      ingredient.priceData.isSet,
      ingredient.priceData.productSize,
      ingredient.priceData.productSizeIsOneUnit,
      ingredient.priceData.price,
      ingredient.priceData.isSet && ingredient.priceData.price ? ingredient.priceData.currency : null
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

  async updatePublic(id: number, taskId: number): Promise<void> {
    await this.ajaxExecute("ingredients/public", {
      method: "put",
      body: json({
        id: id,
        taskId: taskId,
      }),
    });
  }

  async delete(id: number): Promise<void> {
    await this.ajaxExecute(`ingredients/${id}`, {
      method: "delete",
    });
  }

  async getTaskSuggestions(): Promise<Array<TaskSuggestion>> {
    const result = await this.ajax<Array<TaskSuggestion>>("ingredients/task-suggestions");

    return result;
  }

  async getUserIngredientSuggestions(): Promise<Array<IngredientSuggestion>> {
    const result = await this.ajax<Array<IngredientSuggestion>>("ingredients/user-suggestions");

    return result;
  }

  async getPublicIngredientSuggestions(): Promise<PublicIngredientSuggestions> {
    const result = await this.ajax<PublicIngredientSuggestions>("ingredients/public-suggestions");

    return result;
  }
}
