import type { SharingState } from '../../sharingState';
import type { Share } from '../../viewmodels/share';

export class RecipeWithSharesResponse {
	constructor(
		public id: number,
		public name: string,
		public sharingState: SharingState,
		public ownerEmail: string,
		public ownerImageUri: string,
		public userShare: Share,
		public shares: Share[]
	) {}
}
