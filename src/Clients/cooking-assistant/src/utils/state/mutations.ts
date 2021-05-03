import store from "./store";
import { State } from "./state";
import { RecipeModel } from "models/viewmodels/recipeModel";

function getRecipes(state: State, recipes: Array<RecipeModel>) {
  if (!recipes) {
    return;
  }

  const newState = Object.assign({}, state, { recipes: [...recipes] });

  return newState;
}

function deleteRecipe(state: State, recipeId: number) {
  const newState = Object.assign({}, state, {
    recipes: [...state.recipes.filter((x) => x.id != recipeId)],
  });

  return newState;
}

function setDataLastLoad(state: State, recipeId: number, lastOpenedDate: Date) {
  const newState = Object.assign({}, state);

  let recipe = newState.recipes.find((x) => x.id == recipeId);
  recipe.lastOpenedDate = lastOpenedDate;

  return newState;
}

store.registerAction("getRecipes", getRecipes);
store.registerAction("deleteRecipe", deleteRecipe);
store.registerAction("setDataLastLoad", setDataLastLoad);

export { getRecipes, deleteRecipe, setDataLastLoad };
