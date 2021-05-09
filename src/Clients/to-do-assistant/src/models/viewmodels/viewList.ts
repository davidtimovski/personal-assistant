import { ListTask } from "./listTask";
import { SharingState } from "./sharingState";

export class ViewList {
  constructor(
    public id: number,
    public name: string,
    public isOneTimeToggleDefault: boolean,
    public sharingState: SharingState,
    public isArchived: boolean,
    public tasks: Array<ListTask>,
    public privateTasks: Array<ListTask>,
    public completedTasks: Array<ListTask>,
    public completedPrivateTasks: Array<ListTask>
  ) {}
}
