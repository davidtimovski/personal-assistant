import store from "./store";
import * as Mutations from "./mutations";
import { RecipesService } from "services/recipesService";

async function getRecipes(service: RecipesService): Promise<void> {
  const recipes = await service.getAll();
  return store.dispatch(Mutations.getRecipes, recipes);
}

async function deleteRecipe(recipeId: number) {
  store.dispatch(Mutations.deleteRecipe, recipeId);
}

function setDataLastLoad(recipeId: number, lastOpenedDate: Date) {
  store.dispatch(Mutations.setDataLastLoad, recipeId, lastOpenedDate);
}

export { getRecipes, deleteRecipe, setDataLastLoad };
