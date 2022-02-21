import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import * as environment from "../../config/environment.json";
import { RecipesService } from "services/recipesService";
import { IngredientsService } from "services/ingredientsService";
import { UsersService } from "services/usersService";
import { IngredientAutocompleteEvents, IngredientCategory, IngredientSuggestion, IngredientSuggestions, PublicIngredientSuggestions } from "models/viewmodels/ingredientSuggestions";
import { EditRecipeModel } from "models/viewmodels/editRecipeModel";
import { EditRecipeIngredient } from "models/viewmodels/editRecipeIngredient";
import * as Actions from "utils/state/actions";

@inject(Router, RecipesService, IngredientsService, UsersService, ValidationController, I18N, EventAggregator)
export class EditRecipe {
  private model: EditRecipeModel;
  private originalRecipeJson: string;
  private isNewRecipe: boolean;
  private imageInput: HTMLInputElement;
  private imageIsUploading = false;
  private nameInput: HTMLInputElement;
  private nameIsInvalid: boolean;
  private videoUrlIsInvalid: boolean;
  private ingredientName = "";
  private ingredientNameIsInvalid: boolean;
  private prepDurationHours = "00";
  private prepDurationMinutes = "00";
  private cookDurationHours = "00";
  private cookDurationMinutes = "00";
  private ingredientWasChanged: number;
  private confirmationInProgress = false;
  private saveButtonText: string;
  private deleteButtonText: string;
  private leaveButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;
  private leaveButtonIsLoading = false;
  private videoIFrame: HTMLIFrameElement;
  private videoIFrameSrc = "";
  private suggestions: IngredientSuggestions;
  private suggestionsMatched = false;
  private addIngredientsInput: HTMLInputElement;
  private addIngredientsInputPlaceholder: string;
  private measuringUnits: string[];

