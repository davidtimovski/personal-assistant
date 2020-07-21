import { json } from "aurelia-fetch-client";
import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { IndexedDBHelper } from "../utils/indexedDBHelper";
import { List } from "models/entities/list";
import { ListWithTasks } from "models/viewmodels/listWithTasks";
import { ListWithShares } from "models/viewmodels/listWithShares";
import { Share } from "models/viewmodels/share";
import { ShareRequest } from "models/viewmodels/shareRequest";
import { SimpleList } from "models/viewmodels/simpleList";
import { ArchivedList } from "models/viewmodels/archivedList";
import { ListOption } from "models/viewmodels/listOption";
import { CanShareList } from "models/viewmodels/canShareList";
import { SharingState } from "models/viewmodels/sharingState";
import { AssigneeOption } from "models/viewmodels/assigneeOption";
import { ListIcon } from "models/viewmodels/listIcon";

export class ListsService extends HttpProxyBase {
  private readonly idbHelper = new IndexedDBHelper();

  async getAll(): Promise<Array<SimpleList>> {
    const result = await this.ajax<Array<SimpleList>>("lists");

    return result;
  }

  async getAllAsOptions(): Promise<Array<ListOption>> {
    const result = await this.ajax<Array<ListOption>>("lists/options");

    return result;
  }

  async getAllArchived(): Promise<Array<ArchivedList>> {
    const result = await this.ajax<Array<ArchivedList>>("lists/archived");

    return result;
  }

  async get(id: number): Promise<List> {
    const result = await this.ajax<List>(`lists/${id}`);

    return result;
  }

  async getWithTasks(
    id: number,
    cache: boolean = true
  ): Promise<ListWithTasks> {
    const result = await this.ajax<ListWithTasks>(`lists/${id}/with-tasks`);

    if (cache) {
      this.idbHelper.createTasksInList(
        result.id,
        result.tasks
          .concat(result.privateTasks)
          .concat(result.completedTasks)
          .concat(result.completedPrivateTasks)
      );
    }

    return result;
  }

  async getWithShares(id: number): Promise<ListWithShares> {
    const result = await this.ajax<ListWithShares>(`lists/${id}/with-shares`);

    return result;
  }

  async getShareRequests(): Promise<Array<ShareRequest>> {
    const result = await this.ajax<Array<ShareRequest>>("lists/share-requests");

    return result;
  }

  async getPendingShareRequestsCount(): Promise<number> {
    const result = await this.ajax<number>(
      "lists/pending-share-requests-count"
    );

    return result;
  }

  async getIsShared(id: number): Promise<boolean> {
    const result = await this.ajax<boolean>(`lists/${id}/shared`);

    return result;
  }

  async getMembersAsAssigneeOptions(
    id: number
  ): Promise<Array<AssigneeOption>> {
    const result = await this.ajax<Array<AssigneeOption>>(
      `lists/${id}/members`
    );

    return result;
  }

  async create(
    name: string,
    icon: string,
    isOneTimeToggleDefault: boolean,
    tasksText: string
  ): Promise<number> {
    const list = await this.ajax<SimpleList>("lists", {
      method: "post",
      body: json({
        name: name,
        icon: icon,
        isOneTimeToggleDefault: isOneTimeToggleDefault,
        tasksText: tasksText,
      }),
    });

    this.idbHelper.createList(
      new SimpleList(
        list.id,
        name.trim(),
        icon.trim(),
        isOneTimeToggleDefault,
        SharingState.NotShared,
        list.order,
        []
      )
    );

    return list.id;
  }

  async update(list: List): Promise<void> {
    await this.ajaxExecute("lists", {
      method: "put",
      body: json(list),
    });

    if (!list.isArchived) {
      this.idbHelper.createList(
        new SimpleList(
          list.id,
          list.name.trim(),
          list.icon.trim(),
          list.isOneTimeToggleDefault,
          list.sharingState,
          list.order,
          []
        )
      );
    }
  }

  async updateShared(list: List): Promise<void> {
    await this.ajaxExecute("lists/shared", {
      method: "put",
      body: json({
        id: list.id,
        notificationsEnabled: list.notificationsEnabled,
      }),
    });

    if (!list.isArchived) {
      this.idbHelper.createList(
        new SimpleList(
          list.id,
          list.name.trim(),
          list.icon.trim(),
          list.isOneTimeToggleDefault,
          list.sharingState,
          list.order,
          []
        )
      );
    }
  }

  async delete(id: number): Promise<void> {
    await this.ajaxExecute(`lists/${id}`, {
      method: "delete",
    });

    this.idbHelper.deleteListWithTasks(id);
  }

  async canShareListWithUser(email: string): Promise<CanShareList> {
    const result = await this.ajax<CanShareList>(
      `lists/can-share-with-user/${email}`
    );

    return result;
  }

  async share(
    id: number,
    newShares: Array<Share>,
    editedShares: Array<Share>,
    removedShares: Array<Share>
  ): Promise<void> {
    await this.ajaxExecute("lists/share", {
      method: "put",
      body: json({
        listId: id,
        newShares: newShares,
        editedShares: editedShares,
        removedShares: removedShares,
      }),
    });
  }

  async leave(id: number): Promise<void> {
    await this.ajaxExecute(`lists/${id}/leave`, {
      method: "delete",
    });

    this.idbHelper.deleteListWithTasks(id);
  }

  async copy(list: ListWithTasks): Promise<number> {
    const id = await this.ajax<number>("lists/copy", {
      method: "post",
      body: json({
        id: list.id,
        name: list.name,
        icon: list.icon,
      }),
    });

    return id;
  }

  async setIsArchived(id: number, isArchived: boolean): Promise<void> {
    await this.ajaxExecute("lists/is-archived", {
      method: "put",
      body: json({
        listId: id,
        isArchived: isArchived,
      }),
    });

    if (isArchived) {
      this.idbHelper.deleteListWithTasks(id);
    }
  }

  async setTasksAsNotCompleted(id: number): Promise<void> {
    await this.ajaxExecute("lists/set-tasks-as-not-completed", {
      method: "put",
      body: json({
        listId: id,
      }),
    });
  }

  async setShareIsAccepted(id: number, isAccepted: boolean): Promise<void> {
    await this.ajaxExecute("lists/share-is-accepted", {
      method: "put",
      body: json({
        listId: id,
        isAccepted: isAccepted,
      }),
    });
  }

  async reorder(id: number, oldOrder: number, newOrder: number): Promise<void> {
    await this.ajaxExecute("lists/reorder", {
      method: "put",
      body: json({
        id: id,
        oldOrder: oldOrder,
        newOrder: newOrder,
      }),
    });
  }

  static getIconOptions(): Array<ListIcon> {
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
}
