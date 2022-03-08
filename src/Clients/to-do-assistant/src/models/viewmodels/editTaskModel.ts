export class EditTaskModel {
  id: number;
  listId: number;
  name: string;
  isCompleted: boolean;
  isOneTime: boolean;
  isPrivate: boolean;
  isHighPriority: boolean;
  assignedToUserId: number;
  isInSharedList: boolean;
  order: number;
  createdDate: Date;
  modifiedDate: Date;
  recipes: Array<string>;
}
