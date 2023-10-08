import { LocalStorageBase } from '../../../../../Core/shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				[LocalStorageKeys.Currency, 'EUR'],
				[LocalStorageKeys.CurrencyRates, { EUR: 1 }]
			])
		);
	}
}

export enum LocalStorageKeys {
	Currency = 'currency',
	CurrencyRates = 'currencyRates'
}
