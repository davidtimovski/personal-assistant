import type { Syncable } from '$lib/models/syncable';

export class AutomaticTransaction implements Syncable {
	categoryName: string | undefined;
	synced = false;

	constructor(
		public id: number,
		public isDeposit: boolean,
		public categoryId: number,
		public amount: number,
		public currency: string,
		public description: string,
		public dayInMonth: number,
		public createdDate: Date,
		public modifiedDate: Date
	) {}
}
