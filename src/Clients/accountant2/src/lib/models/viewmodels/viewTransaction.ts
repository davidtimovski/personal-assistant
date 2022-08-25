import type { TransactionType } from '$lib/models/viewmodels/transactionType';

export class ViewTransaction {
	constructor(
		public type: TransactionType,
		public typeLabel: string,
		public accountLabel: string | null,
		public accountValue: string | null,
		public amount: number,
		public currency: string,
		public originalAmount: number,
		public fromStocks: number | null,
		public toStocks: number | null,
		public category: string,
		public description: string | null,
		public date: string,
		public isEncrypted: boolean,
		public encryptedDescription: string | null,
		public salt: string | null,
		public nonce: string | null,
		public generated: boolean,
		public decryptionPassword: string | null
	) {}
}
