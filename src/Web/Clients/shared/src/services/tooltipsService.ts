import { autoinject } from "aurelia-framework";
import { json } from "aurelia-fetch-client";

import { ErrorLogger } from "./errorLogger";
import { HttpProxy } from "../utils/httpProxy";
import { Tooltip } from "../models/tooltip";

@autoinject
export class TooltipsService {
  constructor(private readonly httpProxy: HttpProxy, private readonly logger: ErrorLogger) {}

  async getAll(application: string): Promise<Array<Tooltip>> {
    try {
      const result = await this.httpProxy.ajax<Array<Tooltip>>(`api/tooltips/application/${application}`);
      return result;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async getByKey(key: string, application: string): Promise<Tooltip> {
    try {
      const result = await this.httpProxy.ajax<Tooltip>(`api/tooltips/key/${key}/${application}`);
      return result;
    } catch (e) {
      this.logger.logError(e);
      throw e;
    }
  }

  async toggleDismissed(key: string, application: string, isDismissed: boolean): Promise<void> {
    try {
      await this.httpProxy.ajaxExecute("api/tooltips", {
        method: "put",
        body: json({
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
