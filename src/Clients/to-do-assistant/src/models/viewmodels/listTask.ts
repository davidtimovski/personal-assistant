import { Task } from "models/entities/task";
import { AssignedUser } from "../viewmodels/assignedUser";

export class ListTask {
  constructor(
    public id: number,
    public listId: number,
    public name: string,
    public isCompleted: boolean,
    public isOneTime: boolean,
    public isPrivate: boolean,
    public assignedUser: AssignedUser,
    public order: number,
    public rightSideIsLoading: boolean,
    public isDisappearing: boolean
  ) {}

  static fromTask(task: Task) {
    return new ListTask(
      task.id,
      task.listId,
      task.name,
      task.isCompleted,
      task.isOneTime,
      task.isPrivate,
      task.assignedUser,
      task.order,
      false,
      false
    );
  }
}
