import { json } from "aurelia-fetch-client";

import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { Task } from "models/entities/task";
import { EditTaskModel } from "models/viewmodels/editTaskModel";
import * as Actions from "utils/state/actions";
import * as environment from "../../config/environment.json";

export class TasksService extends HttpProxyBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);

  async get(id: number): Promise<Task> {
    const result = await this.ajax<Task>(`tasks/${id}`);

    return result;
  }

  async getForUpdate(id: number): Promise<EditTaskModel> {
    const result = await this.ajax<EditTaskModel>(`tasks/${id}/update`);

    return result;
  }

  async create(listId: number, name: string, isOneTime: boolean, isPrivate: boolean): Promise<number> {
    try {
      const id = await this.ajax<number>("tasks", {
        method: "post",
        body: json({
          listId: listId,
          name: name,
          isOneTime: isOneTime,
          isPrivate: isPrivate,
        }),
      });

      return id;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async bulkCreate(
    listId: number,
    tasksText: string,
    tasksAreOneTime: boolean,
    tasksArePrivate: boolean
  ): Promise<void> {
    try {
      await this.ajaxExecute("tasks/bulk", {
        method: "post",
        body: json({
          listId: listId,
          tasksText: tasksText,
          tasksAreOneTime: tasksAreOneTime,
          tasksArePrivate: tasksArePrivate,
        }),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async update(editTaskViewModel: EditTaskModel): Promise<void> {
    try {
      await this.ajaxExecute("tasks", {
        method: "put",
        body: json(editTaskViewModel),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async delete(id: number, listId: number): Promise<void> {
    try {
      await this.ajaxExecute(`tasks/${id}`, {
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
      await this.ajaxExecute("tasks/complete", {
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
      await this.ajaxExecute("tasks/uncomplete", {
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
      await this.ajaxExecute("tasks/reorder", {
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
