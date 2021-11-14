import { inject, computedFrom } from "aurelia-framework";
import { ConnectionTracker } from "../../../shared/src/utils/connectionTracker";
import { SyncService } from "services/syncService";

@inject(SyncService, ConnectionTracker)
export class TotalSync {
  private syncButtonIsLoading = false;

  constructor(private readonly syncService: SyncService, private readonly connTracker: ConnectionTracker) {}

  @computedFrom("connTracker.isOnline", "syncButtonIsLoading")
  get canSync() {
    return this.connTracker.isOnline && !this.syncButtonIsLoading;
  }

  async sync() {
    if (!this.canSync) {
      return;
    }

    this.syncButtonIsLoading = true;

    try {
      await this.syncService.totalSync();
    } catch {
      this.syncButtonIsLoading = false;
    }
  }

  back() {
    window.history.back();
  }
}
