import store from "./store";
import * as Mutations from "./mutations";
import { SearchFilters } from "models/viewmodels/searchFilters";

function changeFilters(filters: SearchFilters) {
  store.dispatch(Mutations.changeFilters, filters);
}

export { changeFilters };
