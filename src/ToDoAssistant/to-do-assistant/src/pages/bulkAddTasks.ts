import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";
import { ValidationErrors } from "../../../shared/src/models/validationErrors";

import { BulkAddTasksModel } from "models/viewmodels/bulkAddTasksModel";
import { TasksService } from "services/tasksService";
import { ListsService } from "services/listsService";

@autoinject
@connectTo()
export class BulkAddTasks {
  private model: BulkAddTasksModel;
  private listIsShared: boolean;
  private tasksTextIsInvalid: boolean;
  private confirmationInProgress = false;
  private tasksTextInput: HTMLInputElement;
  private saveButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly tasksService: TasksService,
    private readonly listsService: ListsService,
    private readonly validationController: ValidationController,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.tasksTextIsInvalid = false;
    });
  }

  async activate(params: any) {
    this.listIsShared = await this.listsService.getIsShared(parseInt(params.id, 10));

    this.model = new BulkAddTasksModel(parseInt(params.id, 10), "", false, false);

    ValidationRules.ensure((x: BulkAddTasksModel) => x.tasksText)
      .required()
      .on(this.model);
  }

  attached() {
    this.tasksTextInput.focus();
  }

  @computedFrom("model.tasksText")
  get canSave(): boolean {
    return !ValidationUtil.isEmptyOrWhitespace(this.model.tasksText);
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish(AlertEvents.HideError);

    const result: ControllerValidateResult = await this.validationController.validate();

    if (!result.valid) {
      this.tasksTextIsInvalid = !this.model.tasksText || !this.model.tasksText.trim();
      this.saveButtonIsLoading = false;
    } else {
      try {
        await this.tasksService.bulkCreate(this.model, this.listsService);
        this.tasksTextIsInvalid = false;

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "bulkAddTasks.addSuccessful");

        this.router.navigateToRoute("list", { id: this.model.listId });
      } catch (e) {
        if (e instanceof ValidationErrors) {
          this.tasksTextIsInvalid = e.fields.includes("TasksText");
        }

        this.saveButtonIsLoading = false;
      }
    }
  }

  cancel() {
    if (!this.confirmationInProgress) {
      this.router.navigateToRoute("list", { id: this.model.listId });
    }
    this.confirmationInProgress = false;
  }
}
