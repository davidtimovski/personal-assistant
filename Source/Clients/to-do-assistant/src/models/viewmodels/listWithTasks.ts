import { Task } from "../entities/task";
import { SharingState } from "./sharingState";

export class ListWithTasks {
  constructor(
    public id: number,
    public name: string,
    public icon: string,
    public isOneTimeToggleDefault: boolean,
    public sharingState: SharingState,
    public isArchived: boolean,
    public tasks: Array<Task>,
    public privateTasks: Array<Task>,
    public completedTasks: Array<Task>,
    public completedPrivateTasks: Array<Task>
  ) {}
}
