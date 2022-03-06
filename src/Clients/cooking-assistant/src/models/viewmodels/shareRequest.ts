import { Loadable } from "../loadable";

export class ShareRequest implements Loadable {
  recipeId: number;
  recipeName: string;
  recipeOwnerName: string;
  isAccepted: boolean;
  leftSideIsLoading: boolean;
  rightSideIsLoading: boolean;
}
