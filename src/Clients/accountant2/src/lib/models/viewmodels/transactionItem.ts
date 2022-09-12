import type { TransactionType } from './transactionType';

export class TransactionItem {
	constructor(
		public id: number,
		public amount: number,
		public type: TransactionType,
		public category: string,
		public description: string,
		public date: string,
		public synced: boolean
	) {}
}
