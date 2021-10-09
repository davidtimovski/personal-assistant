import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import autocomplete, { AutocompleteResult } from "autocompleter";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import * as environment from "../../config/environment.json";
import { RecipesService } from "services/recipesService";
import { IngredientsService } from "services/ingredientsService";
import { UsersService } from "services/usersService";
import { IngredientSuggestions } from "models/viewmodels/ingredientSuggestions";
import { IngredientSuggestion } from "models/viewmodels/ingredientSuggestion";
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
  private ingredientName: string;
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
  private ingredientIdsToRemove = new Array<number>();
  private autocomplete: AutocompleteResult;
  private suggestions = new Array<IngredientSuggestion>();
  private taskSuggestions = new Array<IngredientSuggestion>();
  private removedSuggestions = new Array<IngredientSuggestion>();
  private ingredientSuggestionsComeFromTasks = false;
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

    this.ingredientsService
      .getSuggestionsForRecipe(this.model.id)
      .then((ingredientSuggestionsVM: IngredientSuggestions) => {
        this.suggestions = ingredientSuggestionsVM.suggestions;
        this.taskSuggestions = ingredientSuggestionsVM.taskSuggestions;

        this.attachAutocomplete(this.suggestions);
      });
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

  attachAutocomplete(ingredientSuggestions: IngredientSuggestion[]) {
    if (this.autocomplete) {
      this.autocomplete.destroy();
    }

    this.autocomplete = autocomplete({
      input: this.addIngredientsInput,
      minLength: 2,
      fetch: (text: string, update: (items: IngredientSuggestion[]) => void) => {
        const suggestions = ingredientSuggestions.filter((i) => i.name.toUpperCase().startsWith(text.toUpperCase()));
        update(suggestions);
      },
      onSelect: (suggestion: IngredientSuggestion) => {
        this.ingredientNameIsInvalid = false;

        if (this.existsInIngredients(suggestion.name)) {
          this.ingredientName = suggestion.name;
          this.ingredientNameIsInvalid = true;
          return;
        }

        if (suggestion.taskId) {
          this.model.ingredients.push(
            new EditRecipeIngredient(
              null,
              suggestion.taskId,
              suggestion.group,
              suggestion.name,
              null,
              null,
              !!suggestion.id
            )
          );
        } else {
          this.model.ingredients.push(
            new EditRecipeIngredient(
              suggestion.id,
              suggestion.taskId,
              null,
              suggestion.name,
              null,
              null,
              !!suggestion.id
            )
          );
        }

        this.suggestions = this.suggestions.filter((x) => x.id !== suggestion.id);
        this.taskSuggestions = this.taskSuggestions.filter(
          (x) => x.taskId !== suggestion.taskId && x.id !== suggestion.id
        );

        if (this.ingredientSuggestionsComeFromTasks) {
          this.attachAutocomplete(this.taskSuggestions);
        } else {
          this.attachAutocomplete(this.suggestions);
        }

        this.ingredientName = "";
        this.removedSuggestions.push(suggestion);
      },
      className: "autocomplete-customizations",
    });
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

  changeIngredientSuggestionsSource() {
    this.autocomplete.destroy();
    this.ingredientNameIsInvalid = false;

    this.ingredientSuggestionsComeFromTasks = !this.ingredientSuggestionsComeFromTasks;

    if (this.ingredientSuggestionsComeFromTasks) {
      this.addIngredientsInputPlaceholder = this.i18n.tr("editRecipe.ingredientFromTask");
      this.attachAutocomplete(this.taskSuggestions);
    } else {
      this.addIngredientsInputPlaceholder = this.i18n.tr("editRecipe.ingredient");
      this.attachAutocomplete(this.suggestions);
    }

    this.addIngredientsInput.focus();
  }

  addNewIngredient() {
    this.ingredientNameIsInvalid = ValidationUtil.isEmptyOrWhitespace(this.ingredientName);
    if (!this.ingredientNameIsInvalid) {
      if (this.existsInIngredients(this.ingredientName)) {
        this.ingredientNameIsInvalid = true;
      } else {
        this.model.ingredients.push(new EditRecipeIngredient(null, null, null, this.ingredientName, null, null, false));
        this.ingredientName = "";
      }
    }
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
    if (ingredient.id) {
      this.ingredientIdsToRemove.push(ingredient.id);
    }
    this.model.ingredients.splice(this.model.ingredients.indexOf(ingredient), 1);

    const ingredientSuggestion = new IngredientSuggestion(
      ingredient.id,
      ingredient.taskId,
      ingredient.name,
      ingredient.unit,
      ingredient.name,
      ingredient.taskList
    );
    this.suggestions.push(ingredientSuggestion);
    if (ingredient.taskId) {
      this.taskSuggestions.push(ingredientSuggestion);
    }

    if (this.ingredientSuggestionsComeFromTasks) {
      this.attachAutocomplete(this.taskSuggestions);
    } else {
      this.attachAutocomplete(this.suggestions);
    }
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
          await this.recipesService.update(this.model, this.ingredientIdsToRemove);

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

  async getMeasuringUnits(): Promise<Array<string>> {
    const preferences = await this.usersService.getPreferences();
    const measuringUnits = preferences.imperialSystem
      ? [null, "oz", "cup", "tbsp", "tsp"]
      : [null, "g", "ml", "tbsp", "tsp"];

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
