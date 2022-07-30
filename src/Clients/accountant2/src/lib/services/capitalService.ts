import { CurrenciesService } from '../../../../shared2/services/currenciesService';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import { TransactionsIDBHelper } from '$lib/utils/transactionsIDBHelper';
import { UpcomingExpensesIDBHelper } from '$lib/utils/upcomingExpensesIDBHelper';
import { DebtsIDBHelper } from '$lib/utils/debtsIDBHelper';
import { AccountsService } from '$lib/services/accountsService';
import { Capital } from '$lib/models/capital';
import type { TransactionModel } from '$lib/models/entities/transaction';
import type { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
import type { DebtModel } from '$lib/models/entities/debt';
import { TransactionsService } from './transactionsService';
import { UpcomingExpenseDashboard } from '$lib/models/viewmodels/upcomingExpenseDashboard';
import { DebtDashboard } from '$lib/models/viewmodels/debtDashboard';

export class CapitalService {
	private readonly transactionsIDBHelper = new TransactionsIDBHelper();
	private readonly upcomingExpensesIDBHelper = new UpcomingExpensesIDBHelper();
	private readonly debtsIDBHelper = new DebtsIDBHelper();
	private readonly transactionsService = new TransactionsService();
	private readonly accountsService = new AccountsService();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

	async get(
		uncategorizedLabel: string,
		includeUpcomingExpenses: boolean,
		includeDebt: boolean,
		currency: string
	): Promise<Capital> {
		try {
			const mainAccountId = await this.accountsService.getMainId();
			const capital = new Capital(0, 0, 0, 0, [], [], []);

			const balancePromise = this.accountsService.getBalance(mainAccountId, currency).then((balance: number) => {
				capital.balance = balance;
			});

			const expendituresPromise = this.transactionsIDBHelper
				.getExpendituresForCurrentMonth(mainAccountId)
				.then(async (transactions: TransactionModel[]) => {
					capital.expenditures = await this.transactionsService.getByCategory(
						transactions,
						currency,
						uncategorizedLabel
					);

					capital.spent = transactions
						.filter((x) => !x.isTax)
						.map((e) => e.amount)
						.reduce((prev, curr) => prev + curr, 0);
				});

			const upcomingPromise = new Promise<void>((resolve) => {
				if (!includeUpcomingExpenses) {
					resolve();
					return;
				}

				this.upcomingExpensesIDBHelper.getAllForMonth().then((monthUpcomingExpenses: UpcomingExpense[]) => {
					monthUpcomingExpenses.forEach((x: UpcomingExpense) => {
						x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
						capital.upcoming += x.amount;
					});

					const upcomingExpenses = new Array<UpcomingExpenseDashboard>();
					for (const upcomingExpense of monthUpcomingExpenses) {
						const trimmedDescription = this.trimDescription(upcomingExpense.description);
						upcomingExpenses.push(
							new UpcomingExpenseDashboard(
								upcomingExpense.categoryName || uncategorizedLabel,
								trimmedDescription,
								upcomingExpense.amount
							)
						);
					}
					capital.upcomingExpenses = upcomingExpenses.sort(
						(a: UpcomingExpenseDashboard, b: UpcomingExpenseDashboard) => {
							return b.amount - a.amount;
						}
					);

					resolve();
				});
			});

			const debtPromise = new Promise<void>(async (resolve) => {
				if (!includeDebt) {
					resolve();
					return;
				}

				this.debtsIDBHelper.getAll().then((debt: DebtModel[]) => {
					debt.forEach((x: DebtModel) => {
						x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
					});

					const debtDashboard = new Array<DebtDashboard>();
					for (const debtItem of debt) {
						const trimmedDescription = this.trimDescription(debtItem.description);
						debtDashboard.push(
							new DebtDashboard(debtItem.person, debtItem.userIsDebtor, trimmedDescription, debtItem.amount)
						);
					}
					capital.debt = debtDashboard;

					resolve();
				});
			});

			await Promise.all([balancePromise, expendituresPromise, upcomingPromise, debtPromise]);

			capital.available = capital.balance - capital.upcoming;

			return capital;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	private trimDescription(description: string | null): string {
		if (!description) {
			return '';
		}

		const length = 25;
		if (description.length <= length) {
			return description;
		}

		return description.substring(0, length - 2) + '..';
	}
}
