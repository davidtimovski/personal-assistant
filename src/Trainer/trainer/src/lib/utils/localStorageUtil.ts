import { LocalStorageBase } from '../../../../../Core/shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(new Map<string, any>([]));
	}
}

export enum LocalStorageKeys {}
