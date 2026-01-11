import { HttpProxy } from "../services/httpProxy";
import { ErrorLogger } from "./errorLogger";
import Variables from "$lib/variables";

export class NotificationsServiceBase {
  protected readonly httpProxy = new HttpProxy();
  protected readonly logger: ErrorLogger;

  constructor(application: string) {
    this.logger = new ErrorLogger(application);
  }

  async createSubscription(
    application: string,
    subscription: PushSubscription
  ): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(
        `${Variables.urls.gateway}/core/api/pushsubscriptions`,
        {
          method: "post",
          body: window.JSON.stringify({
            application: application,
            subscription: subscription,
          }),
        }
      );
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  release() {
    this.httpProxy.release();
  }
}
