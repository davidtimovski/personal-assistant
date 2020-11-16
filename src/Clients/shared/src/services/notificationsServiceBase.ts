import { json } from "aurelia-fetch-client";
import { HttpProxyBase } from "../utils/httpProxyBase";

export class NotificationsServiceBase extends HttpProxyBase {
  async createSubscription(
    application: string,
    subscription: PushSubscription
  ): Promise<void> {
    await this.ajaxExecute("pushsubscriptions", {
      method: "post",
      body: json({
        application: application,
        subscription: subscription,
      }),
    });
  }

  static getApplicationServerKey(vapidPublicKey: string): Uint8Array {
    return NotificationsServiceBase.urlBase64ToUint8Array(vapidPublicKey);
  }

  private static urlBase64ToUint8Array(base64String: string): Uint8Array {
    const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding)
      .replace(/\-/g, "+")
      .replace(/_/g, "/");

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
  }
}
