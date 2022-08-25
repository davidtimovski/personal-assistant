import { CurrenciesService } from '../../../../shared2/services/currenciesService';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import { TransactionsIDBHelper } from '$lib/utils/transactionsIDBHelper';
import { UpcomingExpensesIDBHelper } from '$lib/utils/upcomingExpensesIDBHelper';
import { DebtsIDBHelper } from '$lib/utils/debtsIDBHelper';
import type { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
import type { DebtModel } from '$lib/models/entities/debt';
import { TransactionsService } from '$lib/services/transactionsService';
import { UpcomingExpenseDashboard } from '$lib/models/viewmodels/upcomingExpenseDashboard';
import { DebtDashboard } from '$lib/models/viewmodels/debtDashboard';
import type { AmountByCategory } from '$lib/models/viewmodels/amountByCategory';
import { DebtsService } from '$lib/services/debtsService';

export class CapitalService {
	private readonly transactionsIDBHelper = new TransactionsIDBHelper();
	private readonly upcomingExpensesIDBHelper = new UpcomingExpensesIDBHelper();
	private readonly debtsIDBHelper = new DebtsIDBHelper();
	private readonly transactionsService = new TransactionsService();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

	async getSpent(
		mainAccountId: number,
		uncategorizedLabel: string,
		currency: string
	): Promise<[AmountByCategory[], number]> {
		try {
			const transactions = await this.transactionsIDBHelper.getExpendituresForCurrentMonth(mainAccountId);
			const expenditures = await this.transactionsService.getByCategory(transactions, currency, uncategorizedLabel);

			const spent = transactions
				.filter((x) => !x.isTax)
				.map((e) => e.amount)
				.reduce((prev, curr) => prev + curr, 0);

			return [expenditures, spent];
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getUpcomingExpenses(
		uncategorizedLabel: string,
		currency: string
	): Promise<[UpcomingExpenseDashboard[], number]> {
		try {
			const monthUpcomingExpenses = await this.upcomingExpensesIDBHelper.getAllForMonth();

			let upcomingSum = 0;
			monthUpcomingExpenses.forEach((x: UpcomingExpense) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
				upcomingSum += x.amount;
			});

			let upcomingExpenses = new Array<UpcomingExpenseDashboard>();
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
			upcomingExpenses = upcomingExpenses.sort((a: UpcomingExpenseDashboard, b: UpcomingExpenseDashboard) => {
				return b.amount - a.amount;
			});

			return [upcomingExpenses, upcomingSum];
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getDebt(currency: string): Promise<DebtDashboard[]> {
		try {
			const debt = await this.debtsIDBHelper.getAll();

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

			return debtDashboard;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	private trimDescription(description: string | null): string {
		if (!description) {
			return '';
		}

		if (description.includes(DebtsService.mergedDebtSeparator)) {
			return '[ Combined ]';
		}

		const length = 25;
		if (description.length <= length) {
			return description;
		}

		return description.substring(0, length - 2) + '..';
	}
}
