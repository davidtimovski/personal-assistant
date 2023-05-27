import type { Syncable } from '$lib/models/syncable';

export class DebtModel implements Syncable {
	synced = false;

	constructor(
		public id: number,
		public person: string,
		public amount: number,
		public currency: string,
		public description: string | null,
		public userIsDebtor: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}
