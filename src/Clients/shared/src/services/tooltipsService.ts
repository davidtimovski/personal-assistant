import { json } from "aurelia-fetch-client";

import { HttpProxyBase } from "../utils/httpProxyBase";
import { Tooltip } from "../models/tooltip";

export class TooltipsService extends HttpProxyBase {
  async getAll(application: string): Promise<Array<Tooltip>> {
    const result = await this.ajax<Array<Tooltip>>(
      `tooltips/application/${application}`
    );

    return result;
  }

  async getByKey(key: string, application: string): Promise<Tooltip> {
    const result = await this.ajax<Tooltip>(`tooltips/key/${key}/${application}`);

    return result;
  }

  async toggleDismissed(key: string, application: string, isDismissed: boolean): Promise<void> {
    await this.ajaxExecute("tooltips", {
      method: "put",
      body: json({
        key: key,
        application: application,
        isDismissed: isDismissed,
      }),
    });
  }
}
