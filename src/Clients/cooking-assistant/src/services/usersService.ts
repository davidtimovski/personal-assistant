import { json } from "aurelia-fetch-client";
import { UsersServiceBase } from "../../../shared/src/services/usersServiceBase";
import { PreferencesModel } from "models/preferencesModel";

export class UsersService extends UsersServiceBase {
  private preferences: PreferencesModel;

  async getPreferences(): Promise<PreferencesModel> {
    if (!this.preferences) {
      this.preferences = await this.ajax<PreferencesModel>("users/cooking-preferences");
    }

    return this.preferences;
  }

  async updateNotificationsEnabled(enabled: boolean): Promise<void> {
    await this.ajaxExecute("users/cooking-notifications-enabled", {
      method: "put",
      body: json({
        cookingNotificationsEnabled: enabled,
      }),
    });

    this.preferences.notificationsEnabled = enabled;
  }

  async updateImperialSystem(imperialSystem: boolean): Promise<void> {
    await this.ajaxExecute("users/imperial-system", {
      method: "put",
      body: json({
        imperialSystem: imperialSystem,
      }),
    });

    this.preferences.imperialSystem = imperialSystem;
  }
}
