export class ShareRequest {
  constructor(
    public listId: number,
    public listName: string,
    public listOwnerName: string,
    public isAccepted: boolean,
    public leftSideIsLoading: boolean,
    public rightSideIsLoading: boolean
  ) {}
}
