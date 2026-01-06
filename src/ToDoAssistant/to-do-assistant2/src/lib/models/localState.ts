import type { List } from './entities';

export class LocalState {
	constructor(
		public lists: List[] | null,
		public fromCache: boolean
	) {}
}
