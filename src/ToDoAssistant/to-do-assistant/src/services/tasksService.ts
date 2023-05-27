import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../../../shared/src/utils/httpProxy";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { ListsService } from "./listsService";
import { Task } from "models/entities";
import { EditTaskModel } from "models/viewmodels/editTaskModel";
import { BulkAddTasksModel } from "models/viewmodels/bulkAddTasksModel";
import * as Actions from "utils/state/actions";

@autoinject
export class TasksService {
  constructor(private readonly httpProxy: HttpProxy, private readonly logger: ErrorLogger) {}

  get(id: number): Promise<Task> {
    return this.httpProxy.ajax<Task>(`api/tasks/${id}`);
  }

  getForUpdate(id: number): Promise<EditTaskModel> {
    return this.httpProxy.ajax<EditTaskModel>(`api/tasks/${id}/update`);
  }

  async create(
    listId: number,
    name: string,
    isOneTime: boolean,
    isPrivate: boolean,
    listsService: ListsService
  ): Promise<number> {
    try {
      const id = await this.httpProxy.ajax<number>("api/tasks", {
        method: "post",
        body: json({
          listId: listId,
          name: name,
          isOneTime: isOneTime,
          isPrivate: isPrivate,
        }),
      });

      await Actions.getLists(listsService);

      return id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async bulkCreate(model: BulkAddTasksModel, listsService: ListsService): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/tasks/bulk", {
        method: "post",
        body: json({
          listId: model.listId,
          tasksText: model.tasksText,
          tasksAreOneTime: model.tasksAreOneTime,
          tasksArePrivate: model.tasksArePrivate,
        }),
      });

      await Actions.getLists(listsService);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async update(editTaskViewModel: EditTaskModel, listsService: ListsService): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/tasks", {
        method: "put",
        body: json(editTaskViewModel),
      });

      await Actions.getLists(listsService);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number, listId: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(`api/tasks/${id}`, {
        method: "delete",
      });

      await Actions.deleteTask(id, listId);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async complete(id: number, listId: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/tasks/complete", {
        method: "put",
        body: json({
          id: id,
        }),
      });

      await Actions.completeTask(id, listId);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async uncomplete(id: number, listId: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/tasks/uncomplete", {
        method: "put",
        body: json({
          id: id,
        }),
      });

      await Actions.uncompleteTask(id, listId);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async reorder(id: number, listId: number, oldOrder: number, newOrder: number): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/tasks/reorder", {
        method: "put",
        body: json({
          id: id,
          oldOrder: oldOrder,
          newOrder: newOrder,
        }),
      });

      await Actions.reorderTask(id, listId, oldOrder, newOrder);
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
