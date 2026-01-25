import { CurrenciesService } from '../../../../../Core/shared2/services/currenciesService';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';

import { TransactionsIDBHelper } from '$lib/utils/transactionsIDBHelper';
import { UpcomingExpensesIDBHelper } from '$lib/utils/upcomingExpensesIDBHelper';
import { DebtsIDBHelper } from '$lib/utils/debtsIDBHelper';
import type { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
import type { DebtModel } from '$lib/models/entities/debt';
import { TransactionsService } from '$lib/services/transactionsService';
import { HomePageDebt, HomePageUpcomingExpense } from '$lib/models/viewmodels/homePage';
import type { AmountByCategory } from '$lib/models/viewmodels/amountByCategory';
import { DebtsService } from '$lib/services/debtsService';

export class CapitalService {
	private readonly transactionsIDBHelper = new TransactionsIDBHelper();
	private readonly upcomingExpensesIDBHelper = new UpcomingExpensesIDBHelper();
	private readonly debtsIDBHelper = new DebtsIDBHelper();
	private readonly transactionsService = new TransactionsService();
	private readonly currenciesService = new CurrenciesService('Accountant');
	private readonly logger = new ErrorLogger('Accountant');

	async getSpent(mainAccountId: number, uncategorizedLabel: string, currency: string): Promise<{ expenditures: AmountByCategory[]; spent: number }> {
		try {
			const transactions = await this.transactionsIDBHelper.getExpendituresForCurrentMonth(mainAccountId);
			const expenditures = await this.transactionsService.getAmountByCategory(transactions, currency, uncategorizedLabel);

			const spent = transactions
				.filter((x) => !x.isTax)
				.map((e) => e.amount)
				.reduce((prev, curr) => prev + curr, 0);

			return { expenditures, spent };
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getUpcomingExpenses(
		uncategorizedLabel: string,
		currency: string
	): Promise<{ upcomingExpenses: HomePageUpcomingExpense[]; upcomingAmount: number }> {
		try {
			const monthUpcomingExpenses = await this.upcomingExpensesIDBHelper.getAllForMonth();

			let upcomingAmount = 0;
			monthUpcomingExpenses.forEach((x: UpcomingExpense) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
				upcomingAmount += x.amount;
			});

			let upcomingExpenses = new Array<HomePageUpcomingExpense>();
			for (const upcomingExpense of monthUpcomingExpenses) {
				const trimmedDescription = this.trimDescription(upcomingExpense.description);
				upcomingExpenses.push(
					new HomePageUpcomingExpense(
						upcomingExpense.id,
						upcomingExpense.categoryName || uncategorizedLabel,
						trimmedDescription,
						upcomingExpense.amount
					)
				);
			}
			upcomingExpenses = upcomingExpenses.sort((a: HomePageUpcomingExpense, b: HomePageUpcomingExpense) => {
				return b.amount - a.amount;
			});

			return { upcomingExpenses, upcomingAmount };
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getUpcomingExpensesAmount(currency: string): Promise<number> {
		try {
			const monthUpcomingExpenses = await this.upcomingExpensesIDBHelper.getAllForMonth();

			let upcomingAmount = 0;
			monthUpcomingExpenses.forEach((x: UpcomingExpense) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
				upcomingAmount += x.amount;
			});

			return upcomingAmount;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async getDebt(currency: string, combinedLabel: string): Promise<HomePageDebt[]> {
		try {
			const debt = await this.debtsIDBHelper.getAll();

			debt.forEach((x: DebtModel) => {
				x.amount = this.currenciesService.convert(x.amount, x.currency, currency);
			});

			const homePageDebt = new Array<HomePageDebt>();
			for (const debtItem of debt) {
				const trimmedDescription = this.trimDebtDescription(debtItem.description, combinedLabel);
				homePageDebt.push(new HomePageDebt(debtItem.id, debtItem.person, debtItem.userIsDebtor, trimmedDescription, debtItem.amount));
			}

			return homePageDebt;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	release() {
		this.currenciesService.release();
		this.logger.release();
	}

	private trimDebtDescription(description: string | null, combinedLabel: string): string {
		if (!description) {
			return '';
		}

		if (description.includes(DebtsService.mergedDebtSeparator)) {
			return combinedLabel;
		}

		return this.trimDescription(description);
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
