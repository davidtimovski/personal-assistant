import { IsLoadable } from "../isLoadable";

export class ShareRequest implements IsLoadable {
  recipeId: number;
  recipeName: string;
  recipeOwnerName: string;
  isAccepted: boolean;
  leftSideIsLoading: boolean;
  rightSideIsLoading: boolean;
}
