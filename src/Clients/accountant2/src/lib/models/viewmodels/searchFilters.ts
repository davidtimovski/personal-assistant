import type { TransactionType } from './transactionType';

export class SearchFilters {
	constructor(
		public page: number,
		public pageSize: number,
		public fromDate: string,
		public toDate: string,
		public accountId: number,
		public categoryId: number | null,
		public type: TransactionType,
		public description: string | null
	) {}
}
