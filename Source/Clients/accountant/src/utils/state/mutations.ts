import store from "./store";
import { State } from "./state";
import { SearchFilters } from "models/viewmodels/searchFilters";

function changeFilters(state: State, changedFilters: SearchFilters) {
  const newState = Object.assign({}, state);

  newState.filters = changedFilters;

  return newState;
}

store.registerAction("changeFilters", changeFilters);

export { changeFilters };
