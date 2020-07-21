import { inject } from "aurelia-framework";
import { User, UserManager } from "oidc-client";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

@inject(UserManager, HttpClient, EventAggregator)
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

    this.httpClient.defaults.headers[
      "Authorization"
    ] = `Bearer ${this.user.access_token}`;

    this.eventAggregator.publish("authenticated");
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
