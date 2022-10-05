import type { Unsubscriber } from "svelte/store";
import type { User } from "oidc-client";

import { AuthService } from "./authService";
import { ValidationErrors } from "../models/validationErrors";
import { HttpError } from "../models/enums/httpError";
import { alertState, loggedInUser } from "$lib/stores";

export class HttpProxy {
  private readonly authService: AuthService;
  private readonly successCodes = [200, 201, 204];
  private user: User | null = null;
  private loggedInUserUnsub: Unsubscriber;

  constructor(client: string) {
    this.loggedInUserUnsub = loggedInUser.subscribe((x) => (this.user = x));
    this.authService = new AuthService(client);
  }

  async ajax<T>(uri: string, init?: RequestInit): Promise<T> {
    init = this.setHeaders(init);

    let response: Response;

    try {
      response = await window.fetch(uri, init);
    } catch (e) {
      alertState.update((x) => {
        x.showError("unexpectedError");
        return x;
      });
      throw e;
    }

    if (!this.successCodes.includes(response.status)) {
      await this.redirectIfUnauthorized(response.status);

      if (response.status === 404) {
        throw new Error("Not found");
      }

      await this.handleErrorCodes(response);
    }

    if (response.status === 204) {
      alertState.update((x) => {
        x.showError("unexpectedError");
        return x;
      });
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
      alertState.update((x) => {
        x.showError("unexpectedError");
        return x;
      });
      throw e;
    }

    if (!this.successCodes.includes(response.status)) {
      await this.redirectIfUnauthorized(response.status);

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
      alertState.update((x) => {
        x.showError("unexpectedError");
        return x;
      });
      throw e;
    }
    if (!this.successCodes.includes(response.status)) {
      await this.redirectIfUnauthorized(response.status);
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
      await this.redirectIfUnauthorized(response.status);
      await this.handleErrorCodes(response);
    }

    return <string>await response.json();
  }

  release() {
    this.loggedInUserUnsub();
  }

  private setHeaders(init?: RequestInit): RequestInit {
    const headers = {
      Accept: "application/json",
      Authorization: `Bearer ${this.user?.access_token}`,
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

  private async redirectIfUnauthorized(statusCode: number) {
    if (statusCode === 401) {
      await this.authService.signinRedirect();
      throw HttpError.Unauthorized;
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
        alertState.update((x) => {
          x.showError("unexpectedError");
          return x;
        });
        throw e;
      }

      if (errors.message === "Failed to fetch") {
        alertState.update((x) => {
          x.showError("failedToFetchError");
          return x;
        });
        throw HttpError.FailedToFetch;
      }

      const errorFields = new Array<string>();
      for (let key in errors) {
        errorFields.push(key);
      }
      alertState.update((x) => {
        x.showErrors(errorFields);
        return x;
      });

      throw new ValidationErrors(errorFields);
    }

    alertState.update((x) => {
      x.showError("unexpectedError");
      return x;
    });

    throw new Error(`Error status code ${response.status} not handled`);
  }
}
