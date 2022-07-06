import { NavigationInstruction, Router, RouterConfiguration } from "aurelia-router";
import { inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";

import { AuthService } from "../../shared/src/services/authService";
import { ConnectionTracker } from "../../shared/src/utils/connectionTracker";
import AuthorizeStep from "../../shared/src/authorize-pipeline-step";
import { AuthEvents } from "../../shared/src/models/enums/authEvents";
import { AlertEvents } from "../../shared/src/models/enums/alertEvents";

import * as Actions from "utils/state/actions";
import { LocalStorage } from "utils/localStorage";
import { SignalRClient } from "utils/signalRClient";
import { ListsService } from "services/listsService";
import { AppEvents } from "models/appEvents";
import routes from "./routes";

@inject(
  AuthService,
  LocalStorage,
  ConnectionTracker,
  ListsService,
  SignalRClient,
  EventAggregator,
  BroadcastChannel,
  I18N
)
export class App {
  private isTouchDevice = false;
  router: Router;

  constructor(
    private readonly authService: AuthService,
    private readonly localStorage: LocalStorage,
    private readonly connTracker: ConnectionTracker,
    private readonly listsService: ListsService,
    private readonly signalRClient: SignalRClient,
    private readonly eventAggregator: EventAggregator,
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

    this.eventAggregator.subscribeOnce(AuthEvents.Authenticated, async () => {
      this.localStorage.initialize();

      await Actions.getLists(this.listsService);
      this.eventAggregator.publish(AppEvents.ListsChanged);

      this.signalRClient.initialize(this.authService.currentUser.access_token, this.authService.currentUserId);
    });

    this.authService.login();
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
