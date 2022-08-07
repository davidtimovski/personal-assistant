import type { Syncable } from '$lib/models/syncable';
import type { Category } from '$lib/models/entities/category';

export class TransactionModel implements Syncable {
	accountName: string | undefined;
	category: Category | null = null;
	convertedAmount: number | undefined;
	synced = false;

	constructor(
		public id: number,
		public fromAccountId: number | null,
		public toAccountId: number | null,
		public categoryId: number | null,
		public amount: number,
		public fromStocks: number | null,
		public toStocks: number | null,
		public currency: string,
		public description: string | null,
		public date: string,
		public isEncrypted: boolean,
		public encryptedDescription: string | null,
		public salt: string | null,
		public nonce: string | null,
		public generated: boolean,
		public createdDate: Date,
		public modifiedDate: Date | null
	) {}

	get isTax() {
		return this.category?.isTax;
	}
}
