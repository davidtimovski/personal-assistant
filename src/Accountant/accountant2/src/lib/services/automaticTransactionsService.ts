import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';
import { ValidationResult, ValidationUtil } from '../../../../../Core/shared2/utils/validationUtils';

import { AutomaticTransactionsIDBHelper } from '$lib/utils/automaticTransactionsIDBHelper';
import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
import type { AutomaticTransaction } from '$lib/models/entities/automaticTransaction';
import {
	CreateAutomaticTransaction,
	UpdateAutomaticTransaction
} from '$lib/models/server/requests/automaticTransaction';
import Variables from '$lib/variables';

export class AutomaticTransactionsService {
	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new AutomaticTransactionsIDBHelper();
	private readonly localStorage = new LocalStorageUtil();
	private readonly logger = new ErrorLogger('Accountant');

	async getAll(): Promise<Array<AutomaticTransaction>> {
		try {
			return await this.idbHelper.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	get(id: number): Promise<AutomaticTransaction> {
		return this.idbHelper.get(id);
	}

	static validate(amount: number, amountFrom: number, amountTo: number): ValidationResult {
		const result = new ValidationResult();

		if (!ValidationUtil.between(amount, amountFrom, amountTo)) {
			result.fail('amount');
		}

		return result;
	}

	async create(automaticTransaction: AutomaticTransaction): Promise<number> {
		try {
			if (typeof automaticTransaction.amount === 'string') {
				automaticTransaction.amount = parseFloat(automaticTransaction.amount);
			}

			if (automaticTransaction.description) {
				automaticTransaction.description = automaticTransaction.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}
			const now = new Date();
			automaticTransaction.createdDate = automaticTransaction.modifiedDate = now;

			if (navigator.onLine) {
				const payload = new CreateAutomaticTransaction(
					automaticTransaction.isDeposit,
					automaticTransaction.categoryId,
					automaticTransaction.amount,
					automaticTransaction.currency,
					automaticTransaction.description,
					automaticTransaction.dayInMonth,
					automaticTransaction.createdDate,
					automaticTransaction.modifiedDate
				);

				automaticTransaction.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/automatic-transactions`, {
					method: 'post',
					body: window.JSON.stringify(payload)
				});
				automaticTransaction.synced = true;
			}

			await this.idbHelper.create(automaticTransaction);

			this.localStorage.setLastSyncedNow();

			return automaticTransaction.id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(automaticTransaction: AutomaticTransaction): Promise<void> {
		try {
			if (typeof automaticTransaction.amount === 'string') {
				automaticTransaction.amount = parseFloat(automaticTransaction.amount);
			}

			if (automaticTransaction.description) {
				automaticTransaction.description = automaticTransaction.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}
			automaticTransaction.modifiedDate = new Date();

			if (navigator.onLine) {
				const payload = new UpdateAutomaticTransaction(
					automaticTransaction.id,
					automaticTransaction.isDeposit,
					automaticTransaction.categoryId,
					automaticTransaction.amount,
					automaticTransaction.currency,
					automaticTransaction.description,
					automaticTransaction.dayInMonth,
					automaticTransaction.createdDate,
					automaticTransaction.modifiedDate
				);

				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/automatic-transactions`, {
					method: 'put',
					body: window.JSON.stringify(payload)
				});
				automaticTransaction.synced = true;
			} else if (await this.idbHelper.isSynced(automaticTransaction.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(automaticTransaction);

			this.localStorage.setLastSyncedNow();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/automatic-transactions/${id}`, {
					method: 'delete'
				});
			} else if (await this.idbHelper.isSynced(id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.delete(id);

			this.localStorage.setLastSyncedNow();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
