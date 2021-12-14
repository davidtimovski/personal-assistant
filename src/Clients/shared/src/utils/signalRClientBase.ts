import * as signalR from "@microsoft/signalr";

export class SignalRClientBase {
  protected connection: signalR.HubConnection;

  async connect(url: string, accessToken: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => accessToken })
      .build();

    return this.connection.start();
  }

  async disconnect() {
    if (!this.connection || this.connection.state === signalR.HubConnectionState.Disconnected) {
      return;
    }
    return this.connection.stop();
  }

  get connected() {
    return this.connection && this.connection.state === signalR.HubConnectionState.Connected;
  }

  on(methodName: string, callback: (...args: any[]) => void) {
    return this.connection.on(methodName, callback);
  }
}
