import { Auth0Client } from "@auth0/auth0-spa-js";

import { authInfo } from "$lib/stores";
import { AuthInfo } from "../models/authInfo";
import Variables from "$lib/variables";

/** Handles authentication using Auth0. */
export class AuthService {
  private readonly client: Auth0Client;

  constructor() {
    this.client = new Auth0Client({
      domain: Variables.auth0Domain,
      clientId: Variables.auth0ClientId,
      useRefreshTokens: true,
      cacheLocation: "localstorage",
      authorizationParams: {
        audience: Variables.urls.gateway,
        redirect_uri: `${Variables.urls.host}/signin-oidc`,
      },
    });
  }

  async silentLogin() {
    try {
      const token = await this.client.getTokenSilently();
      const profile = await this.client?.getUser();

      authInfo.set(new AuthInfo(token, profile));
    } catch (error) {
      if (error.error !== "login_required") {
        throw error;
      }
    }
  }

  authenticated() {
    return this.client.isAuthenticated();
  }

  async signinRedirect() {
    await this.client.loginWithRedirect();
  }

  async handleRedirectCallback() {
    await this.client.handleRedirectCallback();
  }

  async logout() {
    await this.client.logout({
      logoutParams: {
        returnTo: Variables.urls.account,
      },
    });
  }
}
