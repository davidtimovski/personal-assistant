import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";
import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { SimpleIngredient } from "../models/viewmodels/simpleIngredient";
import { EditIngredientModel } from "../models/viewmodels/editIngredientModel";
import { IngredientSuggestion, PublicIngredientSuggestions } from "../models/viewmodels/ingredientSuggestions";
import { PriceData } from "../models/viewmodels/priceData";
import { TaskSuggestion } from "../models/viewmodels/taskSuggestion";
import { ViewIngredientModel } from "../models/viewmodels/viewIngredientModel";
import * as environment from "../../config/environment.json";

@inject(AuthService, HttpClient, EventAggregator)
export class IngredientsService extends HttpProxyBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator
  ) {
    super(authService, httpClient, eventAggregator);
  }

  getAll(): Promise<SimpleIngredient[]> {
    return this.ajax<SimpleIngredient[]>("ingredients");
  }

  getForUpdate(id: number): Promise<EditIngredientModel> {
    return this.ajax<EditIngredientModel>(`ingredients/${id}/update`);
  }

  getPublic(id: number): Promise<ViewIngredientModel> {
    return this.ajax<ViewIngredientModel>(`ingredients/${id}/public`);
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
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async updatePublic(id: number, taskId: number): Promise<void> {
    try {
      await this.ajaxExecute("ingredients/public", {
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
      await this.ajaxExecute(`ingredients/${id}`, {
        method: "delete",
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  getTaskSuggestions(): Promise<TaskSuggestion[]> {
    return this.ajax<TaskSuggestion[]>("ingredients/task-suggestions");
  }

  getUserIngredientSuggestions(): Promise<IngredientSuggestion[]> {
    return this.ajax<IngredientSuggestion[]>("ingredients/user-suggestions");
  }

  getPublicIngredientSuggestions(): Promise<PublicIngredientSuggestions> {
    return this.ajax<PublicIngredientSuggestions>("ingredients/public-suggestions");
  }
}
