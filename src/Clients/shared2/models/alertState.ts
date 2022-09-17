import { AlertStatus } from "./enums/alertEvents";

export class AlertState {
  constructor(
    public status: AlertStatus,
    public messageKey: string | null,
    public messages: string[]
  ) {}

  hide() {
    this.status = AlertStatus.Hidden;
    this.messageKey = null;
    this.messages = [];
  }

  showSuccess(messageKey: string) {
    this.status = AlertStatus.Success;
    this.messageKey = messageKey;
    this.messages = [];
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
