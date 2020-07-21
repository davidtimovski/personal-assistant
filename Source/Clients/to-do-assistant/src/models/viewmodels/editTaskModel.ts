export class EditTaskModel {
  constructor(
    public id: number,
    public listId: number,
    public name: string,
    public isCompleted: boolean,
    public isOneTime: boolean,
    public isPrivate: boolean,
    public assignedToUserId: number,
    public isInSharedList: boolean,
    public order: number,
    public createdDate: Date,
    public modifiedDate: Date,
    public recipes: Array<string>
  ) {}
}
