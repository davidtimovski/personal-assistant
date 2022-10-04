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
    let value = window.localStorage.getItem(key);
    if (!value) {
      value = <string>this.defaults.get(key);
      this.set(key, value);
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

  public clear() {
    for (const key of this.defaults.keys()) {
      window.localStorage.removeItem(key);
    }
  }

  private toBool(value: string) {
    return value === "true";
  }
}
