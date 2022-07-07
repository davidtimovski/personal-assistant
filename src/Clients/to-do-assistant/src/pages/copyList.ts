import { autoinject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { ValidationController, validateTrigger, ValidationRules, ControllerValidateResult } from "aurelia-validation";
import { I18N } from "aurelia-i18n";
import { EventAggregator } from "aurelia-event-aggregator";
import { connectTo } from "aurelia-store";

import { ValidationUtil } from "../../../shared/src/utils/validationUtil";
import { AlertEvents } from "../../../shared/src/models/enums/alertEvents";
import { ValidationErrors } from "../../../shared/src/models/validationErrors";

import { List } from "models/entities";
import { ListsService } from "services/listsService";
import { SharingState } from "models/viewmodels/sharingState";
import { State } from "utils/state/state";
import { AppEvents } from "models/appEvents";

@autoinject
@connectTo()
export class CopyList {
  private listId: number;
  private model = new List(0, "", "", false, false, SharingState.NotShared, 0, false, null, [], null);
  private nameIsInvalid: boolean;
  private readonly iconOptions = ListsService.getIconOptions();
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
    this.eventAggregator.subscribe(AppEvents.ListsChanged, () => {
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

        this.eventAggregator.publish(AlertEvents.ShowSuccess, "copyList.copySuccessful");

        this.router.navigateToRoute("listsEdited", {
          editedId: this.model.id,
        });
      } catch (e) {
        if (e instanceof ValidationErrors) {
          this.nameIsInvalid = e.fields.includes("Name");
        }

        this.saveButtonIsLoading = false;
      }
    } else {
      this.saveButtonIsLoading = false;
    }
  }

  copyAsText() {
    this.listsService.copyAsText(this.listName, this.model);

    this.copyAsTextCompleted = true;
    window.setTimeout(() => {
      this.copyAsTextCompleted = false;
    }, 2000);
  }
}
