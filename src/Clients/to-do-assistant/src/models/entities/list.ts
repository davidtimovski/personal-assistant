import { Task } from "./task";
import { SharingState } from "../viewmodels/sharingState";

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
    public tasks: Array<Task>,
    public createdDate: string,
    public modifiedDate: string
  ) {}
}
