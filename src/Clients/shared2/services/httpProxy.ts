import type { User } from "oidc-client";

import { ValidationErrors } from "../models/validationErrors";
import { loggedInUser } from "$lib/stores";

export class HttpProxy {
  private readonly successCodes = [200, 201, 204];
  private user: User | null = null;

  constructor() {
    loggedInUser.subscribe((x) => (this.user = x));
  }

  async ajax<T>(uri: string, init?: RequestInit): Promise<T> {
    init = this.setHeaders(init);

    let response: Response;

    try {
      response = await window.fetch(uri, init);
    } catch (e) {
      //this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw e;
    }

    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);

      if (response.status === 404) {
        throw new Error("Not found");
      }

      await this.handleErrorCodes(response);
    }

    if (response.status === 204) {
      //this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw new Error(`GET request to '${uri}' returned 204 No Content`);
    }

    return <T>await response.json();
  }

  async ajaxBlob(uri: string, init?: RequestInit): Promise<Blob> {
    init = this.setHeaders(init);

    let response: Response;

    try {
      response = await window.fetch(uri, init);
    } catch (e) {
      //this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw e;
    }

    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);

      if (response.status === 404) {
        throw new Error("Not found");
      }

      await this.handleErrorCodes(response);
    }

    return response.blob();
  }

  async ajaxExecute(uri: string, init?: RequestInit): Promise<void> {
    init = this.setHeaders(init);

    let response: Response;
    try {
      response = await window.fetch(uri, init);
    } catch (e) {
      //this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw e;
    }
    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);
      await this.handleErrorCodes(response);
    }
  }

  async ajaxUploadFile(
    uri: string,
    init?: RequestInit
  ): Promise<string | null> {
    init = this.setHeaders(init);

    const response: Response = await window.fetch(uri, init);

    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);

      if (response.status === 404) {
        return null;
      }

      await this.handleErrorCodes(response);
    }

    return <string>await response.json();
  }

  private setHeaders(init?: RequestInit): RequestInit {
    const headers = {
      Accept: "application/json",
      Authorization: `Bearer ${this.user?.access_token}`,
      //"Accept-Language": language,
      "Content-Type": "application/json",
      "X-Requested-With": "Fetch",
    };

    if (init) {
      init.headers = headers;
    } else {
      init = {
        headers: headers,
      };
    }

    return init;
  }

  private redirectIfUnauthorized(statusCode: number) {
    if (statusCode === 401) {
      // TODO
      //this.authService.signinRedirect();
      //throw HttpError.Unauthorized;
    }
  }

  private async handleErrorCodes(response: Response): Promise<void> {
    if (response.status === 404) {
      throw new Error("404 Not Found returned");
    } else if (response.status === 422) {
      let errors: any;
      try {
        errors = await response.json();
      } catch (e) {
        //this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
        throw e;
      }
      if (errors.message === "Failed to fetch") {
        //this.eventAggregator.publish(AlertEvents.ShowError, "failedToFetchError");
        throw new Error("Failed to fetch");
      }
      const modelErrors = new Array<string>();
      const errorFields = new Array<string>();
      for (let key in errors) {
        modelErrors.push(errors[key][0]);
        errorFields.push(key);
      }
      //this.eventAggregator.publish(AlertEvents.ShowError, modelErrors);
      throw new ValidationErrors(errorFields);
    }
    //this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
    throw new Error(`Error status code ${response.status} not handled`);
  }
}
