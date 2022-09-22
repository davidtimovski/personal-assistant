import type { SharingState } from '$lib/models/viewmodels/sharingState';

export class ListModel {
	constructor(
		public id: number,
		public name: string,
		public icon: string,
		public sharingState: SharingState,
		public order: number,
		public computedListType: string,
		public computedListIconClass: string | null,
		public uncompletedTaskCount: number
	) {}

	public get isEmpty() {
		return this.uncompletedTaskCount === 0;
	}
}
