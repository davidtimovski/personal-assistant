import Dexie from "dexie";
import { SimpleList } from "models/viewmodels/simpleList";
import { Task } from "models/entities/task";

export class IDBContext extends Dexie {
  public lists: Dexie.Table<SimpleList, number>;
  public tasks: Dexie.Table<Task, number>;

  public constructor() {
    super("IDBContext");
    this.version(2).stores({
      lists: "id,order",
      tasks: "id,listId,order"
    });
    this.lists = this.table("lists");
    this.tasks = this.table("tasks");
  }
}
