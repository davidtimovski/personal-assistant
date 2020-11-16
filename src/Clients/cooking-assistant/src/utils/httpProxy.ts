import { HttpProxyBase } from "../../../shared/src/utils/httpProxyBase";

export class HttpProxy extends HttpProxyBase {
  protected async ajaxUploadFile(uri: string, request: any): Promise<string> {
    //await this.SetAuthHeader();

    const response: Response = await this.httpClient.fetch(uri, request);

    if (!this.successCodes.includes(response.status)) {
      return await this.HandleErrorCodes(response);
    }

    return <string>await response.json();
  }
}
