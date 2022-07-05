import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";

import { ListsService } from "services/listsService";
import { UsersService } from "services/usersService";
import { EditListModel } from "models/viewmodels/editListModel";
import { SharingState } from "models/viewmodels/sharingState";
import { PreferencesModel } from "models/preferencesModel";

@inject(Router, ListsService, UsersService, ValidationController, I18N, EventAggregator)
export class EditList {
  private model: EditListModel;
  private originalListJson: string;
  private isNewList: boolean;
  private nameIsInvalid: boolean;
  private tasksTextIsInvalid: boolean;
  private tasksInputIsVisible = false;
  private confirmationInProgress = false;
  private saveButtonText: string;
  private deleteButtonText: string;
  private leaveButtonText: string;
  private iconOptions = ListsService.getIconOptions();
  private nameInput: HTMLInputElement;
  private preferences: PreferencesModel;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;
  private leaveButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly usersService: UsersService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");
    this.leaveButtonText = this.i18n.tr("editList.leave");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.nameIsInvalid = false;
    });
  }

  async activate(params: any) {
    this.isNewList = parseInt(params.id, 10) === 0;

    if (this.isNewList) {
      this.model = new EditListModel(0, "", this.iconOptions[0].icon, "", false, false, false, SharingState.NotShared);
      this.saveButtonText = this.i18n.tr("editList.create");
    } else {
      this.saveButtonText = this.i18n.tr("save");

      this.model = await this.listsService.get(params.id);
      if (this.model === null) {
        this.router.navigateToRoute("notFound");
      }
      this.originalListJson = JSON.stringify(this.model);
    }

    this.preferences = await this.usersService.getPreferences();

    ValidationRules.ensure((s: EditListModel) => s.name)
      .required()
      .on(this.model);
  }

  attached() {
    if (this.isNewList) {
      this.nameInput.focus();
    }
  }

  @computedFrom("model.name", "model.icon", "model.isOneTimeToggleDefault", "model.notificationsEnabled")
  get canSave(): boolean {
    if (this.isNewList) {
      return !ValidationUtil.isEmptyOrWhitespace(this.model.name);
    }

    return !ValidationUtil.isEmptyOrWhitespace(this.model.name) && JSON.stringify(this.model) !== this.originalListJson;
  }

  @computedFrom("model.sharingState", "profile.notificationsEnabled")
  get notificationsCheckboxEnabled() {
    return this.model.sharingState !== SharingState.NotShared && this.preferences.notificationsEnabled;
  }

  showTasksTextarea() {
    this.tasksInputIsVisible = true;
  }

  selectIcon(icon: string) {
    this.model.icon = icon;
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
      if (!this.isNewList) {
        try {
          if (this.model.sharingState === SharingState.Member) {
            await this.listsService.updateShared(this.model);
          } else {
            await this.listsService.update(this.model);
          }
          this.nameIsInvalid = false;

          const redirectRoute = this.model.isArchived ? "archivedListsEdited" : "listsEdited";
          this.router.navigateToRoute(redirectRoute, {
            editedId: this.model.id,
          });
        } catch (errorFields) {
          this.nameIsInvalid = errorFields.includes("Name");
          this.saveButtonIsLoading = false;
        }
      } else {
        try {
          const id = await this.listsService.create(
            this.model.name,
            this.model.icon,
            this.model.isOneTimeToggleDefault,
            this.model.tasksText
          );
          this.nameIsInvalid = false;

          this.router.navigateToRoute("listsEdited", {
            editedId: id,
          });
        } catch (errorFields) {
          this.nameIsInvalid = errorFields.includes("Name");
          this.tasksTextIsInvalid = errorFields.includes("TasksText");
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

      await this.listsService.delete(this.model.id);

      this.eventAggregator.publish(AlertEvents.ShowSuccess, "editList.deleteSuccessful");
      this.router.navigateToRoute("lists");
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

      await this.listsService.leave(this.model.id);

      this.eventAggregator.publish(AlertEvents.ShowSuccess, "editList.youHaveLeftTheList");

      this.router.navigateToRoute("lists");
    } else {
      this.leaveButtonText = this.i18n.tr("sure");
      this.confirmationInProgress = true;
    }
  }

  cancel() {
    if (!this.confirmationInProgress) {
      if (this.isNewList) {
        this.router.navigateToRoute("lists");
      } else {
        this.router.navigateToRoute("list", { id: this.model.id });
      }
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.leaveButtonText = this.i18n.tr("editList.leave");
    this.confirmationInProgress = false;
  }

  back() {
    if (this.isNewList) {
      this.router.navigateToRoute("lists");
    } else {
      this.router.navigateToRoute("list", { id: this.model.id });
    }
  }
}
