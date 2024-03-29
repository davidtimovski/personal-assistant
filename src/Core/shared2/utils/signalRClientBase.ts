import * as signalR from "@microsoft/signalr";

export class SignalRClientBase {
  protected connection: signalR.HubConnection | null = null;

  connect(url: string, accessToken: string, debug: boolean): Promise<void> {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => accessToken })
      .configureLogging(
        debug ? signalR.LogLevel.Information : signalR.LogLevel.Error
      )
      .withAutomaticReconnect()
      .build();

    return this.connection.start();
  }

  async disconnect() {
    if (
      !this.connection ||
      this.connection.state === signalR.HubConnectionState.Disconnected
    ) {
      return;
    }
    return this.connection.stop();
  }

  get connected() {
    return (
      this.connection &&
      this.connection.state === signalR.HubConnectionState.Connected
    );
  }

  on(methodName: string, callback: (...args: any[]) => void) {
    return (<signalR.HubConnection>this.connection).on(methodName, callback);
  }
}
