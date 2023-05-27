import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";

import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";

import { ViewRecipe } from "models/viewmodels/viewRecipe";
import { RecipesService } from "services/recipesService";
import { Ingredient } from "models/viewmodels/ingredient";
import { SharingState } from "models/viewmodels/sharingState";

@autoinject
export class Recipe {
  private recipeId: number;
  private model: ViewRecipe;
  private topDrawerIsOpen = false;
  private shareButtonText: string;
  private copyButton: HTMLButtonElement;
  private copyAsTextCompleted = false;
  private editRecipeButtonIsLoading = false;
  private recipeContainer: HTMLDivElement;
  private resizeObserver: ResizeObserver;
  private videoIFrame: HTMLIFrameElement;
  private videoIFrameSrc = "";
  private servingsSelectorIsVisible = false;
  private currency: string;
  private wakeLockSupported: boolean;
  private wakeLock: any;

  constructor(
    private readonly router: Router,
    private readonly i18n: I18N,
    private readonly recipesService: RecipesService,
    private readonly localStorage: LocalStorageCurrencies
  ) {
    this.wakeLockSupported = "wakeLock" in navigator;
  }

  activate(params: any) {
    this.recipeId = parseInt(params.id, 10);

    this.currency = this.localStorage.getCurrency();
  }

  deactivate() {
    if (this.wakeLockSupported && this.wakeLock) {
      this.wakeLock.release();
      this.wakeLock = null;
    }
    this.resizeObserver.disconnect();
  }

  async attached() {
    const viewRecipe = await this.recipesService.get(this.recipeId, this.currency);
    if (viewRecipe === null) {
      this.router.navigateToRoute("notFound");
    } else {
      this.servingsSelectorIsVisible = viewRecipe.ingredients.some((ingredient: Ingredient) => {
        return !!ingredient.amount;
      });

      this.model = viewRecipe;

      // Set image width and height to avoid reflows
      this.resizeObserver = new ResizeObserver(() => {
        this.model.imageWidth = this.recipeContainer.offsetWidth - 2;
        this.model.imageHeight = (this.recipeContainer.offsetWidth - 2) / 2;
      });
      this.resizeObserver.observe(document.body);

      if (this.model.videoUrl) {
        this.videoIFrameSrc = this.recipesService.videoUrlToEmbedSrc(this.model.videoUrl);
      }

      this.shareButtonText =
        this.model.sharingState === SharingState.NotShared
          ? this.i18n.tr("recipe.shareRecipe")
          : this.i18n.tr("recipe.members");

      this.copyButton.addEventListener("click", () => {
        this.copyAsText();
      });

      if (this.wakeLockSupported) {
        (<any>navigator).wakeLock.request("screen").then((wakeLock) => {
          this.wakeLock = wakeLock;
        });
      }
    }
  }

  toggleTopDrawer() {
    this.topDrawerIsOpen = !this.topDrawerIsOpen;
  }

  closeDrawer() {
    if (this.topDrawerIsOpen) {
      this.topDrawerIsOpen = false;
    }
  }

  copyAsText() {
    this.recipesService.copyAsText(
      this.model,
      this.i18n.tr("editRecipe.ingredients"),
      this.i18n.tr("editRecipe.instructions"),
      this.i18n.tr("editRecipe.youTubeUrl"),
      this.i18n.tr("editRecipe.prepDuration"),
      this.i18n.tr("minutesLetter"),
      this.i18n.tr("hoursLetter"),
      this.i18n.tr("editRecipe.cookDuration"),
      this.i18n.tr("recipe.servings", {
        servings: this.model.servings,
      })
    );

    this.copyAsTextCompleted = true;
    window.setTimeout(() => {
      this.copyAsTextCompleted = false;
    }, 2000);
  }

  get instructionsInHtml(): string {
    return this.model.instructions.replace(/(?:\r\n|\r|\n)/g, "<br>");
  }

  editRecipe() {
    this.editRecipeButtonIsLoading = true;
    this.router.navigateToRoute("editRecipe", { id: this.model.id });
  }
}
