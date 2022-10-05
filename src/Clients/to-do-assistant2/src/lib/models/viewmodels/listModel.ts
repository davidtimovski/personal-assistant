import type { SharingState } from '$lib/models/viewmodels/sharingState';

export class ListModel {
	constructor(
		public id: number,
		public name: string,
		public icon: string,
		public sharingState: SharingState,
		public order: number,
		public derivedListType: string,
		public derivedListIconClass: string | null,
		public uncompletedTaskCount: number
	) {}
}
