import pkg from "oidc-client";
const { Log, UserManager, WebStorageStateStore } = pkg;
import Variables from "$lib/variables";
import { loggedInUser } from "$lib/stores";

export class AuthService {
  private userManager: any = null;

  constructor(client: string, window: Window) {
    if (Variables.debug) {
      Log.logger = console;
    }

    this.userManager = new UserManager({
      authority: Variables.urls.authority,
      client_id: client,
      redirect_uri: `${Variables.urls.host}/signin-oidc`,
      response_type: "code",
      scope: "openid email personal-assistant-api personal-assistant-gateway",
      post_logout_redirect_uri: Variables.urls.host,
      userStore: new WebStorageStateStore({
        prefix: "oidc",
        store: window.localStorage,
      }),
    });
  }

  async loginCallback() {
    if (!this.userManager) {
      return;
    }

    let user = await this.userManager.getUser();
    if (user) {
      loggedInUser.set(user);
    } else {
      await this.userManager.clearStaleState();

      try {
        user = await this.userManager.signinSilent();
        loggedInUser.set(user);
      } catch {
        await this.userManager.signinRedirect();
        return;
      }
    }
  }

  async login() {
    if (!window.location.href.includes("signin-oidc")) {
      await this.loginCallback();
    }
  }

  async signinRedirect() {
    if (!this.userManager) {
      return;
    }

    await this.userManager.signinRedirect();
  }

  async logout() {
    if (!this.userManager) {
      return;
    }

    await this.userManager.clearStaleState();
    await this.userManager.signoutRedirect();
  }
}
