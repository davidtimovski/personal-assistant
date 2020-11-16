import { json } from "aurelia-fetch-client";
import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { Task } from "models/entities/task";
import { EditTaskModel } from "models/viewmodels/editTaskModel";

export class TasksService extends HttpProxyBase {
  async get(id: number): Promise<Task> {
    const result = await this.ajax<Task>(`tasks/${id}`);

    return result;
  }

  async getForUpdate(id: number): Promise<EditTaskModel> {
    const result = await this.ajax<EditTaskModel>(`tasks/${id}/update`);

    return result;
  }

  async create(
    listId: number,
    name: string,
    isOneTime: boolean,
    isPrivate: boolean
  ): Promise<number> {
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
  }

  async bulkCreate(
    listId: number,
    tasksText: string,
    tasksAreOneTime: boolean,
    tasksArePrivate: boolean
  ): Promise<void> {
    await this.ajaxExecute("tasks/bulk", {
      method: "post",
      body: json({
        listId: listId,
        tasksText: tasksText,
        tasksAreOneTime: tasksAreOneTime,
        tasksArePrivate: tasksArePrivate,
      }),
    });
  }

  async update(editTaskViewModel: EditTaskModel): Promise<void> {
    await this.ajaxExecute("tasks", {
      method: "put",
      body: json(editTaskViewModel),
    });
  }

  async delete(id: number): Promise<void> {
    await this.ajaxExecute(`tasks/${id}`, {
      method: "delete",
    });
  }

  async complete(id: number): Promise<void> {
    await this.ajaxExecute("tasks/complete", {
      method: "put",
      body: json({
        id: id,
      }),
    });
  }

  async uncomplete(id: number): Promise<void> {
    await this.ajaxExecute("tasks/uncomplete", {
      method: "put",
      body: json({
        id: id,
      }),
    });
  }

  async reorder(id: number, oldOrder: number, newOrder: number): Promise<void> {
    await this.ajaxExecute("tasks/reorder", {
      method: "put",
      body: json({
        id: id,
        oldOrder: oldOrder,
        newOrder: newOrder,
      }),
    });
  }
}
