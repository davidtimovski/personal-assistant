import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import autocomplete, { AutocompleteResult } from "autocompleter";

import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { IngredientsService } from "services/ingredientsService";
import { ViewIngredientModel } from "models/viewmodels/viewIngredientModel";
import { TaskSuggestion } from "models/viewmodels/taskSuggestion";

@inject(
  Router,
  IngredientsService,
  I18N,
  EventAggregator,
  LocalStorageCurrencies
)
export class ViewIngredient {
  private ingredientId: number;
  private model: ViewIngredientModel;
  private originalIngredientJson: string;
  private ingredientLinkedMessage: string;
  private tasksSearchVisible = false;
  private autocomplete: AutocompleteResult;
  private taskSuggestions: Array<TaskSuggestion>;
  private pickTaskInput: HTMLInputElement;
  private currency: string;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly ingredientsService: IngredientsService,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator,
    private readonly localStorage: LocalStorageCurrencies
  ) {
    this.deleteButtonText = this.i18n.tr("delete");
  }

  activate(params: any) {
    this.ingredientId = parseInt(params.id, 10);

    this.currency = this.localStorage.getCurrency();
  }

  async attached() {
    const ingredient = await this.ingredientsService.getPublic(this.ingredientId);
    if (ingredient === null) {
      this.router.navigateToRoute("notFound");
    }

    this.model = ingredient;

    if (this.model.taskId) {
      this.ingredientLinkedMessage = this.i18n.tr("editIngredient.thisIngredientIsLinked", {
        taskName: this.model.taskName,
        taskList: this.model.taskList
      });
    }

    if (!this.model.priceData.isSet) {
      this.model.priceData.currency = this.currency;
    }
  }

  async linkToTask() {
    this.taskSuggestions = await this.ingredientsService.getTaskSuggestions();
    this.tasksSearchVisible = true;

    this.attachAutocomplete(this.taskSuggestions);
  }

  attachAutocomplete(taskSuggestions: TaskSuggestion[]) {
    if (this.autocomplete) {
      this.autocomplete.destroy();
    }

    this.autocomplete = autocomplete({
      input: this.pickTaskInput,
      minLength: 2,
      fetch: (
        text: string,
        update: (items: TaskSuggestion[]) => void
      ) => {
        const suggestions = taskSuggestions.filter((i) =>
          i.label.toUpperCase().startsWith(text.toUpperCase())
        );
        update(suggestions);
      },
      onSelect: (suggestion: TaskSuggestion) => {
        this.ingredientLinkedMessage = this.i18n.tr("editIngredient.thisIngredientIsLinked", {
          taskName: suggestion.label,
          taskList: suggestion.group
        });

        this.model.taskId = suggestion.id;

        this.pickTaskInput.value = "";
      },
      className: "autocomplete-customizations",
    });
  }

  unlinkFromTask() {
    this.model.taskId = null;
    this.tasksSearchVisible = false;
  }

  @computedFrom("model.taskId")
  get canSave() {
    return JSON.stringify(this.model) !== this.originalIngredientJson;
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    await this.ingredientsService.updatePublic(this.model.id, this.model.taskId);

    this.eventAggregator.publish(AlertEvents.ShowSuccess, "viewIngredient.updateSuccessful");
    this.router.navigateToRoute("ingredientsEdited", {
      editedId: this.ingredientId,
    });

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
