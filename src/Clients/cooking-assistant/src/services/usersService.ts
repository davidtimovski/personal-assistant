import { json } from "aurelia-fetch-client";

import { UsersServiceBase } from "../../../shared/src/services/usersServiceBase";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { PreferencesModel } from "../models/preferencesModel";
import * as environment from "../../config/environment.json";

export class UsersService extends UsersServiceBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);
  private preferences: PreferencesModel;

  async getPreferences(): Promise<PreferencesModel> {
    if (!this.preferences) {
      this.preferences = await this.ajax<PreferencesModel>("users/cooking-preferences");
    }

    return this.preferences;
  }

  async updateNotificationsEnabled(enabled: boolean): Promise<void> {
    try {
      await this.ajaxExecute("users/cooking-notifications-enabled", {
        method: "put",
        body: json({
          cookingNotificationsEnabled: enabled,
        }),
      });

      this.preferences.notificationsEnabled = enabled;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async updateImperialSystem(imperialSystem: boolean): Promise<void> {
    try {
      await this.ajaxExecute("users/imperial-system", {
        method: "put",
        body: json({
          imperialSystem: imperialSystem,
        }),
      });

      this.preferences.imperialSystem = imperialSystem;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
