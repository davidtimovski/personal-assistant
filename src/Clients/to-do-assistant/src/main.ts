import { Aurelia } from "aurelia-framework";
import { PLATFORM } from "aurelia-pal";
import { HttpClient } from "aurelia-fetch-client";
import { Backend, TCustomAttribute } from "aurelia-i18n";
import { UserManager, Log, WebStorageStateStore } from "oidc-client";

import { HttpProxy } from "../../shared/src/utils/httpProxy";
import { ErrorLogger } from "../../shared/src/services/errorLogger";
import { Language } from "../../shared/src/models/enums/language";

import * as environment from "../config/environment.json";
import { LocalStorage } from "utils/localStorage";
import { initialState } from "utils/state/state";

export function configure(aurelia: Aurelia) {
  const localStorage = new LocalStorage();
  localStorage.setLanguageFromUrl();
  const language = localStorage.getLanguage();

  const envConfig = JSON.parse(<any>environment);

  aurelia.use
    .standardConfiguration()
    .plugin(PLATFORM.moduleName("aurelia-animator-css"))
    .plugin(PLATFORM.moduleName("aurelia-validation"))
    .plugin(PLATFORM.moduleName("aurelia-store"), { initialState })
    .plugin(PLATFORM.moduleName("@appex/aurelia-dompurify"))
    .plugin(PLATFORM.moduleName("bcx-aurelia-reorderable-repeat"))
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

  // Configure services
  const container = aurelia.container;

  const httpClient = new HttpClient();
  httpClient.configure((config) => {
    config.withBaseUrl(`${envConfig.urls.api}/`).withDefaults({
      headers: {
        Accept: "application/json",
        "Accept-Language": language,
        "Content-Type": "application/json",
        "X-Requested-With": "Fetch",
      },
    });
  });
  container.registerInstance(HttpClient, httpClient);

  container.registerInstance(LocalStorage, localStorage);

  if (envConfig.debug) {
    Log.logger = console;
  }
  container.registerInstance(
    UserManager,
    new UserManager({
      authority: envConfig.urls.authority,
      client_id: "to-do-assistant",
      redirect_uri: `${envConfig.urls.host}/signin-oidc`,
      response_type: "code",
      scope: "openid email personal-assistant-api personal-assistant-gateway",
      post_logout_redirect_uri: envConfig.urls.host,
      userStore: new WebStorageStateStore({
        prefix: "oidc",
        store: window.localStorage,
      }),
    })
  );

  container.registerInstance(
    ErrorLogger,
    new ErrorLogger(container.get(HttpProxy), envConfig.urls.gateway, "ToDoAssistant")
  );

  container.registerInstance(BroadcastChannel, new BroadcastChannel("sw-version-updates"));

  return aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName("app")));
}
