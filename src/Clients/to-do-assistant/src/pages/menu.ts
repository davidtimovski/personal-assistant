import { inject } from "aurelia-framework";
import { Router } from "aurelia-router";

import { NotificationsService } from "services/notificationsService";
import { ListsService } from "services/listsService";
import { AuthService } from "../../../shared/src/services/authService";
import * as environment from "../../config/environment.json";

@inject(Router, NotificationsService, ListsService, AuthService)
export class Menu {
  private version = "--";
  private unseenNotifications = 0;
  private pendingShareRequestCount = 0;
  private readonly personalAssistantUrl: string;
  private preferencesButtonIsLoading = false;
  private helpButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly notificationsService: NotificationsService,
    private readonly listsService: ListsService,
    private readonly authService: AuthService
  ) {
    this.personalAssistantUrl = JSON.parse(<any>environment).urls.authority;
  }

  activate() {
    caches.keys().then((cacheNames) => {
      if (cacheNames.length > 0) {
        this.version = cacheNames.sort().reverse()[0];
      }
    });

    this.notificationsService.getUnseenNotificationsCount().then((unseenNotifications) => {
      this.unseenNotifications = unseenNotifications;
    });

    this.listsService.getPendingShareRequestsCount().then((pendingShareRequestCount) => {
      this.pendingShareRequestCount = pendingShareRequestCount;
    });
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
