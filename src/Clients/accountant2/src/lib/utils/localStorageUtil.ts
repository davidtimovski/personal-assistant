import { LocalStorageBase } from '../../../../shared2/utils/localStorageBase';

export class LocalStorageUtil extends LocalStorageBase {
	constructor() {
		super({
			lastSynced: '1970-01-01T00:00:00.000Z',
			currency: 'EUR',
			currencyRates: { EUR: 1 },
			mergeDebtPerPerson: true,
			showUpcomingExpensesOnDashboard: true,
			showDebtOnDashboard: true
		});
	}
}
