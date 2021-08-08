import { ListTask } from "./listTask";

export class ViewComputedList {
  public iconClass: string;

  constructor(
    public id: number,
    public name: string,
    public computedListType: string,
    public tasks: Array<ListTask>,
    public privateTasks: Array<ListTask>
  ) {}
}
