import { AlertStatus } from "./enums/alertEvents";

export class AlertState {
  constructor(
    public status: AlertStatus,
    public messageKey: string | null,
    public messages: string[],
    public messageData: any = {}
  ) {}

  hide() {
    this.status = AlertStatus.Hidden;
    this.messageKey = null;
    this.messages = [];
  }

  showSuccess(messageKey: string, messageData: any = {}) {
    this.status = AlertStatus.Success;
    this.messageKey = messageKey;
    this.messages = [];
    this.messageData = messageData;
  }

  showError(messageKey: string) {
    this.status = AlertStatus.Error;
    this.messageKey = messageKey;
    this.messages = [];
  }

  showErrors(messages: string[]) {
    this.status = AlertStatus.Error;
    this.messageKey = null;
    this.messages = messages;
  }

  get hidden() {
    return this.status === AlertStatus.Hidden;
  }
}
