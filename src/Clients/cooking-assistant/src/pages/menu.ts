import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";

import * as environment from "../../config/environment.json";
import { RecipesService } from "services/recipesService";
import { AuthService } from "../../../shared/src/services/authService";

@inject(Router, RecipesService, AuthService)
export class Menu {
  private version = "--";
  private pendingShareRequestCount = 0;
  private pendingSendRequestCount = 0;
  private readonly personalAssistantUrl: string;
  private dietaryProfileButtonIsLoading = false;
  private inboxButtonIsLoading = false;
  private preferencesButtonIsLoading = false;
  private helpButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly recipesService: RecipesService,
    private readonly authService: AuthService
  ) {
    this.personalAssistantUrl = JSON.parse(<any>environment).urls.authority;
  }

  activate() {
    this.recipesService.getPendingShareRequestsCount().then((pendingShareRequestCount) => {
      this.pendingShareRequestCount = pendingShareRequestCount;
    });

    this.recipesService.getPendingSendRequestsCount().then((pendingSendRequestCount) => {
      this.pendingSendRequestCount = pendingSendRequestCount;
    });
  }

  async attached() {
    const cacheNames = await caches.keys();
    if (cacheNames.length > 0) {
      this.version = cacheNames.sort().reverse()[0];
    }
  }

  goToDietaryProfile() {
    this.dietaryProfileButtonIsLoading = true;
    this.router.navigateToRoute("dietaryProfile");
  }

  goToInbox() {
    this.inboxButtonIsLoading = true;
    this.router.navigateToRoute("inbox");
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
    window.localStorage.removeItem("profileImageUriLastLoad");
    await this.authService.logout();
  }
}
