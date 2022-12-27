import { json } from "aurelia-fetch-client";

import { HttpProxy } from "../utils/httpProxy";
import { DateHelper } from "../utils/dateHelper";
import { HttpError } from "../models/enums/httpError";
import { ValidationErrors } from "../models/validationErrors";

export class ErrorLogger {
  constructor(
    private readonly httpProxy: HttpProxy,
    private readonly gatewayUrl: string,
    private readonly application: string
  ) {}

  async logError(error: any): Promise<void> {
    if (!navigator.onLine || error === HttpError.Unauthorized || error instanceof ValidationErrors) {
      return;
    }

    let message = "",
      stackTrace = null;

    if (error instanceof Error) {
      message = error.message;
      stackTrace = error.stack;
    } else if (typeof error === "string") {
      message = error;
    } else {
      message = error.toString();
    }

    await this.httpProxy.ajaxExecute(`${this.gatewayUrl}/clientlogger/logs`, {
      method: "post",
      body: json({
        application: this.application,
        message: message,
        stackTrace: stackTrace,
        occurred: DateHelper.adjustForTimeZone(new Date()),
      }),
    });
  }
}
