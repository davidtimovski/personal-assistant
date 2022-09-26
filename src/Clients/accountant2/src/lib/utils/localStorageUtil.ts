import { LocalStorageBase } from '../../../../shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				[LocalStorageKeys.Currency, 'EUR'],
				['currencyRates', { EUR: 1 }],
				[LocalStorageKeys.LastSynced, '1970-01-01T00:00:00.000Z'],
				[LocalStorageKeys.MergeDebtPerPerson, true],
				[LocalStorageKeys.ShowUpcomingExpensesOnHomePage, true],
				[LocalStorageKeys.ShowDebtOnHomePage, true]
			])
		);
	}
}

export enum LocalStorageKeys {
	Currency = 'currency',
	LastSynced = 'lastSynced',
	MergeDebtPerPerson = 'mergeDebtPerPerson',
	ShowUpcomingExpensesOnHomePage = 'showUpcomingExpensesOnHomePage',
	ShowDebtOnHomePage = 'showDebtOnHomePage'
}
