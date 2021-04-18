import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import {
  ValidationController,
  validateTrigger,
  ValidationRules,
  ControllerValidateResult,
} from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { RecipesService } from "services/recipesService";
import { RecipeWithShares } from "models/viewmodels/recipeWithShares";
import { Share } from "models/viewmodels/share";
import { CanShareRecipe } from "models/viewmodels/canShareRecipe";
import { SharingState } from "models/viewmodels/sharingState";
import * as Actions from "utils/state/actions";

@inject(
  Router,
  AuthService,
  RecipesService,
  ValidationController,
  I18N,
  EventAggregator
)
export class ShareRecipe {
  private model: RecipeWithShares;
  private originalShares: Array<Share>;
  private selectedShare: Share;
  private emailIsInvalid: boolean;
  private emailInput: HTMLInputElement;
  private currentUserEmail: string;
  private canSave = false;
  private newShares = new Array<number>();
  private removedShares = new Array<number>();
  private saveButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly recipesService: RecipesService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.resetSelectedShare();

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.emailIsInvalid = false;
    });
  }

  async activate(params: any) {
    this.currentUserEmail = this.authService.currentUser.profile.name;
    
    this.model = await this.recipesService.getWithShares(params.id);
    if (this.model === null) {
      this.router.navigateToRoute("notFound");
    }
    this.originalShares = this.model.shares.slice();
  }

  attached() {
    if (this.model.sharingState === SharingState.NotShared) {
      this.emailInput.focus();
    }
  }

  resetSelectedShare() {
    this.selectedShare = new Share(null, "", "", null);
    ValidationRules.ensure((x: Share) => x.email)
      .required()
      .email()
      .satisfies((email) => email.trim().toLowerCase() !== this.currentUserEmail)
      .on(this.selectedShare);
  }

  @computedFrom("model.shares.length")
  get sharedWithWrapTitle() {
    return JSON.stringify(this.originalShares) !==
      JSON.stringify(this.model.shares)
      ? this.i18n.tr("shareRecipe.shareWith")
      : this.i18n.tr("shareRecipe.members");
  }

  async addShare() {
    this.eventAggregator.publish(AlertEvents.HideError);

    const email = this.selectedShare.email.trim();

    const duplicateEmails = this.model.shares.filter((share) => {
      if (share.email === email) {
        return share;
      }
    });
    if (duplicateEmails.length > 0 || this.model.ownerEmail === email) {
      this.emailIsInvalid = true;
      return;
    }

    console.log(this.currentUserEmail);
    const result: ControllerValidateResult = await this.validationController.validate();

    this.emailIsInvalid = !result.valid;

    if (result.valid) {
      const canShareVM: CanShareRecipe = await this.recipesService.canShareRecipeWithUser(
        email
      );

      if (canShareVM.userId === 0) {
        this.emailIsInvalid = true;
        this.eventAggregator.publish(
          AlertEvents.ShowError,
          "shareRecipe.userDoesntExist"
        );
      } else {
        if (!canShareVM.canShare) {
          this.emailIsInvalid = true;
          this.eventAggregator.publish(
            AlertEvents.ShowError,
            "shareRecipe.cannotShareWithUser"
          );
        } else {
          this.selectedShare.userId = canShareVM.userId;
          this.selectedShare.imageUri = canShareVM.imageUri;
          this.model.shares.push(this.selectedShare);

          if (this.shareExistedPreviously(this.selectedShare)) {
            this.removedShares.splice(
              this.removedShares.indexOf(this.selectedShare.userId),
              1
            );
          } else {
            this.newShares.push(this.selectedShare.userId);
          }
          this.evaluateCanSave();
          this.resetSelectedShare();
        }
      }
    }
  }

  submit() {
    this.addShare();
  }

  removeShare(share: Share) {
    this.model.shares.splice(this.model.shares.indexOf(share), 1);
    this.newShares.splice(this.model.shares.indexOf(share), 1);

    if (this.shareExistedPreviously(share)) {
      this.removedShares.push(share.userId);
    }

    if (share.email === this.selectedShare.email) {
      this.selectedShare = new Share(null, "", "", null);
    }

    this.evaluateCanSave();
  }

  evaluateCanSave(): void {
    this.canSave =
      this.newShares.length +
      this.removedShares.length >
      0;
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.emailIsInvalid = false;

    await this.recipesService.share(
      this.model.id,
      this.newShares,
      this.removedShares
    );

    await Actions.getRecipes(this.recipesService);

    if (this.removedShares.length > 0) {
      this.eventAggregator.publish(
        AlertEvents.ShowSuccess,
        "shareRecipe.sharingDetailsSaved"
      );
    } else if (this.newShares.length === 1) {
      this.eventAggregator.publish(
        AlertEvents.ShowSuccess,
        "shareRecipe.shareRequestSent"
      );
    } else if (this.newShares.length > 1) {
      this.eventAggregator.publish(
        AlertEvents.ShowSuccess,
        "shareRecipe.shareRequestsSent"
      );
    }

    this.router.navigateToRoute("recipesEdited", { editedId: this.model.id });
  }

  shareExistedPreviously(share: Share): boolean {
    const originalShareUserIds = this.originalShares.map((share) => {
      return share.userId;
    });
    return originalShareUserIds.includes(share.userId);
  }
}
