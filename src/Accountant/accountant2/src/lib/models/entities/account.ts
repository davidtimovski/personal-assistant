import type { Syncable } from '$lib/models/syncable';

export class Account implements Syncable {
	stockPrice: number | null = null;
	stocks: number | null = null;
	balance: number | null = null;
	synced = false;

	constructor(
		public id: number,
		public name: string,
		public currency: string,
		public isMain: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}
