import { json } from "aurelia-fetch-client";
import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";
import { IndexedDBHelper } from "../utils/indexedDBHelper";
import { Task } from "models/entities/task";
import { EditTaskModel } from "models/viewmodels/editTaskModel";

export class TasksService extends HttpProxyBase {
  private readonly idbHelper = new IndexedDBHelper();

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
  ): Promise<Task> {
    const createdTask = await this.ajax<Task>("tasks", {
      method: "post",
      body: json({
        listId: listId,
        name: name,
        isOneTime: isOneTime,
        isPrivate: isPrivate,
      }),
    });

    this.idbHelper.createTasks(createdTask);

    return createdTask;
  }

  async bulkCreate(
    listId: number,
    tasksText: string,
    tasksAreOneTime: boolean,
    tasksArePrivate: boolean
  ): Promise<void> {
    const createdTasks = await this.ajax<Array<Task>>("tasks/bulk", {
      method: "post",
      body: json({
        listId: listId,
        tasksText: tasksText,
        tasksAreOneTime: tasksAreOneTime,
        tasksArePrivate: tasksArePrivate,
      }),
    });

    await this.idbHelper.createTasks(...createdTasks);
  }

  async update(editTaskViewModel: EditTaskModel): Promise<void> {
    await this.ajaxExecute("tasks", {
      method: "put",
      body: json(editTaskViewModel),
    });

    this.idbHelper.createTasks(
      new Task(
        editTaskViewModel.id,
        editTaskViewModel.listId,
        editTaskViewModel.name,
        editTaskViewModel.isCompleted,
        editTaskViewModel.isOneTime,
        editTaskViewModel.isPrivate,
        null,
        editTaskViewModel.order,
        editTaskViewModel.createdDate,
        editTaskViewModel.modifiedDate,
        false,
        false
      )
    );
  }

  async delete(id: number): Promise<void> {
    await this.ajaxExecute(`tasks/${id}`, {
      method: "delete",
    });

    this.idbHelper.deleteTask(id);
  }

  async setIsCompleted(task: Task): Promise<void> {
    this.idbHelper.createTasks(task);

    await this.ajaxExecute("tasks/is-completed", {
      method: "put",
      body: json({
        id: task.id,
        isCompleted: task.isCompleted,
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
