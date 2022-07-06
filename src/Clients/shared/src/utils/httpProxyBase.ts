import { inject } from "aurelia-framework";
import { HttpClient, RequestInit } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";

import { HttpError } from "../models/enums/httpError";
import { AlertEvents } from "../models/enums/alertEvents";
import { ValidationErrors } from "models/validationErrors";

@inject(AuthService, HttpClient, EventAggregator)
export class HttpProxyBase {
  protected readonly successCodes = [200, 201, 204];

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator
  ) {}

  protected async ajax<T>(uri: string, init?: RequestInit): Promise<T> {
    let response: Response;

    try {
      response = await this.httpClient.fetch(uri, init);
    } catch (e) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw e;
    }

    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);

      if (response.status === 404) {
        return null;
      }

      await this.HandleErrorCodes(response);
    }

    if (response.status === 204) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw new Error(`GET request to '${uri}' returned 204 No Content.`);
    }

    return <T>await response.json();
  }

  protected async ajaxBlob(uri: string, init?: RequestInit): Promise<Blob> {
    let response: Response;

    try {
      response = await this.httpClient.fetch(uri, init);
    } catch (e) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw e;
    }

    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);

      if (response.status === 404) {
        return null;
      }

      await this.HandleErrorCodes(response);
    }

    return response.blob();
  }

  protected async ajaxExecute(uri: string, init?: RequestInit): Promise<void> {
    let response: Response;

    try {
      response = await this.httpClient.fetch(uri, init);
    } catch (e) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw e;
    }

    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);

      await this.HandleErrorCodes(response);
    }
  }

  protected async ajaxUploadFile(uri: string, init?: RequestInit): Promise<string> {
    const response: Response = await this.httpClient.fetch(uri, init);

    if (!this.successCodes.includes(response.status)) {
      this.redirectIfUnauthorized(response.status);

      if (response.status === 404) {
        return null;
      }

      await this.HandleErrorCodes(response);
    }

    return <string>await response.json();
  }

  private redirectIfUnauthorized(statusCode: number) {
    if (statusCode === 401) {
      this.authService.signinRedirect();
      throw HttpError.Unauthorized;
    }
  }

  private async HandleErrorCodes(response: Response): Promise<void> {
    if (response.status === 404) {
      throw new Error("404 Not Found returned");
    } else if (response.status === 422) {
      let errors: any;

      try {
        errors = await response.json();
      } catch (e) {
        this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
        throw e;
      }

      if (errors.message === "Failed to fetch") {
        this.eventAggregator.publish(AlertEvents.ShowError, "failedToFetchError");
        throw new Error("Failed to fetch");
      }

      const modelErrors = new Array<string>();
      const errorFields = new Array<string>();
      for (let key in errors) {
        modelErrors.push(errors[key][0]);
        errorFields.push(key);
      }
      this.eventAggregator.publish(AlertEvents.ShowError, modelErrors);

      throw new ValidationErrors(errorFields);
    }

    this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
    throw new Error(`Error status code ${response.status} not handled`);
  }
}
