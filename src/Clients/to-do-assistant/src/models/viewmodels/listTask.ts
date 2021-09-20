import { Task } from "models/entities/task";
import { AssignedUser } from "../viewmodels/assignedUser";

export class ListTask {
  constructor(
    public id: number,
    public listId: number,
    public name: string,
    public isCompleted: boolean,
    public isOneTime: boolean,
    public isHighPriority: boolean,
    public isPrivate: boolean,
    public assignedUser: AssignedUser,
    public order: number,
    public rightSideIsLoading: boolean,
    public isFading: boolean
  ) {}

  static fromTask(task: Task) {
    return new ListTask(
      task.id,
      task.listId,
      task.name,
      task.isCompleted,
      task.isOneTime,
      task.isHighPriority,
      task.isPrivate,
      task.assignedUser,
      task.order,
      false,
      false
    );
  }
}
