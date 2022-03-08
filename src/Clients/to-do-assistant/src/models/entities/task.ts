import { AssignedUser } from "../viewmodels/assignedUser";

export class Task {
  id: number;
  listId: number;
  name: string;
  isCompleted: boolean;
  isOneTime: boolean;
  isHighPriority: boolean;
  isPrivate: boolean;
  assignedUser: AssignedUser;
  order: number;
}
