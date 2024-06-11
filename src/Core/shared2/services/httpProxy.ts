import type { Unsubscriber } from "svelte/store";

import { AuthService } from "./authService";
import { ValidationErrors } from "../models/validationErrors";
import { alertState, authInfo } from "$lib/stores";

export class HttpProxyError extends Error {
  constructor(message: string) {
    super(message);
    Object.setPrototypeOf(this, HttpProxyError.prototype);
  }
}

export class HttpProxy {
  private readonly authService: AuthService;
  private readonly successCodes = [200, 201, 204];
  private token: string | null = null;
  private authInfoUnsub: Unsubscriber;

  constructor() {
    this.authInfoUnsub = authInfo.subscribe((x) => {
      if (!x) {
        return;
      }

      this.token = x.token;
    });
    this.authService = new AuthService();
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
      throw new HttpProxyError(e.message);
    }

    if (!this.successCodes.includes(response.status)) {
      await this.handleErrorCodes(response);
    }

    if (response.status === 204) {
      alertState.update((x) => {
        x.showError("unexpectedError");
        return x;
      });
      throw new HttpProxyError(
        `GET request to '${uri}' returned 204 No Content`
      );
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
      throw new HttpProxyError(e.message);
    }

    if (!this.successCodes.includes(response.status)) {
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
      throw new HttpProxyError(e.message);
    }
    if (!this.successCodes.includes(response.status)) {
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
      await this.handleErrorCodes(response);
    }

    return <string>await response.json();
  }

  release() {
    this.authInfoUnsub();
  }

  private setHeaders(init?: RequestInit): RequestInit {
    const headers: HeadersInit = {
      Accept: "application/json",
      Authorization: `Bearer ${this.token}`,
      "X-Requested-With": "Fetch",
    };

    // Skip header for file uploads
    if (!(init?.body instanceof FormData)) {
      headers["Content-Type"] = "application/json";
    }

    if (init) {
      init.headers = headers;
    } else {
      init = {
        headers: headers,
      };
    }

    return init;
  }

  private async handleErrorCodes(response: Response): Promise<void> {
    if (response.status === 401) {
      await this.authService.signinRedirect();
      throw new HttpProxyError("unauthorized");
    } else if (response.status === 404) {
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
        throw new HttpProxyError(e.message);
      }

      if (errors.message === "failedToFetch") {
        alertState.update((x) => {
          x.showError("failedToFetchError");
          return x;
        });
        throw new HttpProxyError("failedToFetch");
      }

      const errorFields = new Array<string>();
      const errorMessages = new Array<string>();
      for (let key in errors) {
        errorFields.push(key);

        for (let message of errors[key]) {
          errorMessages.push(message);
        }
      }
      alertState.update((x) => {
        x.showErrors(errorMessages);
        return x;
      });

      throw new ValidationErrors(errorFields);
    }

    alertState.update((x) => {
      x.showError("unexpectedError");
      return x;
    });

    throw new HttpProxyError(
      `Error status code ${response.status} not handled`
    );
  }
}
