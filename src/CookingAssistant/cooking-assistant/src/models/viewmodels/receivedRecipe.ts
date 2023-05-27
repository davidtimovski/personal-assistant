import { IsLoadable } from "../isLoadable";

export class ReceivedRecipe implements IsLoadable {
  recipeId: number;
  recipeName: string;
  recipeSenderName: string;
  isDeclined: boolean;
  leftSideIsLoading: boolean;
  rightSideIsLoading: boolean;
}
