import { LocalStorageBase } from '../../../../../Core/shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				[LocalStorageKeys.Currency, 'EUR'],
				[LocalStorageKeys.CurrencyRates, { EUR: 1 }],
				[LocalStorageKeys.LastSynced, '1970-01-01T00:00:00.000Z'],
				[LocalStorageKeys.MergeDebtPerPerson, true],
				[LocalStorageKeys.ShowBalanceOnHomePage, false],
				[LocalStorageKeys.ShowUpcomingExpensesOnHomePage, true],
				[LocalStorageKeys.ShowDebtOnHomePage, true]
			])
		);
	}

	getLastSynced() {
		return this.get(LocalStorageKeys.LastSynced);
	}

	setLastSyncedNow() {
		this.set(LocalStorageKeys.LastSynced, new Date().toISOString());
	}

	setLastSynced(lastSynced: string) {
		this.set(LocalStorageKeys.LastSynced, lastSynced);
	}
}

export enum LocalStorageKeys {
	Currency = 'currency',
	CurrencyRates = 'currencyRates',
	LastSynced = 'lastSynced',
	MergeDebtPerPerson = 'mergeDebtPerPerson',
	ShowBalanceOnHomePage = 'showBalanceOnHomePage',
	ShowUpcomingExpensesOnHomePage = 'showUpcomingExpensesOnHomePage',
	ShowDebtOnHomePage = 'showDebtOnHomePage'
}
