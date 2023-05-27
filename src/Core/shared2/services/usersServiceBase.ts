import { HttpProxy } from "../services/httpProxy";
import { LocalStorageBase } from "../utils/localStorageBase";
import type { User } from "../models/user";
import { ErrorLogger } from "./errorLogger";
import Variables from "$lib/variables";

export class UsersServiceBase {
  protected readonly httpProxy = new HttpProxy();
  protected readonly localStorageBase = new LocalStorageBase();
  protected readonly logger: ErrorLogger;

  constructor(private readonly application: string) {
    this.logger = new ErrorLogger(this.application);
  }

  async get<Type extends User>(): Promise<Type> {
    try {
      const user = await this.httpProxy.ajax<Type>(
        `${Variables.urls.gateway}/core/api/users?application=${this.application}`
      );

      // 80x80 thumbnail
      const uriEnvironment = user.imageUri.includes("production")
        ? "production"
        : "development";
      user.imageUri = user.imageUri.replace(
        uriEnvironment,
        `w_80,h_80,c_limit/${uriEnvironment}`
      );

      return user;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  getFromCache<Type extends User>(): Type | null {
    return this.localStorageBase.getObject<Type>("user");
  }

  cache<Type extends User>(user: Type) {
    this.localStorageBase.set("user", window.JSON.stringify(user));
  }

  release() {
    this.httpProxy.release();
    this.logger.release();
  }
}
