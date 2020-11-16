import { inject, inlineView } from "aurelia-framework";
import { Router } from "aurelia-router";
import { UserManager, WebStorageStateStore } from "oidc-client";

import { AuthService } from "../../../../shared/src/services/authService";

@inlineView("<template><script></script></template>")
@inject(AuthService, Router)
export class SigninOidc {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  activate() {
    new UserManager({
      response_mode: "query",
      userStore: new WebStorageStateStore({
        prefix: "oidc",
        store: window.localStorage,
      }),
    })
      .signinRedirectCallback()
      .then(() => {
        this.authService.loginCallback().then(() => {
          this.router.navigateToRoute("dashboard");
        });
      })
      .catch((e) => {
        console.error(e);
      });
  }
}
