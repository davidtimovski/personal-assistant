import { HttpProxy, HttpProxyError } from "../services/httpProxy";
import { DateHelper } from "../utils/dateHelper";
import { ValidationErrors } from "../models/validationErrors";
import Variables from "$lib/variables";

export class ErrorLogger {
  private readonly httpProxy = new HttpProxy();

  constructor(private readonly application: string) {}

  async logError(error: any): Promise<void> {
    if (
      !navigator.onLine ||
      error instanceof HttpProxyError ||
      error instanceof ValidationErrors
    ) {
      return;
    }

    let message = "";
    let stackTrace: string | undefined;
    if (error instanceof Error) {
      message = error.message;
      stackTrace = error.stack;
    } else if (typeof error === "string") {
      message = error;
    } else {
      message = error.toString();
    }

    await this.httpProxy.ajaxExecute(
      `${Variables.urls.gateway}/client-logger/logs`,
      {
        method: "post",
        body: window.JSON.stringify({
          application: this.application,
          message: message,
          stackTrace: stackTrace,
          occurred: DateHelper.adjustTimeZone(new Date()),
        }),
      }
    );
  }

  release() {
    this.httpProxy.release();
  }
}
