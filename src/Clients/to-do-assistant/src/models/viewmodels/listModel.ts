import { Task } from "../entities/task";
import { SharingState } from "../viewmodels/sharingState";

export class ListModel {
  constructor(
    public id: number,
    public name: string,
    public icon: string,
    public sharingState: SharingState,
    public order: number,
    public computedListType: string,
    public tasks: Array<Task>
  ) {}

  public get isEmpty() {
    return this.tasks.every((x) => x.isCompleted);
  }
}
