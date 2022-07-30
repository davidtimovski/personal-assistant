import type { Syncable } from '$lib/models/syncable';

export class Account implements Syncable {
	stockPrice: number | undefined;
	stocks: number | undefined;
	balance: number | undefined;
	synced = false;

	constructor(
		public id: number,
		public name: string,
		public currency: string,
		public isMain: boolean,
		public createdDate: Date,
		public modifiedDate: Date
	) {}
}
