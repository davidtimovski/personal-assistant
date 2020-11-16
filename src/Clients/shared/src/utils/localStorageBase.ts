import { Language } from "../models/enums/language";

export class LocalStorageBase {
  private readonly defaultLanguage = Language.English;
  private readonly ttlMinutes = {
    profileImageUri: 1440,
    data: 15,
  };

  initialize() {
    const language = window.localStorage.getItem("language");
    if (!language) {
      window.localStorage.setItem("language", this.defaultLanguage);
    }

    const profileImageUri = window.localStorage.getItem("profileImageUri");
    if (!profileImageUri) {
      window.localStorage.setItem("profileImageUri", "");
    }

    const profileImageUriLastLoad = window.localStorage.getItem(
      "profileImageUriLastLoad"
    );
    if (!profileImageUriLastLoad) {
      this.setProfileImageUriLastLoad(new Date(0));
    }
  }

  getLanguage(): string {
    const language = window.localStorage.getItem("language");
    return language ? language : this.defaultLanguage;
  }
  setLanguageFromUrl() {
    const urlLanguage = window.location.pathname
      ? window.location.pathname.substr(1)
      : "";
    if (
      [Language.English.toString(), Language.Macedonian.toString()].indexOf(
        urlLanguage
      ) !== -1 &&
      urlLanguage !== window.localStorage.getItem("language")
    ) {
      window.localStorage.setItem("language", urlLanguage);
    }
  }

  getProfileImageUri(): string {
    return window.localStorage.getItem("profileImageUri");
  }
  setProfileImageUri(profileImageUri: string) {
    window.localStorage.setItem("profileImageUri", profileImageUri);
  }
  setProfileImageUriLastLoad(lastLoad: Date) {
    window.localStorage.setItem(
      "profileImageUriLastLoad",
      lastLoad.getTime().toString()
    );
  }

  isStale(dataType: string): boolean {
    const lastLoadValue = window.localStorage.getItem(`${dataType}LastLoad`);
    const lastLoad = lastLoadValue ? parseInt(lastLoadValue, 10) : 0;

    const diff = new Date().getTime() - lastLoad;
    const minutes = Math.floor(diff / 1000 / 60);

    const ttlMinutes: number = this.ttlMinutes[dataType];

    return minutes > ttlMinutes;
  }
}
