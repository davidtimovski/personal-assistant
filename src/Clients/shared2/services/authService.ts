import createAuth0Client, { Auth0Client } from "@auth0/auth0-spa-js";

import { authInfo } from "$lib/stores";
import { AuthInfo } from "../models/authInfo";
import Variables from "$lib/variables";

export class AuthService {
  private client: Auth0Client | null = null;

  get initialized(): boolean {
    return this.client !== null;
  }

  async initialize() {
    this.client = await createAuth0Client({
      domain: Variables.auth0Domain,
      client_id: Variables.auth0ClientId,
      audience: Variables.urls.api,
      cacheLocation: "localstorage",
      redirect_uri: `${Variables.urls.host}/signin-oidc`,
    });
  }

  async authenticated() {
    if (!this.client) {
      throw new Error("Not initialized");
    }

    return await this.client.isAuthenticated();
  }

  async signinRedirect() {
    if (!this.client) {
      throw new Error("Not initialized");
    }

    await this.client.loginWithRedirect();
  }

  async handleRedirectCallback() {
    if (!this.client) {
      throw new Error("Not initialized");
    }

    await this.client.handleRedirectCallback();
  }

  async setToken() {
    if (!this.client) {
      throw new Error("Not initialized");
    }

    const token = await this.client.getTokenSilently();
    const profile = await this.client?.getUser();

    authInfo.set(new AuthInfo(token, profile));
  }

  async logout() {
    if (!this.client) {
      throw new Error("Not initialized");
    }

    await this.client.logout({
      returnTo: Variables.urls.account,
    });
  }
}
