import type { CategoryType } from '$lib/models/entities/category';

export class CreateCategory {
	constructor(
		public parentId: number | null,
		public name: string,
		public type: CategoryType,
		public generateUpcomingExpense: boolean,
		public isTax: boolean,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}

export class UpdateCategory {
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
}
