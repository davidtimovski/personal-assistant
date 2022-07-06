import { NavigationInstruction, RouterConfiguration, Router } from "aurelia-router";
import { inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";

import { AuthService } from "../../shared/src/services/authService";
import { CurrenciesService } from "../../shared/src/services/currenciesService";
import AuthorizeStep from "../../shared/src/authorize-pipeline-step";
import { AlertEvents } from "../../shared/src/models/enums/alertEvents";

import { SyncService } from "services/syncService";
import { LocalStorage } from "utils/localStorage";
import routes from "./routes";
import { AppEvents } from "models/appEvents";
import { AuthEvents } from "../../shared/src/models/enums/authEvents";

@inject(AuthService, EventAggregator, SyncService, LocalStorage, CurrenciesService, BroadcastChannel, I18N)
export class App {
  private isTouchDevice = false;
  router: Router;

  constructor(
    private readonly authService: AuthService,
    private readonly eventAggregator: EventAggregator,
    private readonly syncService: SyncService,
    private readonly localStorage: LocalStorage,
    private readonly currenciesService: CurrenciesService,
    private readonly broadcastChannel: BroadcastChannel,
    private readonly i18n: I18N
  ) {
    this.broadcastChannel.addEventListener("message", (event: MessageEvent) => {
      this.eventAggregator.publish(
        AlertEvents.ShowSuccess,
        this.i18n.tr("versionUpdatedTo", { version: event.data.version })
      );
    });

    this.isTouchDevice = "ontouchstart" in window;

    if ("serviceWorker" in navigator) {
      navigator.serviceWorker.register("/sw.js");
    }

    this.authService.login();
    this.localStorage.initialize();

    window.addEventListener("online", () => {
      this.sync();
    });
    this.eventAggregator.subscribe(AppEvents.Sync, () => {
      this.sync();
    });
    this.eventAggregator.subscribeOnce(AuthEvents.Authenticated, () => {
      this.sync();
    });
  }

  sync() {
    if (navigator.onLine) {
      this.eventAggregator.publish(AppEvents.SyncStarted);
      const syncPromises = new Array<Promise<any>>();

      const lastSynced = this.localStorage.lastSynced;
      const syncPromise = this.syncService.sync(lastSynced);
      syncPromises.push(syncPromise);
      syncPromise.then((lastSyncedServer: string) => {
        this.localStorage.setLastSynced(lastSyncedServer);
      });

      const ratesPromise = this.currenciesService.loadRates();
      syncPromises.push(ratesPromise);

      Promise.all(syncPromises).then(() => {
        this.eventAggregator.publish(AppEvents.SyncFinished);
      });
    }
  }

  configureRouter(config: RouterConfiguration, router: Router): void {
    this.router = router;

    config.options.root = "/";
    config.options.pushState = true;

    this.addLoginRoute(config);
    config.addAuthorizeStep(AuthorizeStep);

    config.map(routes);

    config.mapUnknownRoutes("pages/notFound");
  }

  private addLoginRoute(config: RouterConfiguration) {
    config.mapRoute({
      name: "loginCallback",
      navigationStrategy: (instruction: NavigationInstruction) => {
        const callbackHandler = async () => {
          await this.authService.signinRedirect();
        };

        const navigationInstruction = () => this.redirectAfterCallback(instruction, "/");

        return this.runHandlerAndCompleteNavigationInstruction(callbackHandler, navigationInstruction);
      },
      route: "login-callback",
    });
  }

  private redirectAfterCallback(instruction: NavigationInstruction, route: string) {
    window.history.pushState({}, "", route);
    instruction.queryString = "";
    instruction.config.redirect = route;
  }

  private async runHandlerAndCompleteNavigationInstruction(
    callbackHandler: () => Promise<any>,
    navigationInstruction: () => void
  ): Promise<any> {
    try {
      await callbackHandler();
      navigationInstruction();
    } catch (e) {
      navigationInstruction();
      throw e;
    }
  }
}
