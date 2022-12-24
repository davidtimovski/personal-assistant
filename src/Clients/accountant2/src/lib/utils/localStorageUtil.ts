import { DateHelper } from '../../../../shared2/utils/dateHelper';
import { LocalStorageBase } from '../../../../shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super(
			new Map<string, any>([
				[LocalStorageKeys.Currency, 'EUR'],
				[LocalStorageKeys.CurrencyRates, { EUR: 1 }],
				[LocalStorageKeys.LastSynced, '1970-01-01T00:00:00.000Z'],
				[LocalStorageKeys.MergeDebtPerPerson, true],
				[LocalStorageKeys.ShowUpcomingExpensesOnHomePage, true],
				[LocalStorageKeys.ShowDebtOnHomePage, true]
			])
		);
	}

	getLastSynced() {
		return this.get(LocalStorageKeys.LastSynced);
	}

	setLastSyncedNow() {
		this.set(LocalStorageKeys.LastSynced, DateHelper.adjustTimeZone(new Date()).toISOString());
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
	ShowUpcomingExpensesOnHomePage = 'showUpcomingExpensesOnHomePage',
	ShowDebtOnHomePage = 'showDebtOnHomePage'
}
