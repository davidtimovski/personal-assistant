import { json } from "aurelia-fetch-client";
import { UsersServiceBase } from "../../../shared/src/services/usersServiceBase";
import { PreferencesModel } from "models/preferencesModel";

export class UsersService extends UsersServiceBase {
  private preferences: PreferencesModel;

  async getPreferences(): Promise<PreferencesModel> {
    if (!this.preferences) {
      this.preferences = await this.ajax<PreferencesModel>(
        "users/to-do-preferences"
      );
    }
    return this.preferences;
  }

  async updateNotificationsEnabled(enabled: boolean): Promise<void> {
    await this.ajaxExecute("users/to-do-notifications-enabled", {
      method: "put",
      body: json({
        toDoNotificationsEnabled: enabled,
      }),
    });

    this.preferences.notificationsEnabled = enabled;
  }
}
