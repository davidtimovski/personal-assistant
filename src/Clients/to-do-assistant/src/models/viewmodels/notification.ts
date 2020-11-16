export class Notification {
  constructor(
    public id: number,
    public listId: number,
    public taskId: number,
    public userImageUri: string,
    public message: string,
    public isSeen: boolean,
    public createdDate: string,
    public formattedCreatedDate: string
  ) {}
}