  constructor(
    private readonly router: Router,
    private readonly recipesService: RecipesService,
    private readonly ingredientsService: IngredientsService,
    private readonly usersService: UsersService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.saveButtonText = this.i18n.tr("save");
    this.deleteButtonText = this.i18n.tr("delete");
    this.leaveButtonText = this.i18n.tr("editRecipe.leave");
    this.addIngredientsInputPlaceholder = this.i18n.tr("editRecipe.ingredient");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.nameIsInvalid = false;
      this.videoUrlIsInvalid = false;
    });

    this.eventAggregator.subscribe(IngredientAutocompleteEvents.Selected, (ingredient: IngredientSuggestion) => {
      this.ingredientName = '';
      this.ingredientNameIsInvalid = false;
      this.resetIngredientMatches();

      this.model.ingredients.push(
        new EditRecipeIngredient(
          ingredient.id,
          ingredient.name,
          null,
          ingredient.unit,
          false
        )
      );
    });
  }

  async activate(params: any) {
    this.isNewRecipe = parseInt(params.id, 10) === 0;

    if (this.isNewRecipe) {
      this.model = new EditRecipeModel();
      this.saveButtonText = this.i18n.tr("editRecipe.create");
    } else {
      this.model = await this.recipesService.getForUpdate(params.id);
      if (this.model === null) {
        this.router.navigateToRoute("notFound");
      }

      this.originalRecipeJson = JSON.stringify(this.model);

      if (this.model.videoUrl) {
        this.videoIFrameSrc = this.recipesService.videoUrlToEmbedSrc(this.model.videoUrl);
      }

      if (this.model.prepDuration) {
        this.prepDurationHours = this.model.prepDuration.substring(0, 2);
        this.prepDurationMinutes = this.model.prepDuration.substring(3, 5);
      }
      if (this.model.cookDuration) {
        this.cookDurationHours = this.model.cookDuration.substring(0, 2);
        this.cookDurationMinutes = this.model.cookDuration.substring(3, 5);
      }
    }

    ValidationRules.ensure((x: EditRecipeModel) => x.name)
      .required()
      .on(this.model);

    this.getMeasuringUnits().then((measuringUnits: string[]) => {
      this.measuringUnits = measuringUnits;
    });
  }

  attached() {
    if (this.isNewRecipe) {
      this.nameInput.focus();
    }

    if (this.model.userIsOwner) {
      const userSuggestionsPromise = this.ingredientsService
        .getUserIngredientSuggestions();

      const publicSuggestionsPromise = this.ingredientsService
        .getPublicIngredientSuggestions();

      Promise.all([userSuggestionsPromise, publicSuggestionsPromise]).then((suggestions) => {
        this.suggestions = new IngredientSuggestions(suggestions[0], suggestions[1]);

        if (!this.isNewRecipe) {
          // Set selected ingredient suggestions
          const ingredientIdsInRecipe: number[] = this.model.ingredients.map(x => x.id);

          for (const id of ingredientIdsInRecipe) {
            const suggestion = this.findSuggestion(id);
            suggestion.selected = true;
          }
        }
      });
    }
  }

  videoUrlChanged() {
    if (!this.model.videoUrl) {
      this.videoUrlIsInvalid = false;
      return;
    }

    try {
      this.videoIFrameSrc = this.recipesService.videoUrlToEmbedSrc(this.model.videoUrl);

      // Hack for back button iframe issue
      this.videoIFrame.contentWindow.location.replace(this.videoIFrameSrc);

      return;
    } catch {
      this.videoUrlIsInvalid = true;
      this.videoIFrameSrc = null;
    }
  }

  resetIngredientMatches() {
    this.suggestions.userIngredients.forEach(x => this.matchIngredient(x, false));
    this.suggestions.publicIngredients.uncategorized.forEach(x => this.matchIngredient(x, false));
    this.suggestions.publicIngredients.categories.forEach(x => this.matchCategory(x, false));

    this.suggestionsMatched = false;
  }

  matchIngredient(ingredient: IngredientSuggestion, matched: boolean) {
    ingredient.matched = matched;
    ingredient.children.forEach(x => this.matchIngredient(x, matched));
  }

  searchIngredient(ingredient: IngredientSuggestion, search: string) {
    ingredient.matched = ingredient.name.toUpperCase().includes(search);

    if (ingredient.matched) {
      this.matchIngredient(ingredient, true);
      this.suggestionsMatched = true;
    } else {
      for (const child of ingredient.children) {
        if (this.searchIngredient(child, search)) {
          ingredient.matched = true;
        }
      }
    }

    return ingredient.matched;
  }

  matchCategory(category: IngredientCategory, matched: boolean) {
    category.matched = matched;
    category.ingredients.forEach(x => this.matchIngredient(x, matched));
    category.subcategories.forEach(x => this.matchCategory(x, matched));
  }

  searchCategory(category: IngredientCategory, search: string) {
    category.matched = category.name.toUpperCase().includes(search);

    if (category.matched) {
      this.matchCategory(category, true);
      this.suggestionsMatched = true;
    } else {
      for (const ingredient of category.ingredients) {
        if (this.searchIngredient(ingredient, search)) {
          category.matched = true;
        }
      }

      for (const subcategory of category.subcategories) {
        if (this.searchCategory(subcategory, search)) {
          category.matched = true;
        }
      }
    }

    return category.matched;
  }

  ingredientInputChanged() {
    this.resetIngredientMatches();

    const search = this.ingredientName.trim().toUpperCase();
    if (!search || search.length < 3) {
      return;
    }

    this.suggestions.userIngredients.forEach(x => this.searchIngredient(x, search));
    this.suggestions.publicIngredients.uncategorized.forEach(x => this.searchIngredient(x, search));
    this.suggestions.publicIngredients.categories.forEach(x => this.searchCategory(x, search));
  }

  ingredientChanged() {
    this.ingredientWasChanged = window.Math.random();
  }

  prepDurationChanged() {
    const prepDurationHours = !this.prepDurationHours
      ? "00"
      : parseInt(this.prepDurationHours, 10).toLocaleString("en-US", {
          minimumIntegerDigits: 2,
        });
    const prepDurationMinutes = !this.prepDurationMinutes
      ? "00"
      : parseInt(this.prepDurationMinutes, 10).toLocaleString("en-US", {
          minimumIntegerDigits: 2,
        });

    if (prepDurationHours === "00" && prepDurationMinutes === "00") {
      this.model.prepDuration = "";
    } else {
      this.model.prepDuration = `${prepDurationHours}:${prepDurationMinutes}`;
    }
  }

  cookDurationChanged() {
    const cookDurationHours = !this.cookDurationHours
      ? "00"
      : parseInt(this.cookDurationHours, 10).toLocaleString("en-US", {
          minimumIntegerDigits: 2,
        });
    const cookDurationMinutes = !this.cookDurationMinutes
      ? "00"
      : parseInt(this.cookDurationMinutes, 10).toLocaleString("en-US", {
          minimumIntegerDigits: 2,
        });

    if (cookDurationHours === "00" && cookDurationMinutes === "00") {
      this.model.cookDuration = "";
    } else {
      this.model.cookDuration = `${cookDurationHours}:${cookDurationMinutes}`;
    }
  }

  @computedFrom(
    "model.name",
    "model.description",
    "model.ingredients.length",
    "ingredientWasChanged",
    "model.instructions",
    "model.cookDuration",
    "model.prepDuration",
    "model.servings",
    "model.imageUri",
    "model.videoUrl",
    "videoUrlIsInvalid"
  )
  get canSave(): boolean {
    if (this.isNewRecipe) {
      return !ValidationUtil.isEmptyOrWhitespace(this.model.name) && !this.videoUrlIsInvalid;
    }

    return (
      !ValidationUtil.isEmptyOrWhitespace(this.model.name) &&
      !this.videoUrlIsInvalid &&
      JSON.stringify(this.model) !== this.originalRecipeJson
    );
  }

  @computedFrom("model.ingredients.length")
  get getIngredientsCount(): number {
    return this.model.ingredients.length;
  }

  addNewIngredient() {
    this.ingredientNameIsInvalid = ValidationUtil.isEmptyOrWhitespace(this.ingredientName);

    if (!this.ingredientNameIsInvalid) {
      if (this.existsInIngredients(this.ingredientName)) {
        this.ingredientNameIsInvalid = true;
      } else {
        this.model.ingredients.push(new EditRecipeIngredient(null, this.ingredientName, null, null, true));
        this.ingredientName = "";
      }
    }

    this.resetIngredientMatches();
  }

  existsInIngredients(ingredientName: string): boolean {
    const duplicates = this.model.ingredients.filter((i) => {
      return i.name.trim().toUpperCase() === ingredientName.trim().toUpperCase();
    });
    return duplicates.length > 0;
  }

  toggleUnit(ingredient: EditRecipeIngredient) {
    const index = this.measuringUnits.indexOf(ingredient.unit);
    ingredient.unit =
      index === this.measuringUnits.length - 1 ? this.measuringUnits[0] : this.measuringUnits[index + 1];

    // Hack to activate the this.canSave() getter
    if (this.model.instructions) {
      this.model.instructions = this.model.instructions + " ";
      this.model.instructions = this.model.instructions.trim();
    } else {
      this.model.instructions = " ";
      this.model.instructions = null;
    }
  }

  removeIngredient(ingredient: EditRecipeIngredient) {
    if (!ingredient.isNew) {
      const suggestion = this.findSuggestion(ingredient.id);
      suggestion.selected = false;    
    }
    
    this.model.ingredients.splice(this.model.ingredients.indexOf(ingredient), 1);
  }

  @computedFrom("model.imageUri")
  get imageIsNotDefault(): boolean {
    return this.model.imageUri !== JSON.parse(<any>environment).defaultRecipeImageUri;
  }

  async uploadImage() {
    this.imageIsUploading = true;
    try {
      const data = await this.recipesService.uploadTempImage(this.imageInput.files[0]);
      this.model.imageUri = data.tempImageUri;
    } catch (e) {
      this.saveButtonIsLoading = false;
    }
    this.imageIsUploading = false;
  }

  removeImage() {
    this.model.imageUri = JSON.parse(<any>environment).defaultRecipeImageUri;
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const result: ControllerValidateResult = await this.validationController.validate();

    this.nameIsInvalid = !result.valid;

    if (result.valid) {
      if (!this.isNewRecipe) {
        try {
          await this.recipesService.update(this.model);

          this.nameIsInvalid = false;

          await Actions.getRecipes(this.recipesService);

          this.router.navigateToRoute("recipesEdited", {
            editedId: this.model.id,
          });
        } catch (errorFields) {
          this.nameIsInvalid = errorFields.includes("Name");
          this.saveButtonIsLoading = false;
        }
      } else {
        try {
          const id = await this.recipesService.create(
            this.model.name,
            this.model.description,
            this.model.ingredients,
            this.model.instructions,
            this.model.prepDuration,
            this.model.cookDuration,
            this.model.servings,
            this.model.imageUri,
            this.model.videoUrl
          );

          this.nameIsInvalid = false;
          this.model.id = id;

          await Actions.getRecipes(this.recipesService);

          this.router.navigateToRoute("recipesEdited", {
            editedId: this.model.id,
          });
        } catch (errorFields) {
          this.nameIsInvalid = errorFields.includes("Name");
          this.saveButtonIsLoading = false;
        }
      }
    } else {
      this.saveButtonIsLoading = false;
    }
  }

  async delete() {
    if (this.deleteButtonIsLoading) {
      return;
    }

    if (this.confirmationInProgress) {
      this.deleteButtonIsLoading = true;

      await this.recipesService.delete(this.model.id);

      Actions.deleteRecipe(this.model.id);

      this.eventAggregator.publish(AlertEvents.ShowSuccess, "editRecipe.deleteSuccessful");
      this.router.navigateToRoute("recipes");
    } else {
      this.deleteButtonText = this.i18n.tr("sure");
      this.confirmationInProgress = true;
    }
  }

  async leave() {
    if (this.leaveButtonIsLoading) {
      return;
    }

    if (this.confirmationInProgress) {
      this.leaveButtonIsLoading = true;

      await this.recipesService.leave(this.model.id);

      await Actions.getRecipes(this.recipesService);

      this.eventAggregator.publish(AlertEvents.ShowSuccess, "editRecipe.youHaveLeftTheRecipe");

      this.router.navigateToRoute("recipes");
    } else {
      this.leaveButtonText = this.i18n.tr("sure");
      this.confirmationInProgress = true;
    }
  }

  cancel() {
    if (!this.confirmationInProgress) {
      if (this.isNewRecipe) {
        this.router.navigateToRoute("recipes");
      } else {
        this.router.navigateToRoute("recipe", { id: this.model.id });
      }
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.confirmationInProgress = false;
  }

  findSuggestion(ingredientId: number) {
    let foundSuggestion = this.suggestions.userIngredients.find(x => x.id === ingredientId);
    if (foundSuggestion) {
      return foundSuggestion;
    }

    for (const suggestion of this.suggestions.publicIngredients.uncategorized) {
      foundSuggestion = this.findInIngredientRecursive(suggestion, ingredientId);
      if (foundSuggestion) {
        return foundSuggestion;
      }
    }

    for (const category of this.suggestions.publicIngredients.categories) {
      for (const suggestion of category.ingredients) {
        foundSuggestion = this.findInIngredientRecursive(suggestion, ingredientId);
        if (foundSuggestion) {
          return foundSuggestion;
        }
      }

      for (const subcategory of category.subcategories) {
        for (const suggestion of subcategory.ingredients) {
          foundSuggestion = this.findInIngredientRecursive(suggestion, ingredientId);
          if (foundSuggestion) {
            return foundSuggestion;
          }
        }
      }
    }

    throw 'Could not find suggestion';
  }

  findInIngredientRecursive(ingredient: IngredientSuggestion, ingredientId: number) {
    if (ingredient.id === ingredientId) {
      return ingredient;
    }

    let result = null;
    for (let i = 0; result == null && i < ingredient.children.length; i++){
      result = this.findInIngredientRecursive(ingredient.children[i], ingredientId);
    }
    return result;
  }

  async getMeasuringUnits(): Promise<Array<string>> {
    const preferences = await this.usersService.getPreferences();
    const measuringUnits = preferences.imperialSystem
      ? [null, "oz", "cup", "tbsp", "tsp", "pinch"]
      : [null, "g", "ml", "tbsp", "tsp", "pinch"];

    if (!this.isNewRecipe) {
      const metricUnits = ["g", "ml"];
      const imperialUnits = ["oz", "cup"];

      if (preferences.imperialSystem) {
        const hasMetricUnitIngredients = this.model.ingredients.filter((x) => metricUnits.includes(x.unit)).length > 0;
        if (hasMetricUnitIngredients) {
          measuringUnits.push(...metricUnits);
        }
      } else {
        const hasImperialUnitIngredients =
          this.model.ingredients.filter((x) => imperialUnits.includes(x.unit)).length > 0;
        if (hasImperialUnitIngredients) {
          measuringUnits.push(...imperialUnits);
        }
      }
    }

    return measuringUnits;
  }
}
