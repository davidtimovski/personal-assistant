import { Loadable } from "../loadable";

export class ShareRequest implements Loadable {
  constructor(
    public recipeId: number,
    public recipeName: string,
    public recipeOwnerName: string,
    public isAccepted: boolean,
    public leftSideIsLoading: boolean,
    public rightSideIsLoading: boolean
  ) {}
}
