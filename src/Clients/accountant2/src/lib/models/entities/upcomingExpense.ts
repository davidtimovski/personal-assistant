import type { Syncable } from '$lib/models/syncable';

export class UpcomingExpense implements Syncable {
	categoryName: string | undefined;
	synced = false;

	constructor(
		public id: number,
		public categoryId: number,
		public amount: number,
		public currency: string,
		public description: string,
		public date: string,
		public generated: boolean,
		public createdDate: Date,
		public modifiedDate: Date
	) {}
}
