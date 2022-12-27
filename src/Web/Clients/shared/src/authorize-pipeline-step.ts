import { autoinject } from "aurelia-framework";
import { NavigationInstruction, Next, PipelineStep, Redirect } from "aurelia-router";

import { AuthService } from "./services/authService";

@autoinject
export default class AuthorizeStep implements PipelineStep {
  constructor(private readonly authService: AuthService) {}

  public async run(navigationInstruction: NavigationInstruction, next: Next): Promise<any> {
    const instructions = navigationInstruction.getAllInstructions();
    const noAuth = instructions.some((instruction) => instruction.config.settings?.noAuth);

    if (!noAuth) {
      if (!this.authService.authenticated) {
        return next.cancel(new Redirect("login-callback"));
      }
    }

    return next();
  }
}
