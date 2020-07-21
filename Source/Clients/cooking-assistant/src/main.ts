/// <reference types="aurelia-loader-webpack/src/webpack-hot-interface"/>
import { Aurelia } from "aurelia-framework";
import { PLATFORM } from "aurelia-pal";
import { HttpClient } from "aurelia-fetch-client";
import { UserManager, Log, WebStorageStateStore } from "oidc-client";
import { Backend, TCustomAttribute } from "aurelia-i18n";

import * as environment from "../config/environment.json";
import { initialState } from "utils/state/state";
import { Language } from "../../shared/src/models/enums/language";
import { LocalStorageCurrencies } from "../../shared/src/utils/localStorageCurrencies";

export function configure(aurelia: Aurelia) {
  const localStorage = new LocalStorageCurrencies();
  localStorage.setLanguageFromUrl();
  const language = localStorage.getLanguage();

  const envConfig = JSON.parse(<any>environment);

  aurelia.use
    .standardConfiguration()
    .plugin(PLATFORM.moduleName("aurelia-animator-css"))
    .plugin(PLATFORM.moduleName("aurelia-validation"))
    .plugin(PLATFORM.moduleName("aurelia-store"), { initialState })
    .plugin(PLATFORM.moduleName("aurelia-i18n"), (instance) => {
      const aliases = ["t", "i18n"];

      TCustomAttribute.configureAliases(aliases);

      instance.i18next.use(Backend.with(aurelia.loader));

      return instance.setup({
        backend: {
          loadPath: "locales/{{lng}}/{{ns}}.json",
        },
        attributes: aliases,
        lng: language,
        whitelist: [Language.English, Language.Macedonian],
        debug: false,
      });
    })
    .developmentLogging(envConfig.debug ? "debug" : "warn")
    .feature(PLATFORM.moduleName("resources/index"));

  // Register singletons
  const container = aurelia.container;

  const httpClient = new HttpClient();
  httpClient.configure((config) => {
    config.withBaseUrl(`${envConfig.urls.api}/api/`).withDefaults({
      headers: {
        Accept: "application/json",
        "Accept-Language": language,
        "X-Requested-With": "Fetch",
      },
    });
  });
  container.registerInstance(HttpClient, httpClient);

  container.registerInstance(LocalStorageCurrencies, localStorage);

  if (envConfig.debug) {
    Log.logger = console;
  }
  container.registerInstance(
    UserManager,
    new UserManager({
      authority: envConfig.urls.authority,
      client_id: "cooking-assistant",
      redirect_uri: `${envConfig.urls.host}/signin-oidc`,
      response_type: "code",
      scope: "openid email personal-assistant-api",
      post_logout_redirect_uri: envConfig.urls.host,
      userStore: new WebStorageStateStore({
        prefix: "oidc",
        store: window.localStorage,
      }),
    })
  );

  container.registerInstance(
    BroadcastChannel,
    new BroadcastChannel("sw-version-updates")
  );

  return aurelia
    .start()
    .then(() => aurelia.setRoot(PLATFORM.moduleName("app")));
}
