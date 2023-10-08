import type { IsLoadable } from '../isLoadable';

export class ShareRequest implements IsLoadable {
	constructor(
		public recipeId: number,
		public recipeName: string,
		public recipeOwnerName: string,
		public isAccepted: boolean,
		public leftSideIsLoading: boolean,
		public rightSideIsLoading: boolean
	) {}
}
