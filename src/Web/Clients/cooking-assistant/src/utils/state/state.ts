import { RecipeModel } from "models/viewmodels/recipeModel";

export interface State {
  recipes: RecipeModel[];
}

export const initialState: State = {
  recipes: null,
};
