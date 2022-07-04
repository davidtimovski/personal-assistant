import { json } from "aurelia-fetch-client";

import { UsersServiceBase } from "../../../shared/src/services/usersServiceBase";
import { ErrorLogger } from "../../../shared/src/services/errorLogger";

import { PreferencesModel } from "models/preferencesModel";
import * as environment from "../../config/environment.json";

export class UsersService extends UsersServiceBase {
  private readonly logger = new ErrorLogger(JSON.parse(<any>environment).urls.clientLogger, this.authService);
  private preferences: PreferencesModel;

  async getPreferences(): Promise<PreferencesModel> {
    if (!this.preferences) {
      this.preferences = await this.ajax<PreferencesModel>("users/to-do-preferences");
    }
    return this.preferences;
  }

  async updateNotificationsEnabled(enabled: boolean): Promise<void> {
    try {
      await this.ajaxExecute("users/to-do-notifications-enabled", {
        method: "put",
        body: json({
          toDoNotificationsEnabled: enabled,
        }),
      });

      this.preferences.notificationsEnabled = enabled;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
