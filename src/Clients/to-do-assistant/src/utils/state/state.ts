import { List } from "models/entities/list";

export interface State {
  loading: boolean;
  lists: List[];
  soundsEnabled: boolean;
  highPriorityListEnabled: boolean;
}

export const initialState: State = {
  loading: true,
  lists: null,
  soundsEnabled: window.localStorage.getItem("soundsEnabled") === "true",
  highPriorityListEnabled: window.localStorage.getItem("highPriorityListEnabled") === "true",
};
