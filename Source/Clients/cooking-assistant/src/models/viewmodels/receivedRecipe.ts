import { IsLoadable } from "../isLoadable";

export class ReceivedRecipe implements IsLoadable {
  constructor(
    public recipeId: number,
    public recipeName: string,
    public recipeSenderName: string,
    public isDeclined: boolean,
    public leftSideIsLoading: boolean,
    public rightSideIsLoading: boolean
  ) {}
}
