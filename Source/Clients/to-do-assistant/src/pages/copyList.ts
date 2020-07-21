import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { List } from "models/entities/list";
import { ListsService } from "services/listsService";
import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import {
  ValidationController,
  validateTrigger,
  ValidationRules,
  ControllerValidateResult,
} from "aurelia-validation";
import { LocalStorage } from "utils/localStorage";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { ListWithTasks } from "models/viewmodels/listWithTasks";

@inject(
  Router,
  ListsService,
  ValidationController,
  LocalStorage,
  I18N,
  EventAggregator
)
export class CopyList {
  private list: ListWithTasks;
  private nameIsInvalid: boolean;
  private iconOptions = ListsService.getIconOptions();
  private saveButtonIsLoading = false;
  private copyButton: HTMLButtonElement;
  private copyAsTextCompleted = false;
  private listName: string;

  constructor(
    private readonly router: Router,
    private readonly listsService: ListsService,
    private readonly validationController: ValidationController,
    private readonly localStorage: LocalStorage,
    private readonly i18n: I18N,
    private readonly eventAggregator: EventAggregator
  ) {
    this.validationController.validateTrigger = validateTrigger.manual;

    this.eventAggregator.subscribe("alert-hidden", () => {
      this.nameIsInvalid = false;
    });
  }

  async activate(params: any) {
    this.list = await this.listsService.getWithTasks(params.id);
    if (this.list === null) {
      this.router.navigateToRoute("notFound");
    }
    this.listName = this.list.name;

    this.list.name = (
      this.i18n.tr("copyList.copyOf") +
      " " +
      this.list.name
    ).substring(0, 50);

    ValidationRules.ensure((x: List) => x.name)
      .required()
      .on(this.list);
  }

  attached() {
    this.copyButton.addEventListener("click", () => {
      this.copyAsText();
    });
  }

  selectIcon(icon: string) {
    this.list.icon = icon;
  }

  @computedFrom("list.name")
  get canSave(): boolean {
    return !ValidationUtil.isEmptyOrWhitespace(this.list.name);
  }

  async save() {
    if (!this.canSave || this.saveButtonIsLoading) {
      return;
    }

    this.saveButtonIsLoading = true;
    this.eventAggregator.publish("reset-alert-error");

    const result: ControllerValidateResult = await this.validationController.validate();

    this.nameIsInvalid = !result.valid;

    if (result.valid) {
      try {
        const id = await this.listsService.copy(this.list);
        this.nameIsInvalid = false;
        this.list.id = id;

        this.localStorage.setDataLastLoad(new Date(0));

        this.eventAggregator.publish(
          "alert-success",
          "copyList.copySuccessful"
        );

        this.router.navigateToRoute("listsEdited", {
          editedId: this.list.id,
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

    if (this.list.privateTasks.length + this.list.tasks.length > 0) {
      text += "\n";

      for (let task of this.list.privateTasks) {
        text += `\n${task.name} ☐`;
      }
      for (let task of this.list.tasks) {
        text += `\n${task.name} ☐`;
      }
    }

    if (
      this.list.completedPrivateTasks.length + this.list.completedTasks.length >
      0
    ) {
      if (this.list.tasks.length > 0) {
        text += "\n----------";
      }

      for (let task of this.list.completedPrivateTasks) {
        text += `\n${task.name} 🗹`;
      }
      for (let task of this.list.completedTasks) {
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
