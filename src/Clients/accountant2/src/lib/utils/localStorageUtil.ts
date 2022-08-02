import { LocalStorageBase } from '../../../../shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super({
			currency: 'EUR',
			currencyRates: { EUR: 1 },
			lastSynced: '1970-01-01T00:00:00.000Z',
			mergeDebtPerPerson: true,
			showUpcomingExpensesOnDashboard: true,
			showDebtOnDashboard: true
		});
	}
}

export enum LocalStorageKeys {
	LastSynced = 'lastSynced',
	MergeDebtPerPerson = 'mergeDebtPerPerson',
	ShowUpcomingExpensesOnDashboard = 'showUpcomingExpensesOnDashboard',
	ShowDebtOnDashboard = 'showDebtOnDashboard'
}
