import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { CurrenciesService } from '../../../../shared2/services/currenciesService';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';
import { DateHelper } from '../../../../shared2/utils/dateHelper';

import { DebtsIDBHelper } from '$lib/utils/debtsIDBHelper';
import { DebtModel } from '$lib/models/entities/debt';

export class DebtsService {
	private readonly mergedDebtSeparator = '----------';

	private readonly httpProxy = new HttpProxy();
	private readonly idbHelper = new DebtsIDBHelper();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

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

	async createOrMerge(debt: DebtModel, mergeDebtPerPerson: boolean): Promise<number> {
		try {
			const now = DateHelper.adjustForTimeZone(new Date());
			debt.amount = parseFloat(<any>debt.amount);

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
						new Date(otherDebt.createdDate),
						otherDebt.description
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
					debt.description
				);
				descriptionsArray.push(newDesc);

				const description =
					descriptionsArray.length > 0 ? descriptionsArray.join(`\n${this.mergedDebtSeparator}\n`) : null;

				const mergedDebt = new DebtModel(0, debt.person, balance, debt.currency, description, balance < 0, now, now);

				if (navigator.onLine) {
					mergedDebt.id = await this.httpProxy.ajax<number>('api/debts/merged', {
						method: 'post',
						body: window.JSON.stringify(mergedDebt)
					});
					mergedDebt.synced = true;
				}

				const otherDebtIds = otherDebtWithPerson.map((x) => x.id);
				await this.idbHelper.createMerged(mergedDebt, otherDebtIds);

				return mergedDebt.id;
			} else {
				if (debt.description) {
					debt.description = debt.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
				}
				debt.createdDate = debt.modifiedDate = now;

				if (navigator.onLine) {
					debt.id = await this.httpProxy.ajax<number>('api/debts', {
						method: 'post',
						body: window.JSON.stringify(debt)
					});
					debt.synced = true;
				}

				await this.idbHelper.create(debt);

				return debt.id;
			}
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	private getMergedDebtDescription(
		amount: number,
		currency: string,
		userIsDebtor: boolean,
		createdDate: Date,
		description: string | null
	) {
		try {
			const date = DateHelper.formatForReading(createdDate);

			// start with date
			let result = date + ' | ';

			// add amount (positive or negative) and currency
			result += userIsDebtor ? '-' + amount + currency : amount + currency;

			if (description) {
				if (description.includes(this.mergedDebtSeparator)) {
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

	async update(debt: DebtModel): Promise<void> {
		try {
			debt.amount = parseFloat(<any>debt.amount);

			if (debt.description) {
				debt.description = debt.description.replace(/(\r\n|\r|\n){3,}/g, '$1\n').trim();
			}
			debt.modifiedDate = DateHelper.adjustForTimeZone(new Date());

			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute('api/debts', {
					method: 'put',
					body: window.JSON.stringify(debt)
				});
				debt.synced = true;
			} else if (await this.idbHelper.isSynced(debt.id)) {
				throw 'failedToFetchError';
			}

			await this.idbHelper.update(debt);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			if (navigator.onLine) {
				await this.httpProxy.ajaxExecute(`api/debts/${id}`, {
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
