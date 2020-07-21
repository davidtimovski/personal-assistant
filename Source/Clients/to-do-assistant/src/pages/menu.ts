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
  private personalAssistantUrl: string;
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

    this.notificationsService
      .getUnseenNotificationsCount()
      .then((unseenNotifications) => {
        this.unseenNotifications = unseenNotifications;
      });

    this.listsService
      .getPendingShareRequestsCount()
      .then((pendingShareRequestCount) => {
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
    // Delete indexedDB, cache, and some localStorage data before log out
    const deleteContextRequest = window.indexedDB.deleteDatabase("IDBContext");

    deleteContextRequest.onsuccess = () => {
      const deleteDbNamesRequest = window.indexedDB.deleteDatabase("__dbnames");

      deleteDbNamesRequest.onsuccess = async () => {
        window.localStorage.setItem("dataLastLoad", "1970-01-01T00:00:00.000Z");
        window.localStorage.setItem(
          "profileImageUriLastLoad",
          "1970-01-01T00:00:00.000Z"
        );

        await this.authService.logout();
      };
    };
  }
}
