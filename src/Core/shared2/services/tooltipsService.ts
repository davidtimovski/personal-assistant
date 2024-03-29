import { ErrorLogger } from "./errorLogger";
import { HttpProxy } from "../services/httpProxy";
import type { Tooltip } from "../models/tooltip";
import Variables from "$lib/variables";

export class TooltipsService {
  private readonly httpProxy = new HttpProxy();
  private readonly logger: ErrorLogger;

  constructor(private readonly application: string) {
    this.logger = new ErrorLogger(application);
  }

  async getAll(): Promise<Array<Tooltip>> {
    try {
      const result = await this.httpProxy.ajax<Array<Tooltip>>(
        `${Variables.urls.gateway}/core/api/tooltips/application/${this.application}`
      );
      return result;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async getByKey(key: string): Promise<Tooltip> {
    try {
      const result = await this.httpProxy.ajax<Tooltip>(
        `${Variables.urls.gateway}/core/api/tooltips/key/${key}/${this.application}`
      );
      return result;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async toggleDismissed(key: string, isDismissed: boolean): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute(
        `${Variables.urls.gateway}/core/api/tooltips`,
        {
          method: "put",
          body: window.JSON.stringify({
            key: key,
            application: this.application,
            isDismissed: isDismissed,
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
