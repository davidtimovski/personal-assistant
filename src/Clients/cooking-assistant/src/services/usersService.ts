import { json } from "aurelia-fetch-client";
import { UsersServiceBase } from "../../../shared/src/services/usersServiceBase";
import { PreferencesModel } from "models/preferencesModel";

export class UsersService extends UsersServiceBase {
  private preferences: PreferencesModel;

  async getPreferences(): Promise<PreferencesModel> {
    if (!this.preferences) {
      this.preferences = await this.httpProxy.ajax<PreferencesModel>("api/users/cooking-preferences");
    }

    return this.preferences;
  }

  async updateNotificationsEnabled(enabled: boolean): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/users/cooking-notifications-enabled", {
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
      await this.httpProxy.ajaxExecute("api/users/imperial-system", {
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
