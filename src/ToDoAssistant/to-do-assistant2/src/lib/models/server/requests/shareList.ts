import type { ShareUserAndPermission } from './shareUserAndPermission';

export class ShareList {
	constructor(
		public readonly listId: number,
		public readonly newShares: ShareUserAndPermission[],
		public readonly editedShares: ShareUserAndPermission[],
		public readonly removedShares: ShareUserAndPermission[]
	) {}
}
