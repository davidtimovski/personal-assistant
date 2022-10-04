import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { CurrenciesService } from '../../../../shared2/services/currenciesService';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { AutomaticTransactionsIDBHelper } from '$lib/utils/automaticTransactionsIDBHelper';
import type { AutomaticTransaction } from '$lib/models/entities/automaticTransaction';
import Variables from '$lib/variables';

export class AutomaticTransactionsService {
	private readonly httpProxy = new HttpProxy('accountant2');
	private readonly idbHelper = new AutomaticTransactionsIDBHelper();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant', 'accountant2');

	async getAll(currency: string): Promise<Array<AutomaticTransaction>> {
		try {
			const automaticTransactions = await this.idbHelper.getAll();

			automaticTransactions.forEach((x: AutomaticTransaction) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});

			return automaticTransactions;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	get(id: number): Promise<AutomaticTransaction> {
		return this.idbHelper.get(id);
	}

	async create(automaticTransaction: AutomaticTransaction): Promise<number> {
		try {
			automaticTransaction.amount = parseFloat(<any>automaticTransaction.amount);

			if (automaticTransaction.description) {
				automaticTransaction.description = automaticTransaction.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}
			const now = DateHelper.adjustForTimeZone(new Date());
			automaticTransaction.createdDate = automaticTransaction.modifiedDate = now;

			if (navigator.onLine) {
				automaticTransaction.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/api/automatictransactions`, {
					method: 'post',
					body: window.JSON.stringify(automaticTransaction)
				});
				automaticTransaction.synced = true;
			}

			await this.idbHelper.create(automaticTransaction);

			return automaticTransaction.id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(automaticTransaction: AutomaticTransaction): Promise<void> {
		try {
			automaticTransaction.amount = parseFloat(<any>automaticTransaction.amount);

			if (automaticTransaction.description) {
				automaticTransaction.description = automaticTransaction.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}
			automaticTransaction.modifiedDate = DateHelper.adjustForTimeZone(new Date());

			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/automatictransactions`, {
					method: 'put',
					body: window.JSON.stringify(automaticTransaction)
				});
				automaticTransaction.synced = true;
			} else if (await this.idbHelper.isSynced(automaticTransaction.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(automaticTransaction);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/automatictransactions/${id}`, {
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
}
