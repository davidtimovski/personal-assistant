import { ErrorLogger } from "./errorLogger";
import { HttpProxy } from "../services/httpProxy";
import { Tooltip } from "../models/tooltip";
import Variables from "$lib/variables";

export class TooltipsService {
  private readonly httpProxy = new HttpProxy();

  constructor(private readonly logger: ErrorLogger) {}

  async getAll(application: string): Promise<Array<Tooltip>> {
    try {
      const result = await this.httpProxy.ajax<Array<Tooltip>>(
        `${Variables.urls.api}/api/tooltips/application/${application}`
      );
      return result;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async getByKey(key: string, application: string): Promise<Tooltip> {
    try {
      const result = await this.httpProxy.ajax<Tooltip>(
        `${Variables.urls.api}/api/tooltips/key/${key}/${application}`
      );
      return result;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async toggleDismissed(
    key: string,
    application: string,
    isDismissed: boolean
  ): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tooltips`, {
        method: "put",
        body: window.JSON.stringify({
          key: key,
          application: application,
          isDismissed: isDismissed,
        }),
      });
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }
}
