export class BulkAddTasksModel {
  constructor(
    public listId: number,
    public tasksText: string,
    public tasksAreOneTime: boolean,
    public tasksArePrivate: boolean
  ) {}
}
