export enum SyncEvents {
	NotSyncing = 'app:not-syncing',
	SyncStarted = 'app:sync-started',
	SyncFinished = 'app:sync-finished',
	ReSync = 'app:resync'
}

export class SyncStatus {
	constructor(public status: SyncEvents, public retrieved: number, public pushed: number) {}
}
