import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../../../shared/src/utils/httpProxy";
import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";
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
import { RecipeWithShares } from "models/viewmodels/recipeWithShares";
import { CanShareRecipe } from "models/viewmodels/canShareRecipe";
import { ShareRequest } from "models/viewmodels/shareRequest";
import * as Actions from "utils/state/actions";

@autoinject
export class RecipesService {
  constructor(
    private readonly httpProxy: HttpProxy,
    private readonly localStorage: LocalStorageCurrencies,
    private readonly logger: ErrorLogger
  ) {}

  getAll(): Promise<RecipeModel[]> {
    return this.httpProxy.ajax<RecipeModel[]>("api/recipes");
  }

  async get(id: number, currency: string): Promise<ViewRecipe> {
    const result = await this.httpProxy.ajax<ViewRecipe>(`api/recipes/${id}/${currency}`);

    Actions.setDataLastLoad(id, new Date());

    return result;
  }

  getForUpdate(id: number): Promise<EditRecipeModel> {
    return this.httpProxy.ajax<EditRecipeModel>(`api/recipes/${id}/update`);
  }

  getWithShares(id: number): Promise<RecipeWithShares> {
    return this.httpProxy.ajax<RecipeWithShares>(`api/recipes/${id}/with-shares`);
  }

  getShareRequests(): Promise<ShareRequest[]> {
    return this.httpProxy.ajax<ShareRequest[]>("api/recipes/share-requests");
  }

  getPendingShareRequestsCount(): Promise<number> {
    return this.httpProxy.ajax<number>("api/recipes/pending-share-requests-count");
  }

  getForSending(id: number): Promise<SendRecipeModel> {
    return this.httpProxy.ajax<SendRecipeModel>(`api/recipes/${id}/sending`);
  }

  getSendRequests(): Promise<ReceivedRecipe[]> {
    return this.httpProxy.ajax<ReceivedRecipe[]>("api/recipes/send-requests");
  }

  getPendingSendRequestsCount(): Promise<number> {
    return this.httpProxy.ajax<number>("api/recipes/pending-send-requests-count");
  }

