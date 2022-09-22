import { Task } from "models/entities";
import { ListTask } from "./listTask";

export class ViewComputedList {
  public tasks = new Array<ListTask>();
  public privateTasks = new Array<ListTask>();
  public iconClass: string;

  constructor(public id: number, public name: string, public computedListType: string) {}

  public setTasks(tasks: Task[]) {
    this.tasks = tasks
      .filter((x) => !x.isCompleted && !x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      })
      .map((x) => {
        return ListTask.fromTask(x);
      });
  }

  public setPrivateTasks(tasks: Task[]) {
    this.privateTasks = tasks
      .filter((x) => !x.isCompleted && x.isPrivate)
      .sort((a: Task, b: Task) => {
        return a.order - b.order;
      })
      .map((x) => {
        return ListTask.fromTask(x);
      });
  }
}
