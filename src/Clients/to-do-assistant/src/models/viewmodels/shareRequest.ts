import { Loadable } from "../loadable";

export class ShareRequest implements Loadable {
  constructor(
    public listId: number,
    public listName: string,
    public listOwnerName: string,
    public isAccepted: boolean,
    public leftSideIsLoading: boolean,
    public rightSideIsLoading: boolean
  ) {}
}
