import { Task } from "../entities/task";
import { SharingState } from "./sharingState";

export class SimpleList {
  constructor(
    public id: number,
    public name: string,
    public icon: string,
    public isOneTimeToggleDefault: boolean,
    public sharingState: SharingState,
    public order: number,
    public tasks: Array<Task>
  ) {}
}
