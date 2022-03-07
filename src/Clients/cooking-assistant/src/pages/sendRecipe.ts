import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { RecipesService } from "services/recipesService";
import { SendRecipeModel } from "models/viewmodels/sendRecipeModel";
import { Recipient } from "models/viewmodels/recipient";
import { CanSendRecipe } from "models/viewmodels/canSendRecipe";

@inject(Router, AuthService, RecipesService, ValidationController, EventAggregator)
export class SendRecipe {
  private model: SendRecipeModel;
  private recipientEmail = "";
  private emailIsInvalid: boolean;
  private emailInput: HTMLInputElement;
  private currentUserEmail: string;
  private recipients = new Array<Recipient>();
  private sendButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly recipesService: RecipesService,
    private readonly validationController: ValidationController,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    ValidationRules.ensure((x: SendRecipe) => x.recipientEmail)
      .required()
      .email()
      .on(this);

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.emailIsInvalid = false;
    });
  }

  async activate(params: any) {
    this.currentUserEmail = this.authService.currentUser.profile.name;

    this.model = await this.recipesService.getForSending(params.id);
    if (this.model === null) {
      this.router.navigateToRoute("notFound");
    }
  }

  attached() {
    this.emailInput.focus();
  }

  async addRecipient() {
    this.eventAggregator.publish(AlertEvents.HideError);

    const email = this.recipientEmail.trim().toLowerCase();
    if (email == this.currentUserEmail) {
      this.emailIsInvalid = true;
      return;
    }

    const duplicateEmails = this.recipients.filter((recipient) => {
      if (recipient.email === email) {
        return recipient;
      }
    });
    if (duplicateEmails.length > 0) {
      this.emailIsInvalid = true;
      return;
    }

    const result: ControllerValidateResult = await this.validationController.validate();

    this.emailIsInvalid = !result.valid;

    if (result.valid) {
      const canSendVM: CanSendRecipe = await this.recipesService.canSendRecipeToUser(email, this.model.id);

      if (canSendVM.userId === 0) {
        this.emailIsInvalid = true;
        this.eventAggregator.publish(AlertEvents.ShowError, "sendRecipe.userDoesntExist");
      } else {
        if (!canSendVM.canSend) {
          this.emailIsInvalid = true;
          this.eventAggregator.publish(AlertEvents.ShowError, "sendRecipe.cannotSendToUser");
        } else if (canSendVM.alreadySent) {
          this.emailIsInvalid = true;
          this.eventAggregator.publish(AlertEvents.ShowError, "sendRecipe.alreadySendToUser");
        } else {
          this.recipients.push(new Recipient(canSendVM.userId, email, canSendVM.imageUri));
          this.recipientEmail = "";
        }
      }
    }
  }

  removeRecipient(recipient: Recipient) {
    this.recipients.splice(this.recipients.indexOf(recipient), 1);
  }

  @computedFrom("recipients.length")
  get canSend(): boolean {
    return this.recipients.length > 0;
  }

  async send() {
    if (!this.canSend || this.sendButtonIsLoading) {
      return;
    }

    this.sendButtonIsLoading = true;
    this.emailIsInvalid = false;

    const recipientsIds = this.recipients.map((x) => x.userId);
    await this.recipesService.send(this.model.id, recipientsIds);

    this.eventAggregator.publish(AlertEvents.ShowSuccess, "sendRecipe.sendSuccessful");

    this.router.navigateToRoute("recipesEdited", {
      editedId: this.model.id,
    });
  }
}
