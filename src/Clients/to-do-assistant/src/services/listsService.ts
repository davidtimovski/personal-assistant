import { json } from "aurelia-fetch-client";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { List, Task } from "models/entities";
import { ListWithShares } from "models/viewmodels/listWithShares";
import { Share } from "models/viewmodels/share";
import { ShareRequest } from "models/viewmodels/shareRequest";
import { ListOption } from "models/viewmodels/listOption";
import { CanShareList } from "models/viewmodels/canShareList";
import { AssigneeOption } from "models/viewmodels/assigneeOption";
import { ListIcon } from "models/viewmodels/listIcon";
import { EditListModel } from "models/viewmodels/editListModel";
import { ArchivedList } from "models/viewmodels/archivedList";
import { ListModel } from "models/viewmodels/listModel";
import * as Actions from "utils/state/actions";
import * as environment from "../../config/environment.json";

export class ListsService extends HttpProxyBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);

  async getAll(): Promise<List[]> {
    const result = await this.ajax<List[]>("lists");

    return result;
  }

  getAllAsOptions(): Promise<ListOption[]> {
    return this.ajax<ListOption[]>("lists/options");
  }

  get(id: number): Promise<EditListModel> {
    return this.ajax<EditListModel>(`lists/${id}`);
  }

  getWithShares(id: number): Promise<ListWithShares> {
    return this.ajax<ListWithShares>(`lists/${id}/with-shares`);
  }

  getShareRequests(): Promise<ShareRequest[]> {
    return this.ajax<ShareRequest[]>("lists/share-requests");
  }

  getPendingShareRequestsCount(): Promise<number> {
    return this.ajax<number>("lists/pending-share-requests-count");
  }

  getIsShared(id: number): Promise<boolean> {
    return this.ajax<boolean>(`lists/${id}/shared`);
  }

  getMembersAsAssigneeOptions(id: number): Promise<AssigneeOption[]> {
    return this.ajax<AssigneeOption[]>(`lists/${id}/members`);
  }

  async create(name: string, icon: string, isOneTimeToggleDefault: boolean, tasksText: string): Promise<number> {
    try {
      const id = await this.ajax<number>("lists", {
        method: "post",
        body: json({
          name: name,
          icon: icon,
          isOneTimeToggleDefault: isOneTimeToggleDefault,
          tasksText: tasksText,
        }),
      });

      await Actions.getLists(this);

      return id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async update(list: EditListModel): Promise<void> {
    try {
      await this.ajaxExecute("lists", {
        method: "put",
        body: json(list),
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async updateShared(list: EditListModel): Promise<void> {
    try {
      await this.ajaxExecute("lists/shared", {
        method: "put",
        body: json({
          id: list.id,
          notificationsEnabled: list.notificationsEnabled,
        }),
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number): Promise<void> {
    try {
      await this.ajaxExecute(`lists/${id}`, {
        method: "delete",
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  canShareListWithUser(email: string): Promise<CanShareList> {
    return this.ajax<CanShareList>(`lists/can-share-with-user/${email}`);
  }

  async share(id: number, newShares: Share[], editedShares: Share[], removedShares: Share[]): Promise<void> {
    try {
      await this.ajaxExecute("lists/share", {
        method: "put",
        body: json({
          listId: id,
          newShares: newShares,
          editedShares: editedShares,
          removedShares: removedShares,
        }),
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async leave(id: number): Promise<void> {
    try {
      await this.ajaxExecute(`lists/${id}/leave`, {
        method: "delete",
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async copy(list: List): Promise<number> {
    try {
      const id = await this.ajax<number>("lists/copy", {
        method: "post",
        body: json({
          id: list.id,
          name: list.name,
          icon: list.icon,
        }),
      });

      await Actions.getLists(this);

      return id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  copyAsText(listName: string, listToCopy: List) {
    try {
      let text = listName;

      const tasks = listToCopy.tasks
        .filter((x) => !x.isCompleted && !x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        });
      const privateTasks = listToCopy.tasks
        .filter((x) => !x.isCompleted && x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        });
      const completedTasks = listToCopy.tasks
        .filter((x) => x.isCompleted && !x.isPrivate)
        .sort((a: Task, b: Task) => {
          return a.order - b.order;
        });
      const completedPrivateTasks = listToCopy.tasks
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
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async setIsArchived(id: number, isArchived: boolean): Promise<void> {
    try {
      await this.ajaxExecute("lists/is-archived", {
        method: "put",
        body: json({
          listId: id,
          isArchived: isArchived,
        }),
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async setTasksAsNotCompleted(id: number): Promise<void> {
    try {
      await this.ajaxExecute("lists/set-tasks-as-not-completed", {
        method: "put",
        body: json({
          listId: id,
        }),
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async setShareIsAccepted(id: number, isAccepted: boolean): Promise<void> {
    try {
      await this.ajaxExecute("lists/share-is-accepted", {
        method: "put",
        body: json({
          listId: id,
          isAccepted: isAccepted,
        }),
      });

      await Actions.getLists(this);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async reorder(id: number, oldOrder: number, newOrder: number): Promise<void> {
    try {
      await this.ajaxExecute("lists/reorder", {
        method: "put",
        body: json({
          id: id,
          oldOrder: oldOrder,
          newOrder: newOrder,
        }),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  static getForHomeScreen(lists: List[]): ListModel[] {
    return lists
      .filter((x) => !x.isArchived && !x.computedListType)
      .sort((a: List, b: List) => {
        return a.order - b.order;
      })
      .map(
        (x) =>
          new ListModel(
            x.id,
            x.name,
            x.icon,
            x.sharingState,
            x.order,
            x.computedListType,
            null,
            x.tasks.filter((x) => !x.isCompleted).length
          )
      );
  }

  static getComputedForHomeScreen(lists: List[], computedListNameLookup: any): ListModel[] {
    return lists
      .filter((x) => x.computedListType)
      .map(
        (x) =>
          new ListModel(
            x.id,
            computedListNameLookup[x.computedListType],
            x.icon,
            x.sharingState,
            x.order,
            x.computedListType,
            this.getComputedListIconClass(x.computedListType),
            x.tasks.filter((x) => !x.isCompleted).length
          )
      );
  }

  static getArchived(lists: List[]): ArchivedList[] {
    return lists
      .filter((x) => x.isArchived)
      .sort((a: List, b: List) => {
        const aDate = new Date(a.modifiedDate);
        const bDate = new Date(b.modifiedDate);
        if (aDate > bDate) return -1;
        if (aDate < bDate) return 1;
        return 0;
      })
      .map((x) => new ArchivedList(x.id, x.name, x.icon, x.sharingState));
  }

  static getIconOptions(): ListIcon[] {
    return [
      new ListIcon("list", "fas fa-list"),
      new ListIcon("shopping", "fas fa-shopping-cart"),
      new ListIcon("home", "fas fa-home"),
      new ListIcon("birthday", "fas fa-birthday-cake"),
      new ListIcon("cheers", "fas fa-glass-cheers"),
      new ListIcon("vacation", "fas fa-umbrella-beach"),
      new ListIcon("plane", "fas fa-plane-departure"),
      new ListIcon("car", "fas fa-car"),
      new ListIcon("pickup-truck", "fas fa-truck-pickup"),
      new ListIcon("world", "fas fa-globe-americas"),
      new ListIcon("camping", "fas fa-campground"),
      new ListIcon("motorcycle", "fas fa-motorcycle"),
      new ListIcon("bicycle", "fas fa-bicycle"),
      new ListIcon("ski", "fas fa-skiing"),
      new ListIcon("snowboard", "fas fa-snowboarding"),
      new ListIcon("work", "fas fa-briefcase"),
      new ListIcon("baby", "fas fa-baby-carriage"),
      new ListIcon("dog", "fas fa-dog"),
      new ListIcon("cat", "fas fa-cat"),
      new ListIcon("fish", "fas fa-fish"),
      new ListIcon("camera", "fas fa-camera"),
      new ListIcon("medicine", "fas fa-prescription-bottle-alt"),
      new ListIcon("file", "fas fa-file-alt"),
      new ListIcon("book", "fas fa-book"),
      new ListIcon("mountain", "fas fa-mountain"),
    ];
  }

  static highPriorityComputedListMoniker = "high-priority";

  static getComputedListIconClass(type: string): string {
    if (type === this.highPriorityComputedListMoniker) {
      return "fas fa-exclamation-triangle";
    }

    throw "No such computed list type";
  }
}
