import { Task } from "./task";
import { SharingState } from "../viewmodels/sharingState";

export class List {
  constructor(
    public id: number,
    public userId: number,
    public name: string,
    public icon: string,
    public tasksText: string,
    public order: number,
    public notificationsEnabled: boolean,
    public isOneTimeToggleDefault: boolean,
    public isArchived: boolean,
    public sharingState: SharingState,
    public tasks: Array<Task>,
    public createdDate: Date,
    public modifiedDate: Date
  ) {}
}
