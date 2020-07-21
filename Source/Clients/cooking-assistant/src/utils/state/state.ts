import { RecipeModel } from "models/viewmodels/recipeModel";

export interface State {
  recipes: Array<RecipeModel>;
}

export const initialState: State = {
  recipes: null,
};
