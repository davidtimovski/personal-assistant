import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";

import { AuthService } from "../../../shared/src/services/authService";
import * as environment from "../../config/environment.json";

@inject(Router, AuthService)
export class Menu {
  private reportsDrawerIsOpen = false;
  private version = "--";
  private personalAssistantUrl: string;
  private preferencesButtonIsLoading = false;
  private helpButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) {
    this.personalAssistantUrl = JSON.parse(<any>environment).urls.authority;
  }

  attached() {
    caches.keys().then((cacheNames) => {
      if (cacheNames.length > 0) {
        this.version = cacheNames.sort().reverse()[0];
      }
    });
  }

  toggleReportsDrawer() {
    this.reportsDrawerIsOpen = !this.reportsDrawerIsOpen;
  }

  goToPreferences() {
    this.preferencesButtonIsLoading = true;
    this.router.navigateToRoute("preferences");
  }

  goToHelp() {
    this.helpButtonIsLoading = true;
    this.router.navigateToRoute("help");
  }

  async logOut() {
    await this.authService.logout();
  }
}
