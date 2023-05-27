import type { SharingState } from '$lib/models/viewmodels/sharingState';
import type { Share } from '$lib/models/entities';

export class ListWithShares {
	constructor(
		public id: number,
		public name: string,
		public sharingState: SharingState,
		public ownerEmail: string,
		public ownerName: string,
		public ownerImageUri: string,
		public userShare: Share,
		public shares: Array<Share>
	) {}
}
