import type { Syncable } from '$lib/models/syncable';

export class Category implements Syncable {
	synced = false;
	parent: Category | undefined;

	constructor(
		public id: number,
		public parentId: number | null,
		public name: string,
		public type: CategoryType,
		public generateUpcomingExpense: boolean,
		public isTax: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}

	get fullName(): string {
		return this.parent ? `${this.parent.name}/${this.name}` : this.name;
	}
}

export enum CategoryType {
	AllTransactions,
	DepositOnly,
	ExpenseOnly
}
