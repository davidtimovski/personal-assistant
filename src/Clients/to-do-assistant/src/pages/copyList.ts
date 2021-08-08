import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/utils/alertEvents";

import { List } from "models/entities/list";
import { ListsService } from "services/listsService";
import { SharingState } from "models/viewmodels/sharingState";
import { Task } from "models/entities/task";
import { State } from "utils/state/state";
import * as Actions from "utils/state/actions";

@inject(Router, ListsService, ValidationController, I18N, EventAggregator)
@connectTo()
export class CopyList {
  private listId: number;
  private model = new List(0, "", "", false, false, SharingState.NotShared, 0, false, null, [], null);
  private nameIsInvalid: boolean;
  private iconOptions = ListsService.getIconOptions();
  private saveButtonIsLoading = false;
  private copyButton: HTMLButtonElement;
  private copyAsTextCompleted = false;
  private listName: string;
  state: State;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly validationController: ValidationController,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.eventAggregator.subscribe("get-lists-finished", () => {
      this.setModelFromState();
    });

    this.validationController.validateTrigger = validateTrigger.manual;

    this.eventAggregator.subscribe(AlertEvents.OnHidden, () => {
      this.nameIsInvalid = false;
    });
  }

  async activate(params: any) {
    this.listId = parseInt(params.id, 10);
  }

  attached() {
    if (!this.state.loading) {
      this.setModelFromState();
    }

    this.copyButton.addEventListener("click", () => {
      this.copyAsText();
    });
  }

  setModelFromState() {
    const list = this.state.lists.find((x) => x.id === this.listId);
    if (!list) {
      this.router.navigateToRoute("notFound");
    } else {
      this.model = JSON.parse(JSON.stringify(list));
      this.listName = this.model.name;

      this.model.name = (this.i18n.tr("copyList.copyOf") + " " + this.model.name).substring(0, 50);

      ValidationRules.ensure((x: List) => x.name)
        .required()
        .on(this.model);
    }
  }

  selectIcon(icon: string) {
    this.model.icon = icon;
  }

  @computedFrom("model.name")
  get canSave(): boolean {
    return !ValidationUtil.isEmptyOrWhitespace(this.model.name);
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
        this.model.id = await this.listsService.copy(this.model);
        this.nameIsInvalid = false;

        await Actions.getLists(this.listsService, this.i18n.tr("highPriority"));

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "copyList.copySuccessful");

        this.router.navigateToRoute("listsEdited", {
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

  copyAsText() {
    let text = this.listName;

    const tasks = this.model.tasks
      .filter((x) => !x.isCompleted && !x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });
    const privateTasks = this.model.tasks
      .filter((x) => !x.isCompleted && x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });
    const completedTasks = this.model.tasks
      .filter((x) => x.isCompleted && !x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });
    const completedPrivateTasks = this.model.tasks
      .filter((x) => x.isCompleted && x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });

    if (privateTasks.length + tasks.length > 0) {
      text += "\n";

      for (let task of privateTasks) {
        text += `\n${task.name} ☐`;
      }
      for (let task of tasks) {
        text += `\n${task.name} ☐`;
      }
    }

    if (completedPrivateTasks.length + completedTasks.length > 0) {
      if (tasks.length > 0) {
        text += "\n----------";
      }

      for (let task of completedPrivateTasks) {
        text += `\n${task.name} 🗹`;
      }
      for (let task of completedTasks) {
        text += `\n${task.name} 🗹`;
      }
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
}
