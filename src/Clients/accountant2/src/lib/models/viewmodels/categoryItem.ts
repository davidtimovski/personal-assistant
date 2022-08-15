import type { CategoryType } from '$lib/models/entities/category';
import type { Syncable } from '$lib/models/syncable';

export class CategoryItem implements Syncable {
	constructor(
		public id: number,
		public name: string,
		public type: CategoryType,
		public generateUpcomingExpense: boolean,
		public synced: boolean,
		public subCategories: CategoryItem[]
	) {}
}
