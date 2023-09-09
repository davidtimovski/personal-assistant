import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { CurrenciesService } from '../../../../../Core/shared2/services/currenciesService';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';
import { DateHelper } from '../../../../../Core/shared2/utils/dateHelper';

import { DebtsIDBHelper } from '$lib/utils/debtsIDBHelper';
import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
import { Formatter } from '$lib/utils/formatter';
import { DebtModel } from '$lib/models/entities/debt';
import Variables from '$lib/variables';

export class DebtsService {
	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new DebtsIDBHelper();
	private readonly localStorage = new LocalStorageUtil();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

	static readonly mergedDebtSeparator = '--------------';

	async getAll(currency: string): Promise<Array<DebtModel>> {
		try {
			const debts = await this.idbHelper.getAll();

			debts.forEach((x: DebtModel) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});

			return debts;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	get(id: number): Promise<DebtModel> {
		return this.idbHelper.get(id);
	}

	async createOrMerge(
		debt: DebtModel,
		mergeDebtPerPerson: boolean,
		culture: string,
		lendedLabel: string,
		borrowedLabel: string
	): Promise<number> {
		try {
			const now = new Date();

			debt.person = debt.person.trim();

			if (typeof debt.amount === 'string') {
				debt.amount = parseFloat(debt.amount);
			}

			const otherDebtWithPerson = await this.idbHelper.getByPerson(debt.person.trim().toLowerCase());
			if (mergeDebtPerPerson && otherDebtWithPerson.length > 0) {
				const descriptionsArray = new Array<string>();
				let balance = 0;
				for (const otherDebt of otherDebtWithPerson) {
					const convertedAmount = this.currenciesService.convert(otherDebt.amount, otherDebt.currency, debt.currency);
					if (otherDebt.userIsDebtor) {
						balance -= convertedAmount;
					} else {
						balance += convertedAmount;
					}

					const desc = this.getMergedDebtDescription(
						convertedAmount,
						debt.currency,
						otherDebt.userIsDebtor,
						new Date(<Date>otherDebt.createdDate),
						otherDebt.description,
						culture,
						lendedLabel,
						borrowedLabel
					);
					descriptionsArray.push(desc);
				}

				if (debt.userIsDebtor) {
					balance -= debt.amount;
				} else {
					balance += debt.amount;
				}

				const newDesc = this.getMergedDebtDescription(
					debt.amount,
					debt.currency,
					debt.userIsDebtor,
					now,
					debt.description,
					culture,
					lendedLabel,
					borrowedLabel
				);
				descriptionsArray.push(newDesc);

				const description =
					descriptionsArray.length > 0 ? descriptionsArray.join(`\n${DebtsService.mergedDebtSeparator}\n`) : null;

				const userIsDebtor = balance < 0;
				const mergedDebt = new DebtModel(
					0,
					debt.person,
					Math.abs(balance),
					debt.currency,
					description,
					userIsDebtor,
					now,
					now
				);

				if (navigator.onLine) {
					mergedDebt.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/debts/merged`, {
						method: 'post',
						body: window.JSON.stringify(mergedDebt)
					});
					mergedDebt.synced = true;
				}

				const otherDebtIds = otherDebtWithPerson.map((x) => x.id);
				await this.idbHelper.createMerged(mergedDebt, otherDebtIds);

				this.localStorage.setLastSyncedNow();

				return mergedDebt.id;
			} else {
				if (debt.description) {
					debt.description = debt.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
				}
				debt.createdDate = debt.modifiedDate = now;

				if (navigator.onLine) {
					debt.id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/debts`, {
						method: 'post',
						body: window.JSON.stringify(debt)
					});
					debt.synced = true;
				}

				await this.idbHelper.create(debt);

				this.localStorage.setLastSyncedNow();

				return debt.id;
			}
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(debt: DebtModel): Promise<void> {
		try {
			debt.modifiedDate = new Date();
			debt.person = debt.person.trim();

			if (typeof debt.amount === 'string') {
				debt.amount = parseFloat(debt.amount);
			}

			if (debt.description) {
				debt.description = debt.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}

			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/debts`, {
					method: 'put',
					body: window.JSON.stringify(debt)
				});
				debt.synced = true;
			} else if (await this.idbHelper.isSynced(debt.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(debt);

			this.localStorage.setLastSyncedNow();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`${Variables.urls.api}/debts/${id}`, {
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
		this.currenciesService.release();
		this.logger.release();
	}

	private getMergedDebtDescription(
		amount: number,
		currency: string,
		userIsDebtor: boolean,
		createdDate: Date,
		description: string | null,
		culture: string,
		lendedLabel: string,
		borrowedLabel: string
	) {
		try {
			const date = DateHelper.formatForReading(createdDate);

			// start with date
			let result = date + ' | ';

			// add amount (positive or negative) and currency
			result += (userIsDebtor ? borrowedLabel : lendedLabel) + ' ' + Formatter.money(amount, currency, culture);

			if (description) {
				if (description.includes(DebtsService.mergedDebtSeparator)) {
					// use existing description if the debt is already a merged one
					result = description;
				} else {
					result += ` | ${description}`;
				}
			}

			return result;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}
}
