import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { ListsService } from "services/listsService";
import { ListWithShares } from "models/viewmodels/listWithShares";
import { Share } from "models/viewmodels/share";
import { CanShareList } from "models/viewmodels/canShareList";
import { SharingState } from "models/viewmodels/sharingState";

@inject(Router, AuthService, ListsService, ValidationController, I18N, EventAggregator)
export class ShareList {
  private model: ListWithShares;
  private originalShares: Share[];
  private selectedShare: Share;
  private originalSelectedShareJson: string;
  private emailIsInvalid: boolean;
  private emailInput: HTMLInputElement;
  private currentUserEmail: string;
  private currentlyEditing = false;
  private canEditEmail = true;
  private canSave = false;
  private newShares = new Array<Share>();
  private editedShares = new Array<Share>();
  private removedShares = new Array<Share>();
  private saveButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly listsService: ListsService,
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

    this.model = await this.listsService.getWithShares(params.id);
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

  @computedFrom("selectedShare.isAdmin")
  get emailPlaceholderText() {
    return this.selectedShare.isAdmin
      ? this.i18n.tr("shareList.newAdminEmail")
      : this.i18n.tr("shareList.newMemberEmail");
  }

  resetSelectedShare() {
    this.selectedShare = new Share(null, "", "", false, null);
    ValidationRules.ensure((x: Share) => x.email)
      .required()
      .email()
      .satisfies((email) => email.trim().toLowerCase() !== this.currentUserEmail)
      .on(this.selectedShare);
  }

  @computedFrom("model.shares.length")
  get sharedWithWrapTitle() {
    return JSON.stringify(this.originalShares) !== JSON.stringify(this.model.shares)
      ? this.i18n.tr("shareList.shareWith")
      : this.i18n.tr("shareList.members");
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

    const result: ControllerValidateResult = await this.validationController.validate();

    this.emailIsInvalid = !result.valid;

    if (result.valid) {
      const canShareVM: CanShareList = await this.listsService.canShareListWithUser(email);

      if (canShareVM.userId === 0) {
        this.emailIsInvalid = true;
        this.eventAggregator.publish(AlertEvents.ShowError, "shareList.userDoesntExist");
      } else {
        if (!canShareVM.canShare) {
          this.emailIsInvalid = true;
          this.eventAggregator.publish(AlertEvents.ShowError, "shareList.cannotShareWithUser");
        } else {
          this.selectedShare.userId = canShareVM.userId;
          this.selectedShare.imageUri = canShareVM.imageUri;
          this.model.shares.push(this.selectedShare);

          if (this.shareExistedPreviously(this.selectedShare)) {
            this.removedShares.splice(this.removedShares.indexOf(this.selectedShare), 1);
            this.editedShares.push(this.selectedShare);
          } else {
            this.newShares.push(this.selectedShare);
          }
          this.evaluateCanSave();
          this.resetSelectedShare();
        }
      }
    }
  }

  async saveShare() {
    const email = this.selectedShare.email.trim();

    if (this.canEditEmail) {
      const selectedShare = this.model.shares.find((share) => {
        return share.userId === this.selectedShare.userId;
      });
      this.selectedShare.userId = null;

      const canShareVM: CanShareList = await this.listsService.canShareListWithUser(email);

      this.selectedShare.userId = selectedShare.userId = canShareVM.userId;

      if (this.originalSelectedShareJson !== JSON.stringify(this.selectedShare)) {
        this.newShares.push(this.selectedShare);
      }
      this.evaluateCanSave();
    } else {
      if (
        this.originalSelectedShareJson !== JSON.stringify(this.selectedShare) &&
        !this.newShares.includes(this.selectedShare) &&
        !this.editedShares.includes(this.selectedShare)
      ) {
        this.editedShares.push(this.selectedShare);
      }
      this.evaluateCanSave();
    }

    this.currentlyEditing = false;
    this.canEditEmail = true;
    this.resetSelectedShare();
  }

  submit() {
    if (!this.currentlyEditing) {
      this.addShare();
    }
  }

  removeShare(share: Share) {
    this.model.shares.splice(this.model.shares.indexOf(share), 1);
    this.newShares.splice(this.model.shares.indexOf(share), 1);
    this.editedShares.splice(this.model.shares.indexOf(share), 1);

    if (this.shareExistedPreviously(share)) {
      this.removedShares.push(share);
    }

    if (share.email === this.selectedShare.email) {
      this.selectedShare = new Share(null, "", "", false, null);
      this.currentlyEditing = false;
      this.canEditEmail = true;
    }

    this.evaluateCanSave();
  }

  select(share: Share) {
    this.emailIsInvalid = false;

    if (this.selectedShare === share) {
      this.resetSelectedShare();
      this.currentlyEditing = false;
      this.canEditEmail = true;
    } else {
      this.canEditEmail = share.userId === null || share.userId === 0;
      this.originalSelectedShareJson = JSON.stringify(share);
      this.selectedShare = share;
      this.currentlyEditing = true;
    }
  }

  evaluateCanSave(): void {
    this.canSave = this.newShares.length + this.editedShares.length + this.removedShares.length > 0;
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.emailIsInvalid = false;

    await this.listsService.share(this.model.id, this.newShares, this.editedShares, this.removedShares);

    if (this.editedShares.length + this.removedShares.length > 0) {
      this.eventAggregator.publish(AlertEvents.ShowSuccess, "shareList.sharingDetailsSaved");
    } else if (this.newShares.length === 1) {
      this.eventAggregator.publish(AlertEvents.ShowSuccess, "shareList.shareRequestSent");
    } else if (this.newShares.length > 1) {
      this.eventAggregator.publish(AlertEvents.ShowSuccess, "shareList.shareRequestsSent");
    }

    this.router.navigateToRoute("listsEdited", { editedId: this.model.id });
  }

  shareExistedPreviously(share: Share): boolean {
    const originalShareUserIds = this.originalShares.map((share) => {
      return share.userId;
    });
    return originalShareUserIds.includes(share.userId);
  }
}
