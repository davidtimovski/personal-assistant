import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { I18N } from "aurelia-i18n";
import { ViewRecipe } from "models/viewmodels/viewRecipe";
import { RecipesService } from "services/recipesService";
import { LocalStorageCurrencies } from "../../../shared/src/utils/localStorageCurrencies";
import { Ingredient } from "models/viewmodels/ingredient";

@inject(Router, I18N, RecipesService, LocalStorageCurrencies)
export class Recipe {
  private recipeId: number;
  private model: ViewRecipe;
  private topDrawerIsOpen = false;
  private copyButton: HTMLButtonElement;
  private copyAsTextCompleted = false;
  private editRecipeButtonIsLoading = false;
  private videoIFrame: HTMLIFrameElement;
  private videoIFrameSrc = "";
  private servingsSelectorIsVisible = false;
  private nutritionInfoVisible = false;
  private costInfoVisible = false;
  private currency: string;

  constructor(
    private readonly router: Router,
    private readonly i18n: I18N,
    private readonly recipesService: RecipesService,
    private readonly localStorage: LocalStorageCurrencies
  ) {}

  activate(params: any) {
    this.recipeId = parseInt(params.id, 10);

    this.currency = this.localStorage.getCurrency();
  }

  async attached() {
    this.recipesService
      .get(this.recipeId, this.currency)
      .then((viewRecipe: ViewRecipe) => {
        if (viewRecipe === null) {
          this.router.navigateToRoute("notFound");
        } else {
          const model = new ViewRecipe(
            viewRecipe.id,
            viewRecipe.name,
            viewRecipe.description,
            viewRecipe.ingredients,
            viewRecipe.instructions,
            viewRecipe.prepDuration,
            viewRecipe.cookDuration,
            viewRecipe.servings,
            viewRecipe.imageUri,
            viewRecipe.videoUrl,
            viewRecipe.lastOpenedDate,
            viewRecipe.nutritionSummary,
            viewRecipe.costSummary
          );

          this.servingsSelectorIsVisible = model.ingredients.some(
            (ingredient: Ingredient) => {
              return !!ingredient.amount;
            }
          );

          this.model = model;
        }
      })
      .then(() => {
        if (this.model.videoUrl) {
          this.videoIFrameSrc = this.recipesService.videoUrlToEmbedSrc(
            this.model.videoUrl
          );

          // Hack for back button iframe issue
          this.videoIFrame.contentWindow.location.replace(this.videoIFrameSrc);
        }
      });

    this.copyButton.addEventListener("click", () => {
      this.copyAsText();
    });
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
    let text = this.model.name + "\n----------";

    if (this.model.description) {
      text += "\n\n" + this.model.description;
    }

    if (this.model.ingredients.length > 0) {
      text += `\n\n${this.i18n.tr("editRecipe.ingredients")}:`;

      for (let ingredient of this.model.ingredients) {
        text += `\n◾ ${ingredient.name}`;
        if (ingredient.amount) {
          text += ` - ${
            ingredient.amount + (ingredient.unit ? " " + ingredient.unit : "")
          }`;
        }
      }
    }

    if (this.model.instructions) {
      text += `\n\n${this.i18n.tr("editRecipe.instructions")}:`;
      text += "\n----------\n" + this.model.instructions + "\n----------";
    }

    if (this.model.videoUrl) {
      text += `\n\n${this.i18n.tr("editRecipe.youTubeUrl")}: ${
        this.model.videoUrl
      }`;
    }

    if (this.model.prepDuration || this.model.cookDuration) {
      text += "\n";

      if (this.model.prepDuration) {
        const prepDurationHours = this.model.prepDuration.substring(0, 2);
        const prepDurationMinutes = this.model.prepDuration.substring(3, 5);

        text += `\n${this.i18n.tr("editRecipe.prepDuration")}: `;

        if (parseInt(prepDurationHours, 10) === 0) {
          text +=
            parseInt(prepDurationMinutes, 10) + this.i18n.tr("minutesLetter");
        } else {
          text +=
            parseInt(prepDurationHours, 10) +
            this.i18n.tr("hoursLetter") +
            " " +
            parseInt(prepDurationMinutes, 10) +
            this.i18n.tr("minutesLetter");
        }
      }
      if (this.model.cookDuration) {
        const cookDurationHours = this.model.cookDuration.substring(0, 2);
        const cookDurationMinutes = this.model.cookDuration.substring(3, 5);

        text += `\n${this.i18n.tr("editRecipe.cookDuration")}: `;

        if (parseInt(cookDurationHours, 10) === 0) {
          text +=
            parseInt(cookDurationMinutes, 10) + this.i18n.tr("minutesLetter");
        } else {
          text +=
            parseInt(cookDurationHours, 10) +
            this.i18n.tr("hoursLetter") +
            " " +
            parseInt(cookDurationMinutes, 10) +
            this.i18n.tr("minutesLetter");
        }
      }
    }

    if (this.model.servings > 1) {
      text +=
        "\n\n" +
        this.i18n.tr("recipe.servings", {
          servings: this.model.servings,
        });
    }

    const textArea = document.createElement("textarea");
    textArea.value = text;
    textArea.style.position = "fixed"; // avoid scrolling to bottom
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    document.execCommand("copy");

    document.body.removeChild(textArea);

    this.copyAsTextCompleted = true;
    window.setTimeout(() => {
      this.copyAsTextCompleted = false;
    }, 2000);
  }

  get instructionsInHtml(): string {
    return this.model.instructions.replace(/(?:\r\n|\r|\n)/g, "<br>");
  }

  toggleNutritionInfo() {
    if (!this.nutritionInfoVisible) {
      if (this.costInfoVisible) {
        this.model.ingredients.forEach((ingredient) => {
          ingredient.costSource = false;
        });
        this.costInfoVisible = false;
      }

      this.model.ingredients.forEach((ingredient) => {
        ingredient.nutritionSource = this.model.nutritionSummary.ingredientIds.includes(
          ingredient.id
        );
      });

      this.scrollToIngredientsSection();
    } else {
      this.model.ingredients.forEach((ingredient) => {
        ingredient.nutritionSource = false;
      });
    }
    this.nutritionInfoVisible = !this.nutritionInfoVisible;
  }

  toggleCostInfo() {
    if (!this.costInfoVisible) {
      if (this.nutritionInfoVisible) {
        this.model.ingredients.forEach((ingredient) => {
          ingredient.nutritionSource = false;
        });
        this.nutritionInfoVisible = false;
      }

      this.model.ingredients.forEach((ingredient) => {
        ingredient.costSource = this.model.costSummary.ingredientIds.includes(
          ingredient.id
        );
      });

      this.scrollToIngredientsSection();
    } else {
      this.model.ingredients.forEach((ingredient) => {
        ingredient.costSource = false;
      });
    }
    this.costInfoVisible = !this.costInfoVisible;
  }

  editRecipe() {
    this.editRecipeButtonIsLoading = true;
    this.router.navigateToRoute("editRecipe", { id: this.model.id });
  }

  private scrollToIngredientsSection() {
    const ingredientsSection = document.getElementById("ingredients-section");
    window.scroll({
      top: ingredientsSection.getBoundingClientRect().top + window.scrollY,
      behavior: "smooth",
    });
  }
}
