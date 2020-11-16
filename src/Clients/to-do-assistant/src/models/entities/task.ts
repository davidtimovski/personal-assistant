import { Loadable } from "../loadable";
import { AssignedUser } from "../viewmodels/assignedUser";

export class Task implements Loadable {
  constructor(
    public id: number,
    public listId: number,
    public name: string,
    public isCompleted: boolean,
    public isOneTime: boolean,
    public isPrivate: boolean,
    public assignedUser: AssignedUser,
    public order: number,
    public createdDate: Date,
    public modifiedDate: Date,
    public leftSideIsLoading: boolean,
    public rightSideIsLoading: boolean
  ) {}
}
