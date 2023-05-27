import type { SharingState } from '$lib/models/viewmodels/sharingState';

export class ArchivedList {
	constructor(public id: number, public name: string, public icon: string, public sharingState: SharingState) {}
}
