import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { IngredientsService } from "services/ingredientsService";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import {
  ValidationController,
  validateTrigger,
  ValidationRules,
  ControllerValidateResult,
  ValidateResult,
} from "aurelia-validation";
import autocomplete, { AutocompleteResult } from "autocompleter";

import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { EditIngredientModel } from "models/viewmodels/editIngredientModel";
import { IngredientSuggestion } from "models/viewmodels/ingredientSuggestion";
import { NutritionData } from "models/viewmodels/nutritionData";
import { PriceData } from "models/viewmodels/priceData";

@inject(
  Router,
  IngredientsService,
  ValidationController,
  I18N,
  EventAggregator,
  LocalStorageCurrencies
)
export class EditIngredient {
  private ingredientId: number;
  private model: EditIngredientModel;
  private originalIngredientJson: string;
  private ingredientLinkedMessage: string;
  private tasksSearchVisible = false;
  private autocomplete: AutocompleteResult;
  private taskSuggestions = new Array<IngredientSuggestion>();
  private pickTaskInput: HTMLInputElement;
  private nameIsInvalid: boolean;
  private caloriesIsInvalid: boolean;
  private fatIsInvalid: boolean;
  private saturatedFatIsInvalid: boolean;
  private carbohydrateIsInvalid: boolean;
  private sugarsIsInvalid: boolean;
  private addedSugarsIsInvalid: boolean;
  private fiberIsInvalid: boolean;
  private proteinIsInvalid: boolean;
  private sodiumIsInvalid: boolean;
  private cholesterolIsInvalid: boolean;
  private vitaminAIsInvalid: boolean;
  private vitaminCIsInvalid: boolean;
  private vitaminDIsInvalid: boolean;
  private calciumIsInvalid: boolean;
  private ironIsInvalid: boolean;
  private potassiumIsInvalid: boolean;
  private magnesiumIsInvalid: boolean;
  private priceIsInvalid: boolean;
  private currency: string;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly ingredientsService: IngredientsService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly localStorage: LocalStorageCurrencies
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.nameIsInvalid = false;
      this.caloriesIsInvalid = false;
      this.fatIsInvalid = false;
      this.saturatedFatIsInvalid = false;
      this.carbohydrateIsInvalid = false;
      this.sugarsIsInvalid = false;
      this.addedSugarsIsInvalid = false;
      this.fiberIsInvalid = false;
      this.proteinIsInvalid = false;
      this.sodiumIsInvalid = false;
      this.cholesterolIsInvalid = false;
      this.vitaminAIsInvalid = false;
      this.vitaminCIsInvalid = false;
      this.vitaminDIsInvalid = false;
      this.calciumIsInvalid = false;
      this.ironIsInvalid = false;
      this.potassiumIsInvalid = false;
      this.magnesiumIsInvalid = false;
      this.priceIsInvalid = false;
    });
  }

  activate(params: any) {
    this.ingredientId = parseInt(params.id, 10);

    this.currency = this.localStorage.getCurrency();
  }

  attached() {
    this.ingredientsService
      .get(this.ingredientId)
      .then((ingredient: EditIngredientModel) => {
        if (ingredient === null) {
          this.router.navigateToRoute("notFound");
        }
        this.model = ingredient;

        if (this.model.taskId) {
          this.ingredientLinkedMessage = this.i18n.tr("editIngredient.thisIngredientIsLinked", {
            taskName: this.model.name,
            taskList: this.model.taskList
          });
        }

        if (!this.model.priceData.isSet) {
          this.model.priceData.currency = this.currency;
        }

        this.originalIngredientJson = JSON.stringify(this.model);

        const gTo = 1000;
        const gToLabel = gTo - 1;
        const mgTo = 20000;
        const mgToLabel = mgTo - 1;
        ValidationRules.ensure((x: EditIngredientModel) => x.name)
          .required()
          .on(this.model);

        ValidationRules.ensure((x: NutritionData) => x.calories)
          .between(-1, gTo)
          .withMessage(
            this.i18n.tr("caloriesBetween", { from: 0, to: gToLabel })
          )
          .ensure((x: NutritionData) => x.fat)
          .between(-1, gTo)
          .withMessage(this.i18n.tr("fatBetween", { from: 0, to: gToLabel }))
          .ensure((x: NutritionData) => x.saturatedFat)
          .between(-1, gTo)
          .withMessage(
            this.i18n.tr("saturatedFatBetween", { from: 0, to: gToLabel })
          )
          .ensure((x: NutritionData) => x.carbohydrate)
          .between(-1, gTo)
          .withMessage(
            this.i18n.tr("carbohydrateBetween", { from: 0, to: gToLabel })
          )
          .ensure((x: NutritionData) => x.sugars)
          .between(-1, gTo)
          .withMessage(this.i18n.tr("sugarsBetween", { from: 0, to: gToLabel }))
          .ensure((x: NutritionData) => x.addedSugars)
          .between(-1, gTo)
          .withMessage(
            this.i18n.tr("addedSugarsBetween", { from: 0, to: gToLabel })
          )
          .ensure((x: NutritionData) => x.fiber)
          .between(-1, gTo)
          .withMessage(this.i18n.tr("fiberBetween", { from: 0, to: gToLabel }))
          .ensure((x: NutritionData) => x.protein)
          .between(-1, gTo)
          .withMessage(
            this.i18n.tr("proteinBetween", { from: 0, to: gToLabel })
          )
          .ensure((x: NutritionData) => x.sodium)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("sodiumBetween", { from: 0, to: mgToLabel })
          )
          .ensure((x: NutritionData) => x.cholesterol)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("cholesterolBetween", { from: 0, to: mgToLabel })
          )
          .ensure((x: NutritionData) => x.vitaminA)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("vitaminABetween", { from: 0, to: mgToLabel })
          )
          .ensure((x: NutritionData) => x.vitaminC)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("vitaminCBetween", { from: 0, to: mgToLabel })
          )
          .ensure((x: NutritionData) => x.vitaminD)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("vitaminDBetween", { from: 0, to: mgToLabel })
          )
          .ensure((x: NutritionData) => x.calcium)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("calciumBetween", { from: 0, to: mgToLabel })
          )
          .ensure((x: NutritionData) => x.iron)
          .between(-1, mgTo)
          .withMessage(this.i18n.tr("ironBetween", { from: 0, to: mgToLabel }))
          .ensure((x: NutritionData) => x.potassium)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("potassiumBetween", { from: 0, to: mgToLabel })
          )
          .ensure((x: NutritionData) => x.magnesium)
          .between(-1, mgTo)
          .withMessage(
            this.i18n.tr("magnesiumBetween", { from: 0, to: mgToLabel })
          )
          .on(this.model.nutritionData);

        ValidationRules.ensure((x: PriceData) => x.price)
          .between(0, mgTo)
          .withMessage(this.i18n.tr("editIngredient.priceBetween"))
          .on(this.model.priceData);
      });
  }

  async linkToTask() {
    this.taskSuggestions = await this.ingredientsService.getTaskSuggestions();
    this.tasksSearchVisible = true;

    this.attachAutocomplete(this.taskSuggestions);
  }

  attachAutocomplete(ingredientSuggestions: IngredientSuggestion[]) {
    if (this.autocomplete) {
      this.autocomplete.destroy();
    }

    this.autocomplete = autocomplete({
      input: this.pickTaskInput,
      minLength: 2,
      fetch: (
        text: string,
        update: (items: IngredientSuggestion[]) => void
      ) => {
        const suggestions = ingredientSuggestions.filter((i) =>
          i.name.toUpperCase().startsWith(text.toUpperCase())
        );
        update(suggestions);
      },
      onSelect: (suggestion: IngredientSuggestion) => {
        this.nameIsInvalid = false;

        this.ingredientLinkedMessage = this.i18n.tr("editIngredient.thisIngredientIsLinked", {
          taskName: suggestion.name,
          taskList: suggestion.group
        });

        this.model.taskId = suggestion.taskId;
        this.model.name = suggestion.name;

        this.pickTaskInput.value = "";
      },
      className: "autocomplete-customizations",
    });
  }

  unlinkFromTask() {
    this.model.taskId = null;
    this.tasksSearchVisible = false;

    const originalName = JSON.parse(this.originalIngredientJson).name;
    if (this.model.name !== originalName) {
      this.model.name = originalName;
    }
  }

  setNutritionData() {
    this.model.nutritionData.isSet = true;
  }

  removeNutritionData() {
    this.model.nutritionData.isSet = false;

    this.model.nutritionData.calories = null;
    this.model.nutritionData.fat = null;
    this.model.nutritionData.saturatedFat = null;
    this.model.nutritionData.carbohydrate = null;
    this.model.nutritionData.sugars = null;
    this.model.nutritionData.addedSugars = null;
    this.model.nutritionData.fiber = null;
    this.model.nutritionData.protein = null;
    this.model.nutritionData.sodium = null;
    this.model.nutritionData.cholesterol = null;
    this.model.nutritionData.vitaminA = null;
    this.model.nutritionData.vitaminC = null;
    this.model.nutritionData.vitaminD = null;
    this.model.nutritionData.calcium = null;
    this.model.nutritionData.iron = null;
    this.model.nutritionData.potassium = null;
    this.model.nutritionData.magnesium = null;
  }

  setPriceData() {
    this.model.priceData.isSet = true;
  }

  removePriceData() {
    this.model.priceData.isSet = false;
    this.model.priceData.price = null;
  }

  @computedFrom(
    "model.taskId",
    "model.name",
    "model.nutritionData.servingSize",
    "model.nutritionData.servingSizeIsOneUnit",
    "model.nutritionData.calories",
    "model.nutritionData.fat",
    "model.nutritionData.saturatedFat",
    "model.nutritionData.carbohydrate",
    "model.nutritionData.sugars",
    "model.nutritionData.addedSugars",
    "model.nutritionData.fiber",
    "model.nutritionData.protein",
    "model.nutritionData.sodium",
    "model.nutritionData.cholesterol",
    "model.nutritionData.vitaminA",
    "model.nutritionData.vitaminC",
    "model.nutritionData.vitaminD",
    "model.nutritionData.calcium",
    "model.nutritionData.iron",
    "model.nutritionData.potassium",
    "model.nutritionData.magnesium",
    "model.priceData.productSize",
    "model.priceData.productSizeIsOneUnit",
    "model.priceData.price"
  )
  get canSave() {
    return JSON.stringify(this.model) !== this.originalIngredientJson;
  }

  propertyIsInvalid(
    properties: ValidateResult[],
    property: string,
    errorMessages: Array<string>
  ): boolean {
    let invalidProperty = properties.find((p) => {
      return p.propertyName === property && !p.valid;
    });

    if (invalidProperty) {
      errorMessages.push(invalidProperty.message);
      return true;
    }
    return false;
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const result: ControllerValidateResult = await this.validationController.validate();

    if (result.valid) {
      try {
        await this.ingredientsService.update(this.model);

        this.nameIsInvalid = false;

        this.router.navigateToRoute("ingredientsEdited", {
          editedId: this.ingredientId,
        });
      } catch (errorFields) {
        this.nameIsInvalid = errorFields.includes("Name");
      }
    } else {
      let errorMessages = new Array<string>();

      this.nameIsInvalid = this.propertyIsInvalid(
        result.results,
        "name",
        errorMessages
      );
      this.caloriesIsInvalid = this.propertyIsInvalid(
        result.results,
        "calories",
        errorMessages
      );
      this.fatIsInvalid = this.propertyIsInvalid(
        result.results,
        "fat",
        errorMessages
      );
      this.saturatedFatIsInvalid = this.propertyIsInvalid(
        result.results,
        "saturatedFat",
        errorMessages
      );
      this.carbohydrateIsInvalid = this.propertyIsInvalid(
        result.results,
        "carbohydrate",
        errorMessages
      );
      this.sugarsIsInvalid = this.propertyIsInvalid(
        result.results,
        "sugars",
        errorMessages
      );
      this.addedSugarsIsInvalid = this.propertyIsInvalid(
        result.results,
        "addedSugars",
        errorMessages
      );
      this.fiberIsInvalid = this.propertyIsInvalid(
        result.results,
        "fiber",
        errorMessages
      );
      this.proteinIsInvalid = this.propertyIsInvalid(
        result.results,
        "protein",
        errorMessages
      );
      this.sodiumIsInvalid = this.propertyIsInvalid(
        result.results,
        "sodium",
        errorMessages
      );
      this.cholesterolIsInvalid = this.propertyIsInvalid(
        result.results,
        "cholesterol",
        errorMessages
      );
      this.vitaminAIsInvalid = this.propertyIsInvalid(
        result.results,
        "vitaminA",
        errorMessages
      );
      this.vitaminCIsInvalid = this.propertyIsInvalid(
        result.results,
        "vitaminC",
        errorMessages
      );
      this.vitaminDIsInvalid = this.propertyIsInvalid(
        result.results,
        "vitaminD",
        errorMessages
      );
      this.calciumIsInvalid = this.propertyIsInvalid(
        result.results,
        "calcium",
        errorMessages
      );
      this.ironIsInvalid = this.propertyIsInvalid(
        result.results,
        "iron",
        errorMessages
      );
      this.potassiumIsInvalid = this.propertyIsInvalid(
        result.results,
        "potassium",
        errorMessages
      );
      this.magnesiumIsInvalid = this.propertyIsInvalid(
        result.results,
        "magnesium",
        errorMessages
      );
      this.priceIsInvalid = this.propertyIsInvalid(
        result.results,
        "price",
        errorMessages
      );

      this.eventAggregator.publish(AlertEvents.ShowError, errorMessages);
    }

    this.saveButtonIsLoading = false;
  }

  async delete() {
    if (this.deleteButtonIsLoading) {
      return;
    }

    if (this.deleteInProgress) {
      this.deleteButtonIsLoading = true;

      await this.ingredientsService.delete(this.model.id);
      this.eventAggregator.publish(
        AlertEvents.ShowSuccess,
        "editIngredient.deleteSuccessful"
      );
      this.router.navigateToRoute("ingredients");
    } else if (this.model.recipes.length > 0) {
      this.deleteButtonText = this.i18n.tr("editIngredient.yesImSure");
      this.deleteInProgress = true;
    } else {
      this.deleteButtonText = this.i18n.tr("sure");
      this.deleteInProgress = true;
    }
  }

  cancel() {
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
  }
}
