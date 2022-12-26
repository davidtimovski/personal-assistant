import { LocalStorageBase } from "../../../shared/src/utils/localStorageBase";

export class LocalStorage extends LocalStorageBase {
  initialize() {
    super.initialize();

    const soundsEnabled = window.localStorage.getItem("soundsEnabled");
    if (!soundsEnabled) {
      window.localStorage.setItem("soundsEnabled", "true");
    }

    const highPriorityListEnabled = window.localStorage.getItem("highPriorityListEnabled");
    if (!highPriorityListEnabled) {
      window.localStorage.setItem("highPriorityListEnabled", "true");
    }
  }
}
