import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";
import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";
import { Language } from "../../../shared/src/models/enums/language";

import { RecipeModel } from "models/viewmodels/recipeModel";
import { ViewRecipe } from "models/viewmodels/viewRecipe";
import { EditRecipeIngredient } from "models/viewmodels/editRecipeIngredient";
import { EditRecipeModel } from "models/viewmodels/editRecipeModel";
import { SendRecipeModel } from "models/viewmodels/sendRecipeModel";
import { CanSendRecipe } from "models/viewmodels/canSendRecipe";
import { ReceivedRecipe } from "models/viewmodels/receivedRecipe";
import { ReviewIngredientsModel } from "models/viewmodels/reviewIngredientsModel";
import { IngredientReplacement } from "models/viewmodels/ingredientReplacement";
import * as Actions from "utils/state/actions";

@inject(AuthService, HttpClient, EventAggregator, LocalStorageCurrencies)
export class RecipesService extends HttpProxyBase {
  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator,
    private readonly localStorage: LocalStorageCurrencies
  ) {
    super(authService, httpClient, eventAggregator);
  }

  async getAll(): Promise<Array<RecipeModel>> {
    const result = await this.ajax<Array<RecipeModel>>("recipes");

    return result;
  }

  async get(id: number, currency: string): Promise<ViewRecipe> {
    const result = await this.ajax<ViewRecipe>(`recipes/${id}/${currency}`);

    Actions.setDataLastLoad(id, new Date());

    return result;
  }

  async getForUpdate(id: number): Promise<EditRecipeModel> {
    const result = await this.ajax<EditRecipeModel>(`recipes/${id}/update`);

    return result;
  }

  async getForSending(id: number): Promise<SendRecipeModel> {
    const result = await this.ajax<SendRecipeModel>(`recipes/${id}/sending`);

    return result;
  }

  async getSendRequests(): Promise<Array<ReceivedRecipe>> {
    const result = await this.ajax<Array<ReceivedRecipe>>(
      "recipes/send-requests"
    );

    return result;
  }

  async getPendingSendRequestsCount(): Promise<number> {
    const result = await this.ajax<number>(
      "recipes/pending-send-requests-count"
    );

    return result;
  }

  async tryImport(
    id: number,
    ingredientReplacements: Array<IngredientReplacement>,
    checkIfReviewRequired: boolean
  ): Promise<number> {
    const result = await this.ajax<number>(`recipes/try-import`, {
      method: "post",
      body: json({
        id: id,
        ingredientReplacements: ingredientReplacements,
        checkIfReviewRequired: checkIfReviewRequired,
      }),
    });

    return result;
  }

  async getForReview(id: number): Promise<ReviewIngredientsModel> {
    const result = await this.ajax<ReviewIngredientsModel>(
      `recipes/${id}/review`
    );

    return result;
  }

  async create(
    name: string,
    description: string,
    ingredients: Array<EditRecipeIngredient>,
    instructions: string,
    prepDuration: string,
    cookDuration: string,
    servings: number,
    imageUri: string,
    videoUrl: string
  ): Promise<number> {
    const parsedIngredients = this.parseIngredientsAmount(ingredients);

    const id = await this.ajax<number>("recipes", {
      method: "post",
      body: json({
        name: name,
        description: description,
        ingredients: parsedIngredients,
        instructions: instructions,
        prepDuration: prepDuration,
        cookDuration: cookDuration,
        servings: servings,
        imageUri: imageUri,
        videoUrl: videoUrl,
      }),
    });

    return id;
  }

  async uploadTempImage(image: File): Promise<any> {
    const formData = new FormData();
    formData.append("image", image);

    const data = await this.ajaxUploadFile("recipes/upload-temp-image", {
      method: "post",
      body: formData,
    });

    return data;
  }

  async update(
    recipe: EditRecipeModel,
    ingredientIdsToRemove: Array<number>
  ): Promise<void> {
    const parsedIngredients = this.parseIngredientsAmount(recipe.ingredients);

    await this.ajaxExecute("recipes", {
      method: "put",
      body: json({
        id: recipe.id,
        name: recipe.name,
        description: recipe.description,
        ingredients: parsedIngredients,
        instructions: recipe.instructions,
        prepDuration: recipe.prepDuration,
        cookDuration: recipe.cookDuration,
        servings: recipe.servings,
        imageUri: recipe.imageUri,
        videoUrl: recipe.videoUrl,
        ingredientIdsToRemove: ingredientIdsToRemove,
      }),
    });
  }

  async delete(id: number): Promise<void> {
    await this.ajaxExecute(`recipes/${id}`, {
      method: "delete",
    });
  }

  async canSendRecipeToUser(
    email: string,
    recipeId: number
  ): Promise<CanSendRecipe> {
    const result = await this.ajax<CanSendRecipe>(
      `recipes/can-send-recipe-to-user/${email}/${recipeId}`
    );

    return result;
  }

  async send(id: number, recipientsIds: Array<number>): Promise<void> {
    await this.ajaxExecute("recipes/send", {
      method: "post",
      body: json({
        recipeId: id,
        recipientsIds: recipientsIds,
      }),
    });
  }

  async declineSendRequest(id: number): Promise<void> {
    await this.ajaxExecute("recipes/decline-send-request", {
      method: "put",
      body: json({
        recipeId: id,
      }),
    });
  }

  async deleteSendRequest(id: number): Promise<void> {
    await this.ajaxExecute(`recipes/${id}/send-request`, {
      method: "delete",
    });
  }

  videoUrlToEmbedSrc(videoUrl: string): string {
    if (videoUrl.includes("youtube.com") || videoUrl.includes("youtu.be")) {
      const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
      const match = videoUrl.match(regExp);

      if (match && match[2].length == 11) {
        const id = match[2];

        const language = this.localStorage.getLanguage();
        const hl = language == Language.English ? "en" : "mk";

        return `//www.youtube.com/embed/${id}?disablekb=1&hl=${hl}&iv_load_policy=3&loop=1&modestbranding=1&rel=0`;
      }
    }

    throw "Invalid url";
  }

  private parseIngredientsAmount(ingredients: Array<EditRecipeIngredient>) {
    return ingredients.map((ingredient: EditRecipeIngredient) => {
      const parsedIngredient = new EditRecipeIngredient(
        ingredient.id,
        ingredient.taskId,
        null,
        ingredient.name,
        ingredient.amount,
        ingredient.unit,
        false
      );

      if (
        parsedIngredient.amount &&
        typeof parsedIngredient.amount === "string" &&
        parsedIngredient.amount.includes("/")
      ) {
        const fractions = parsedIngredient.amount.split("/");
        parsedIngredient.amount = (
          parseInt(fractions[0], 10) / parseInt(fractions[1], 10)
        ).toFixed(2);
      }

      return parsedIngredient;
    });
  }
}