  async tryImport(
    id: number,
    ingredientReplacements: IngredientReplacement[],
    checkIfReviewRequired: boolean
  ): Promise<number> {
    try {
      const result = await this.httpProxy.ajax<number>(`api/recipes/try-import`, {
        method: "post",
        body: json({
          id: id,
          ingredientReplacements: ingredientReplacements,
          checkIfReviewRequired: checkIfReviewRequired,
        }),
      });

      await Actions.getRecipes(this);

      return result;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  getForReview(id: number): Promise<ReviewIngredientsModel> {
    return this.httpProxy.ajax<ReviewIngredientsModel>(`api/recipes/${id}/review`);
  }

  async create(
    name: string,
    description: string,
    ingredients: EditRecipeIngredient[],
    instructions: string,
    prepDuration: string,
    cookDuration: string,
    servings: number,
    imageUri: string,
    videoUrl: string
  ): Promise<number> {
    try {
      const parsedIngredients = this.parseIngredientsAmount(ingredients);

      const id = await this.httpProxy.ajax<number>("api/recipes", {
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

      await Actions.getRecipes(this);

      return id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async uploadTempImage(image: File): Promise<string> {
    try {
      const formData = new FormData();
      formData.append("image", image);

      const data: any = await this.httpProxy.ajaxUploadFile("api/recipes/upload-temp-image", {
        method: "post",
        body: formData,
      });

      return data.tempImageUri;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async update(recipe: EditRecipeModel): Promise<void> {
    try {
      const parsedIngredients = this.parseIngredientsAmount(recipe.ingredients);

      await this.httpProxy.ajaxExecute("api/recipes", {
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
        }),
      });

      await Actions.getRecipes(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(`api/recipes/${id}`, {
        method: "delete",
      });

      Actions.deleteRecipe(id);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  canShareRecipeWithUser(email: string): Promise<CanShareRecipe> {
    return this.httpProxy.ajax<CanShareRecipe>(`api/recipes/can-share-with-user/${email}`);
  }

  async share(id: number, newShares: number[], removedShares: number[]): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/recipes/share", {
        method: "put",
        body: json({
          recipeId: id,
          newShares: newShares,
          removedShares: removedShares,
        }),
      });

      await Actions.getRecipes(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async setShareIsAccepted(id: number, isAccepted: boolean): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/recipes/share-is-accepted", {
        method: "put",
        body: json({
          recipeId: id,
          isAccepted: isAccepted,
        }),
      });

      await Actions.getRecipes(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async leave(id: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(`api/recipes/${id}/leave`, {
        method: "delete",
      });

      await Actions.getRecipes(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  canSendRecipeToUser(email: string, recipeId: number): Promise<CanSendRecipe> {
    return this.httpProxy.ajax<CanSendRecipe>(`api/recipes/can-send-recipe-to-user/${email}/${recipeId}`);
  }

  async send(id: number, recipientsIds: number[]): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/recipes/send", {
        method: "post",
        body: json({
          recipeId: id,
          recipientsIds: recipientsIds,
        }),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async declineSendRequest(id: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/recipes/decline-send-request", {
        method: "put",
        body: json({
          recipeId: id,
        }),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async deleteSendRequest(id: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(`api/recipes/${id}/send-request`, {
        method: "delete",
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  copyAsText(
    recipe: ViewRecipe,
    ingredientsLabel: string,
    instructionsLabel: string,
    youTubeUrlLabel: string,
    prepDurationLabel: string,
    minutesLetter: string,
    hoursLetter: string,
    cookDurationLabel: string,
    servingsLabel: string
  ) {
    try {
      let text = recipe.name + "\n----------";

      if (recipe.description) {
        text += "\n\n" + recipe.description;
      }

      if (recipe.ingredients.length > 0) {
        text += `\n\n${ingredientsLabel}:`;

        for (let ingredient of recipe.ingredients) {
          text += `\n◾ ${ingredient.name}`;
          if (ingredient.amount) {
            text += ` - ${ingredient.amount + (ingredient.unit ? " " + ingredient.unit : "")}`;
          }
        }
      }

      if (recipe.instructions) {
        text += `\n\n${instructionsLabel}:`;
        text += "\n----------\n" + recipe.instructions + "\n----------";
      }

      if (recipe.videoUrl) {
        text += `\n\n${youTubeUrlLabel}: ${recipe.videoUrl}`;
      }

      if (recipe.prepDuration || recipe.cookDuration) {
        text += "\n";

        if (recipe.prepDuration) {
          const prepDurationHours = recipe.prepDuration.substring(0, 2);
          const prepDurationMinutes = recipe.prepDuration.substring(3, 5);

          text += `\n${prepDurationLabel}: `;

          if (parseInt(prepDurationHours, 10) === 0) {
            text += parseInt(prepDurationMinutes, 10) + minutesLetter;
          } else {
            text +=
              parseInt(prepDurationHours, 10) + hoursLetter + " " + parseInt(prepDurationMinutes, 10) + minutesLetter;
          }
        }
        if (recipe.cookDuration) {
          const cookDurationHours = recipe.cookDuration.substring(0, 2);
          const cookDurationMinutes = recipe.cookDuration.substring(3, 5);

          text += `\n${cookDurationLabel}: `;

          if (parseInt(cookDurationHours, 10) === 0) {
            text += parseInt(cookDurationMinutes, 10) + minutesLetter;
          } else {
            text +=
              parseInt(cookDurationHours, 10) + hoursLetter + " " + parseInt(cookDurationMinutes, 10) + minutesLetter;
          }
        }
      }

      if (recipe.servings > 1) {
        text += "\n\n" + servingsLabel;
      }

      const textArea = document.createElement("textarea");
      textArea.value = text;
      textArea.style.position = "fixed"; // avoid scrolling to bottom
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();

      document.execCommand("copy");

      document.body.removeChild(textArea);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  videoUrlToEmbedSrc(videoUrl: string): string {
    try {
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
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }

    throw "Invalid url";
  }

  private parseIngredientsAmount(ingredients: EditRecipeIngredient[]) {
    return ingredients.map((ingredient: EditRecipeIngredient) => {
      const parsedIngredient = new EditRecipeIngredient(
        ingredient.id,
        ingredient.name,
        ingredient.amount,
        ingredient.unit,
        ingredient.hasNutritionData,
        ingredient.hasPriceData,
        false
      );

      if (
        parsedIngredient.amount &&
        typeof parsedIngredient.amount === "string" &&
        parsedIngredient.amount.includes("/")
      ) {
        const fractions = parsedIngredient.amount.split("/");
        parsedIngredient.amount = (parseInt(fractions[0], 10) / parseInt(fractions[1], 10)).toFixed(2);
      }

      return parsedIngredient;
    });
  }
}
