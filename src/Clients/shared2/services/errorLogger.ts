import { HttpProxy } from "../services/httpProxy";
import { DateHelper } from "../utils/dateHelper";
import { HttpError } from "../models/enums/httpError";
import { ValidationErrors } from "../models/validationErrors";
import Variables from "$lib/variables";

export class ErrorLogger {
  private readonly httpProxy = new HttpProxy();

  constructor(private readonly application: string) {}

  async logError(error: any): Promise<void> {
    if (
      !navigator.onLine ||
      error === HttpError.Unauthorized ||
      error instanceof ValidationErrors
    ) {
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

    await this.httpProxy.ajaxExecute(
      `${Variables.urls.gateway}/clientlogger/logs`,
      {
        method: "post",
        body: window.JSON.stringify({
          application: this.application,
          message: message,
          stackTrace: stackTrace,
          occurred: DateHelper.adjustForTimeZone(new Date()),
        }),
      }
    );
  }
}