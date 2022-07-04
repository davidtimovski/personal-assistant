import { inject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { AuthService } from "./authService";
import { DateHelper } from "../utils/dateHelper";
import { HttpError } from "../models/enums/httpError";

@inject(AuthService)
export class ErrorLogger {
  constructor(private readonly baseUrl: string, private readonly authService: AuthService) {}

  async logError(error: any): Promise<void> {
    if (!navigator.onLine || error === HttpError.Unauthorized) {
      return;
    }

    let message = "",
      stackTrace = "";

    if (error instanceof Error) {
      message = error.message;
      stackTrace = error.stack;
    } else if (typeof error === "string") {
      message = error;
    }

    await window.fetch(`${this.baseUrl}/logs`, {
      method: "post",
      headers: {
        "Content-Type": "application/json",
        "X-Requested-With": "Fetch",
      },
      body: json({
        userId: this.authService.currentUser.profile.sub,
        message: message,
        stackTrace: stackTrace,
        occurred: DateHelper.adjustForTimeZone(new Date()),
      }),
    });
  }
}
