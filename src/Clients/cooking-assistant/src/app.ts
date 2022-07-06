import { inject } from "aurelia-framework";
import { NavigationInstruction, RouterConfiguration, Router } from "aurelia-router";
import { EventAggregator } from "aurelia-event-aggregator";
import { I18N } from "aurelia-i18n";

import { AuthService } from "../../shared/src/services/authService";
import { AlertEvents } from "../../shared/src/models/enums/alertEvents";
import { AuthEvents } from "../../shared/src/models/enums/authEvents";
import { LocalStorageCurrencies } from "../../shared/src/utils/localStorageCurrencies";
import { CurrenciesService } from "../../shared/src/services/currenciesService";
import { ConnectionTracker } from "../../shared/src/utils/connectionTracker";
import AuthorizeStep from "../../shared/src/authorize-pipeline-step";

import * as Actions from "utils/state/actions";
import { RecipesService } from "services/recipesService";
import routes from "./routes";
import { AppEvents } from "models/appEvents";

@inject(
  AuthService,
  LocalStorageCurrencies,
  CurrenciesService,
  EventAggregator,
  ConnectionTracker,
  RecipesService,
  BroadcastChannel,
  I18N
)
export class App {
  private isTouchDevice = false;
  router: Router;

  constructor(
    private readonly authService: AuthService,
    private readonly localStorage: LocalStorageCurrencies,
    private readonly currenciesService: CurrenciesService,
    private readonly eventAggregator: EventAggregator,
    private readonly connTracker: ConnectionTracker,
    private readonly recipesService: RecipesService,
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

    this.eventAggregator.subscribeOnce(AuthEvents.Authenticated, () => {
      this.currenciesService.loadRates().then(() => {
        this.localStorage.initialize();
      });

      Actions.getRecipes(this.recipesService).then(() => {
        this.eventAggregator.publish(AppEvents.RecipesLoaded);
      });
    });
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
