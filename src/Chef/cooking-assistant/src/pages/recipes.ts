import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { connectTo } from "aurelia-store";
import { EventAggregator } from "aurelia-event-aggregator";

import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";
import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { ProgressBar } from "../../../shared/src/models/progressBar";

import * as environment from "../../config/environment.json";
import { RecipesService } from "services/recipesService";
import { UsersService } from "services/usersService";
import { RecipeModel } from "models/viewmodels/recipeModel";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";
import { AppEvents } from "models/appEvents";

@autoinject
@connectTo()
export class Recipes {
  private imageUri = JSON.parse(<any>environment).defaultProfileImageUri;
  private progressBar = new ProgressBar();
  private recipes: RecipeModel[];
  private lastEditedId: number;
  private menuButtonIsLoading = false;
  private recipesContainer: HTMLDivElement;
  private imageWidth: number;
  private imageHeight: number;
  private resizeObserver: ResizeObserver;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly recipesService: RecipesService,
    private readonly usersService: UsersService,
    private readonly i18n: I18N,
    private readonly localStorage: LocalStorageCurrencies,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker
  ) {
    this.eventAggregator.subscribe(AppEvents.RecipesLoaded, () => {
      this.setRecipesFromState();
      this.progressBar.finish();
    });
  }

  activate(params: any) {
    if (params.editedId) {
      this.lastEditedId = parseInt(params.editedId, 10);
    }

    this.imageUri = this.localStorage.getProfileImageUri();
  }

  deactivate() {
    this.resizeObserver.disconnect();
  }

  async attached() {
    // Set images width and height to avoid reflows
    this.resizeObserver = new ResizeObserver(() => {
      this.imageWidth = this.recipesContainer.offsetWidth;
      this.imageHeight = this.recipesContainer.offsetWidth / 2;
    });
    this.resizeObserver.observe(document.body);

    if (this.state.recipes) {
      this.setRecipesFromState();
    } else {
      this.progressBar.start();
    }

    if (this.localStorage.isStale("profileImageUri")) {
      const imageUri = await this.usersService.getProfileImageUri();
      if (this.imageUri !== imageUri) {
        this.imageUri = imageUri;
      }
    }
  }

  setRecipesFromState() {
    this.recipes = this.state.recipes
      .sort((a: RecipeModel, b: RecipeModel) => {
        const aDate = new Date(a.lastOpenedDate);
        const bDate = new Date(b.lastOpenedDate);

        if (aDate > bDate) {
          return -1;
        } else if (aDate < bDate) {
          return 1;
        }
        return 0;
      })
      .map((recipe: RecipeModel) => {
        if (recipe.ingredientsMissing !== 0) {
          const missingIngredientsKey =
            recipe.ingredientsMissing > 1 ? "recipes.missingIngredients" : "recipes.missingIngredient";
          recipe.ingredientsMissingLabel = recipe.ingredientsMissing + " " + this.i18n.tr(missingIngredientsKey);
        }

        return recipe;
      });
  }

  sync() {
    this.progressBar.start();

    Actions.getRecipes(this.recipesService).then(() => {
      this.setRecipesFromState();
      this.progressBar.finish();
    });

    this.usersService.getProfileImageUri().then((imageUri) => {
      if (this.imageUri !== imageUri) {
        this.imageUri = imageUri;
      }
    });
  }

  @computedFrom("progressBar.progress")
  get getProgress() {
    if (this.progressBar.progress === 100) {
      this.progressBar.visible = false;
    }
    return this.progressBar.progress;
  }

  @computedFrom("lastEditedId")
  get getEditedId() {
    return this.lastEditedId;
  }

  goToMenu() {
    this.menuButtonIsLoading = true;
    this.router.navigateToRoute("menu");
  }

  detached() {
    window.clearInterval(this.progressBar.intervalId);
  }
}
