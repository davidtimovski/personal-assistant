import { List } from "models/entities/list";

export interface State {
  loading: boolean;
  lists: Array<List>;
}

export const initialState: State = {
  loading: true,
  lists: null,
};
