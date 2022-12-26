import type { List } from './entities';

export class State {
	constructor(public lists: List[] | null, public fromCache: boolean) {}
}
