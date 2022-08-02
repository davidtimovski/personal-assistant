import { Language } from "../models/enums/language";

export class LocalStorageBase {
  private readonly defaults: { [name: string]: any } = {
    language: Language.English,
    profileImageUri: null,
    profileImageUriLastLoad: 0,
  };

  constructor(additionalDefaults?: any) {
    if (additionalDefaults) {
      this.defaults = { ...this.defaults, ...additionalDefaults };
    }
  }

  public get(key: string): string {
    const value = window.localStorage.getItem(key);
    if (value) {
      return value;
    }

    this.set(key, this.defaults[key]);
    return this.defaults[key];
  }

  public getBool(key: string): boolean {
    return this.toBool(this.get(key));
  }

  public set(key: string, value: any) {
    window.localStorage.setItem(key, value);
  }

  private toBool(value: string) {
    return value === "true";
  }
}
