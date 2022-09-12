import { Language } from "../models/enums/language";

export class LocalStorageBase {
  private readonly defaults = new Map<string, any>([
    ["language", Language.English],
    ["profileImageUri", null],
    ["profileImageUriLastLoad", 0],
  ]);

  constructor(additionalDefaults?: Map<string, any>) {
    if (additionalDefaults) {
      this.defaults = new Map([...this.defaults, ...additionalDefaults]);
    }
  }

  public get(key: string): string {
    const value = window.localStorage.getItem(key);
    if (!value) {
      this.set(key, this.defaults.get(key));
      return this.defaults.get(key);
    }

    return value;
  }

  public getObject<Type>(key: string): Type | null {
    const value = window.localStorage.getItem(key);
    if (!value) {
      return null;
    }

    return JSON.parse(value);
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
