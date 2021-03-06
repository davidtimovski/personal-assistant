import { inject } from "aurelia-framework";
import { HttpClient } from "aurelia-fetch-client";
import { EventAggregator } from "aurelia-event-aggregator";

import { AuthService } from "../../../shared/src/services/authService";

import { HttpError } from "../models/enums/httpError";
import { AlertEvents } from "./alertEvents";

@inject(AuthService, HttpClient, EventAggregator)
export class HttpProxyBase {
  protected readonly successCodes: Array<number> = [200, 201, 204];

  constructor(
    protected readonly authService: AuthService,
    protected readonly httpClient: HttpClient,
    protected readonly eventAggregator: EventAggregator
  ) {}

  protected async ajax<T>(uri: string, request?: any): Promise<T> {
    let response: Response;

    try {
      response = await this.httpClient.fetch(uri, request);
    } catch (error) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw [];
    }

    if (!this.successCodes.includes(response.status)) {
      if (await this.IsUnauthorized(response.status)) {
        return;
      }

      return await this.HandleErrorCodes(response);
    }

    if (response.status === 204) {
      return null;
    }

    return <T>await response.json();
  }

  protected async ajaxBlob(uri: string, request?: any): Promise<Blob> {
    let response: Response;

    try {
      response = await this.httpClient.fetch(uri, request);
    } catch (error) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw [];
    }

    if (!this.successCodes.includes(response.status)) {
      if (await this.IsUnauthorized(response.status)) {
        return;
      }

      return await this.HandleErrorCodes(response);
    }

    return response.blob();
  }

  protected async ajaxExecute(uri: string, request?: any): Promise<void> {
    let response: Response;

    try {
      response = await this.httpClient.fetch(uri, request);
    } catch (error) {
      this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
      throw [];
    }

    if (!this.successCodes.includes(response.status)) {
      if (await this.IsUnauthorized(response.status)) {
        return;
      }

      await this.HandleErrorCodes(response);
    }
  }

  protected async ajaxUploadFile(uri: string, request: any): Promise<string> {
    const response: Response = await this.httpClient.fetch(uri, request);

    if (!this.successCodes.includes(response.status)) {
      return await this.HandleErrorCodes(response);
    }

    return <string>await response.json();
  }

  private async IsUnauthorized(statusCode: number) {
    if (statusCode === 401) {
      this.authService.signinRedirect();
      throw HttpError.Unauthorized;
    }

    return false;
  }

  private async HandleErrorCodes(response: Response) {
    if (response.status === 404) {
      return null;
    } else if (response.status === 422) {
      let errors: any;

      try {
        errors = await response.json();
      } catch (_) {
        this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
        throw [];
      }

      if (errors.message === "Failed to fetch") {
        this.eventAggregator.publish(AlertEvents.ShowError, "failedToFetchError");
        throw [];
      }

      const modelErrors = new Array<string>();
      const errorFields = new Array<string>();
      for (let key in errors) {
        modelErrors.push(errors[key][0]);
        errorFields.push(key);
      }
      this.eventAggregator.publish(AlertEvents.ShowError, modelErrors);

      throw errorFields;
    }

    this.eventAggregator.publish(AlertEvents.ShowError, "unexpectedError");
    throw HttpError.Unexpected;
  }
}
