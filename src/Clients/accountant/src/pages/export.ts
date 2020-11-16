import { inject, computedFrom } from "aurelia-framework";
import { Router } from "aurelia-router";
import { TransactionsService } from "services/transactionsService";
import { I18N } from "aurelia-i18n";
import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { DateHelper } from "../../../shared/src/utils/dateHelper";

@inject(Router, TransactionsService, I18N, ConnectionTracker)
export class Export {
  private exportButtonIsLoading = false;

  constructor(
    private readonly router: Router,
    private readonly transactionsService: TransactionsService,
    private readonly i18n: I18N,
    private readonly connTracker: ConnectionTracker
  ) {}

  @computedFrom("connTracker.isOnline", "exportButtonIsLoading")
  get canExport() {
    return this.connTracker.isOnline && !this.exportButtonIsLoading;
  }

  async export() {
    if (!this.canExport) {
      return;
    }

    this.exportButtonIsLoading = true;

    const fileId = this.generateGuid();
    const fileName = await this.transactionsService.export(fileId);
    const date = DateHelper.format(new Date());

    const a = document.createElement("a");
    a.style.display = "none";
    document.body.appendChild(a);
    a.href = window.URL.createObjectURL(fileName);
    a.setAttribute(
      "download",
      `${this.i18n.tr("export.transactions")}-${date}.csv`
    );
    a.click();

    window.URL.revokeObjectURL(a.href);
    document.body.removeChild(a);

    this.transactionsService.deleteExportedFile(fileId);

    this.router.navigateToRoute("dashboard");
  }

  generateGuid(): string {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
      const random = (Math.random() * 16) | 0,
        v = c == "x" ? random : (random & 0x3) | 0x8;
      return v.toString(16);
    });
  }
}
