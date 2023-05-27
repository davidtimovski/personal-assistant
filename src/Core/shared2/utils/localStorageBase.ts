export class LocalStorageBase {
  private readonly defaults: Map<string, any>;

  constructor(defaults?: Map<string, any>) {
    if (defaults) {
      this.defaults = defaults;
    } else {
      this.defaults = new Map<string, any>();
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
    window.localStorage.removeItem("user");
    for (const key of this.defaults.keys()) {
      window.localStorage.removeItem(key);
    }
  }

  private toBool(value: string) {
    return value === "true";
  }
}
