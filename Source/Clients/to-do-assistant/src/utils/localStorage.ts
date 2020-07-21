import { LocalStorageBase } from "../../../shared/src/utils/localStorageBase";

export class LocalStorage extends LocalStorageBase {
  initialize() {
    super.initialize();

    const dataLastLoad = window.localStorage.getItem("dataLastLoad");
    if (!dataLastLoad) {
      this.setDataLastLoad(new Date(0));
    }

    const soundsEnabled = window.localStorage.getItem("soundsEnabled");
    if (!soundsEnabled) {
      this.setSoundsEnabled(true);
    }
  }

  setDataLastLoad(lastLoad: Date) {
    window.localStorage.setItem("dataLastLoad", lastLoad.getTime().toString());
  }

  getSoundsEnabled(): boolean {
    const soundsEnabled = window.localStorage.getItem("soundsEnabled");
    return soundsEnabled === "true";
  }
  setSoundsEnabled(soundsEnabled: boolean) {
    window.localStorage.setItem("soundsEnabled", soundsEnabled.toString());
  }
}
