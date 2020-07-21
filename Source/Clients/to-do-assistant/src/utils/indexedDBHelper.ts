import { Task } from "models/entities/task";
import { ListWithTasks } from "models/viewmodels/listWithTasks";
import { SimpleList } from "models/viewmodels/simpleList";
import { IDBContext } from "./idbContext";

export class IndexedDBHelper {
  private readonly db = new IDBContext();

  async getLists(): Promise<Array<SimpleList>> {
    return await this.db.lists.toCollection().sortBy("order");
  }

  async getListWithTasks(id: number): Promise<ListWithTasks> {
    const dbList = await this.db.lists.get(id);
    if (!dbList) {
      return null;
    }

    const list = new ListWithTasks(
      dbList.id,
      dbList.name,
      dbList.icon,
      dbList.isOneTimeToggleDefault,
      dbList.sharingState,
      false,
      null,
      null,
      null,
      null
    );

    const tasks = await this.db.tasks
      .where("listId")
      .equals(id)
      .toArray();

    list.tasks = tasks
      .filter((task: Task) => {
        return !task.isCompleted && !task.isPrivate;
      })
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });
    list.privateTasks = tasks
      .filter((task: Task) => {
        return !task.isCompleted && task.isPrivate;
      })
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });
    list.completedTasks = tasks
      .filter((task: Task) => {
        return task.isCompleted && !task.isPrivate;
      })
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });
    list.completedPrivateTasks = tasks
      .filter((task: Task) => {
        return task.isCompleted && task.isPrivate;
      })
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      });

    return list;
  }

  async createList(list: SimpleList) {
    await this.db.lists.put(list);
  }

  async createTasksInList(listId: number, tasks: Array<Task>) {
    return this.db.transaction("rw", this.db.tasks, function*() {
      yield this.db.tasks
        .where("listId")
        .equals(listId)
        .delete();

      for (const task of tasks) {
        yield this.db.tasks.put(task);
      }
    });
  }

  async deleteListWithTasks(id: number) {
    return this.db.transaction("rw", this.db.lists, this.db.tasks, function*() {
      yield this.db.lists.delete(id);
      yield this.db.tasks
        .where("listId")
        .equals(id)
        .delete();
    });
  }

  async createListsWithTasks(lists: Array<SimpleList>) {
    return this.db.transaction("rw", this.db.lists, this.db.tasks, async () => {
      await this.db.lists.clear();
      await this.db.tasks.clear();

      for (const list of lists) {
        await this.db.lists.put(list);
        await this.db.tasks.bulkPut(list.tasks);
      }
    });
  }

  async createTasks(...tasks: Array<Task>) {
    return this.db.transaction("rw", this.db.tasks, async () => {
      await this.db.tasks.bulkPut(tasks);
    });
  }

  async deleteTask(id: number) {
    await this.db.tasks.delete(id);
  }
}
