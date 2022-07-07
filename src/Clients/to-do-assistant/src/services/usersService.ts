import { json } from "aurelia-fetch-client";
import { UsersServiceBase } from "../../../shared/src/services/usersServiceBase";
import { PreferencesModel } from "models/preferencesModel";

export class UsersService extends UsersServiceBase {
  private preferences: PreferencesModel;

  async getPreferences(): Promise<PreferencesModel> {
    if (!this.preferences) {
      this.preferences = await this.httpProxy.ajax<PreferencesModel>("api/users/to-do-preferences");
    }
    return this.preferences;
  }

  async updateNotificationsEnabled(enabled: boolean): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/users/to-do-notifications-enabled", {
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
