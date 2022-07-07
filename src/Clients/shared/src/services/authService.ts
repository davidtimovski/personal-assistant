import { autoinject } from "aurelia-framework";
import { User, UserManager } from "oidc-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthEvents } from "../models/enums/authEvents";

@autoinject
export class AuthService {
  private user: User;

  constructor(
    private readonly userManager: UserManager,
    private readonly httpClient: HttpClient,
    private readonly eventAggregator: EventAggregator
  ) {}

  get currentUser(): User {
    return this.user;
  }

  get currentUserId(): number {
    if (!this.user) {
      return null;
    }

    return parseInt(this.user.profile.sub, 10);
  }

  get authenticated(): boolean {
    return !!this.user;
  }

  async loginCallback() {
    this.user = await this.userManager.getUser();

    if (!this.user) {
      await this.userManager.clearStaleState();

      try {
        this.user = await this.userManager.signinSilent();
      } catch {
        await this.userManager.signinRedirect();
        return;
      }
    }

    this.httpClient.defaults.headers["Authorization"] = `Bearer ${this.user.access_token}`;

    this.eventAggregator.publish(AuthEvents.Authenticated);
  }

  async login() {
    if (!window.location.href.includes("signin-oidc")) {
      await this.loginCallback();
    }
  }

  async signinRedirect() {
    await this.userManager.signinRedirect();
  }

  async logout() {
    await this.userManager.clearStaleState();
    await this.userManager.signoutRedirect();
  }
}
