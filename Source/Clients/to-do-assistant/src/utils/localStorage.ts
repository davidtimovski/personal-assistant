import { LocalStorageBase } from "../../../shared/src/utils/localStorageBase";

export class LocalStorage extends LocalStorageBase {
  initialize() {
    super.initialize();

    const soundsEnabled = window.localStorage.getItem("soundsEnabled");
    if (!soundsEnabled) {
      this.setSoundsEnabled(true);
    }
  }

  getSoundsEnabled(): boolean {
    const soundsEnabled = window.localStorage.getItem("soundsEnabled");
    return soundsEnabled === "true";
  }
  setSoundsEnabled(soundsEnabled: boolean) {
    window.localStorage.setItem("soundsEnabled", soundsEnabled.toString());
  }
}
