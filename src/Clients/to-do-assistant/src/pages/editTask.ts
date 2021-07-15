import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { TasksService } from "services/tasksService";
import { ListOption } from "models/viewmodels/listOption";
import { ListsService } from "services/listsService";
import { EditTaskModel } from "models/viewmodels/editTaskModel";
import { AssigneeOption } from "models/viewmodels/assigneeOption";
import * as environment from "../../config/environment.json";
import * as Actions from "utils/state/actions";

@inject(Router, TasksService, ListsService, ValidationController, I18N, EventAggregator)
export class EditTask {
  private taskId: number;
  private model: EditTaskModel;
  private originalTaskJson: string;
  private listOptions: Array<ListOption>;
  private assigneeOptions: Array<AssigneeOption>;
  private nobodyImageUri = JSON.parse(<any>environment).defaultProfileImageUri;
  private nameIsInvalid: boolean;
  private recipesText: string;
  private deleteInProgress = false;
  private deleteButtonText: string;
  private saveButtonIsLoading = false;
  private deleteButtonIsLoading = false;
  private privateTasksTooltipKey = "privateTasks";

  constructor(
    private readonly router: Router,
    private readonly tasksService: TasksService,
    private readonly listsService: ListsService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;
    this.deleteButtonText = this.i18n.tr("delete");

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.nameIsInvalid = false;
    });
  }

  activate(params: any) {
    this.taskId = parseInt(params.id, 10);
  }

  attached() {
    this.tasksService.getForUpdate(this.taskId).then((task: EditTaskModel) => {
      if (task === null) {
        this.router.navigateToRoute("notFound");
      }

      this.model = task;
      this.originalTaskJson = JSON.stringify(this.model);

      if (this.model.isInSharedList) {
        this.listsService.getMembersAsAssigneeOptions(this.model.listId).then((assigneeOptions) => {
          this.assigneeOptions = assigneeOptions;
        });
      }

      this.recipesText = this.model.recipes.join(", ");

      ValidationRules.ensure((x: EditTaskModel) => x.name)
        .required()
        .on(this.model);
    });

    this.listsService.getAllAsOptions().then((listOptions) => {
      this.listOptions = listOptions;
    });
  }

  isPrivateChanged() {
    if (this.model.isPrivate) {
      this.model.assignedToUserId = null;
    }
  }

  @computedFrom("model.name", "model.listId", "model.isOneTime", "model.isPrivate", "model.assignedToUserId")
  get canSave() {
    return !ValidationUtil.isEmptyOrWhitespace(this.model.name) && JSON.stringify(this.model) !== this.originalTaskJson;
  }

  @computedFrom("model.assignedToUserId")
  get assignToUserLabel() {
    return this.model.assignedToUserId ? this.i18n.tr("editTask.assignedTo") : this.i18n.tr("editTask.assignToUser");
  }

  @computedFrom("model.listId", "model.isPrivate")
  get assignToUserIsVisible() {
    return this.model.isInSharedList && !this.model.isPrivate;
  }

  listChanged() {
    const selectedList = this.listOptions.find((x) => x.id === this.model.listId);
    this.model.isInSharedList = selectedList.isShared;

    if (this.model.isInSharedList) {
      this.listsService.getMembersAsAssigneeOptions(this.model.listId).then((assigneeOptions) => {
        this.assigneeOptions = assigneeOptions;
      });
    } else {
      this.assigneeOptions = [];
    }
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
      try {
        await this.tasksService.update(this.model);
        this.nameIsInvalid = false;

        await Actions.getLists(this.listsService);

        this.router.navigateToRoute("listEdited", {
          id: this.model.listId,
          editedId: this.model.id,
        });
      } catch (errorFields) {
        this.nameIsInvalid = errorFields.includes("Name");
        this.saveButtonIsLoading = false;
      }
    } else {
      this.saveButtonIsLoading = false;
    }
  }

  async delete() {
    if (this.deleteButtonIsLoading) {
      return;
    }

    if (this.deleteInProgress) {
      this.deleteButtonIsLoading = true;

      await this.tasksService.delete(this.model.id);

      await Actions.getLists(this.listsService);

      this.eventAggregator.publish(AlertEvents.ShowSuccess, "editTask.deleteSuccessful");
      this.router.navigateToRoute("list", { id: this.model.listId });
    } else {
      if (this.model.recipes.length > 0) {
        this.deleteButtonText = this.i18n.tr("editTask.okayDelete");
      } else {
        this.deleteButtonText = this.i18n.tr("sure");
      }

      this.deleteInProgress = true;
    }
  }

  cancel() {
    if (!this.deleteInProgress) {
      this.router.navigateToRoute("list", { id: this.model.listId });
    }
    this.deleteButtonText = this.i18n.tr("delete");
    this.deleteInProgress = false;
  }
}
