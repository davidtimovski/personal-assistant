import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { CurrenciesService } from '../../../../shared2/services/currenciesService';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { UpcomingExpensesIDBHelper } from '$lib/utils/upcomingExpensesIDBHelper';
import type { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
import Variables from '$lib/variables';

export class UpcomingExpensesService {
	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new UpcomingExpensesIDBHelper();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

	async getAll(currency: string): Promise<Array<UpcomingExpense>> {
		try {
			const upcomingExpenses = await this.idbHelper.getAll();

			upcomingExpenses.forEach((x: UpcomingExpense) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});

			return upcomingExpenses;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	get(id: number): Promise<UpcomingExpense> {
		return this.idbHelper.get(id);
	}

	async create(upcomingExpense: UpcomingExpense): Promise<number> {
		try {
			if (typeof upcomingExpense.amount === 'string') {
				upcomingExpense.amount = parseFloat(upcomingExpense.amount);
			}

			if (upcomingExpense.description) {
				upcomingExpense.description = upcomingExpense.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}
			const now = DateHelper.adjustTimeZone(new Date());
			upcomingExpense.createdDate = upcomingExpense.modifiedDate = now;

			if (navigator.onLine) {
				upcomingExpense.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/upcoming-expenses`, {
					method: 'post',
					body: window.JSON.stringify(upcomingExpense)
				});
				upcomingExpense.synced = true;
			}

			await this.idbHelper.create(upcomingExpense);

			return upcomingExpense.id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(upcomingExpense: UpcomingExpense): Promise<void> {
		try {
			if (typeof upcomingExpense.amount === 'string') {
				upcomingExpense.amount = parseFloat(upcomingExpense.amount);
			}

			if (upcomingExpense.description) {
				upcomingExpense.description = upcomingExpense.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}
			upcomingExpense.modifiedDate = DateHelper.adjustTimeZone(new Date());

			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/upcoming-expenses`, {
					method: 'put',
					body: window.JSON.stringify(upcomingExpense)
				});
				upcomingExpense.synced = true;
			} else if (await this.idbHelper.isSynced(upcomingExpense.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(upcomingExpense);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/upcoming-expenses/${id}`, {
					method: 'delete'
				});
			} else if (await this.idbHelper.isSynced(id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.delete(id);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	release() {
		this.httpProxy.release();
		this.currenciesService.release();
		this.logger.release();
	}
}
