import { AssignedUser } from "../viewmodels/assignedUser";

export class Task {
  constructor(
    public id: number,
    public listId: number,
    public name: string,
    public isCompleted: boolean,
    public isOneTime: boolean,
    public isHighPriority: boolean,
    public isPrivate: boolean,
    public assignedUser: AssignedUser,
    public order: number
  ) {}
}
