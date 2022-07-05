import { SharingState } from "./viewmodels/sharingState";
import { AssignedUser } from "./viewmodels/assignedUser";

export class List {
  constructor(
    public id: number,
    public name: string,
    public icon: string,
    public notificationsEnabled: boolean,
    public isOneTimeToggleDefault: boolean,
    public sharingState: SharingState,
    public order: number,
    public isArchived: boolean,
    public computedListType: string,
    public tasks: Array<Task>,
    public modifiedDate: string
  ) {}
}

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
