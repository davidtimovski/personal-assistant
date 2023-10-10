import type { Category } from '$lib/models/entities/category';
import type { Account } from '$lib/models/entities/account';
import type { TransactionModel } from '$lib/models/entities/transaction';
import type { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
import type { DebtModel } from '$lib/models/entities/debt';
import type { AutomaticTransaction } from '$lib/models/entities/automaticTransaction';

export class Changed {
	constructor(
		public lastSynced: string,
		public deletedAccountIds: number[],
		public accounts: Account[],
		public deletedCategoryIds: number[],
		public categories: Category[],
		public deletedTransactionIds: number[],
		public transactions: TransactionModel[],
		public deletedUpcomingExpenseIds: number[],
		public upcomingExpenses: UpcomingExpense[],
		public deletedDebtIds: number[],
		public debts: DebtModel[],
		public deletedAutomaticTransactionIds: number[],
		public automaticTransactions: AutomaticTransaction[]
	) {}
}

export class CreatedEntityIds {
	constructor(
		public accountIdPairs: CreatedIdPair[],
		public categoryIdPairs: CreatedIdPair[],
		public transactionIdPairs: CreatedIdPair[],
		public upcomingExpenseIdPairs: CreatedIdPair[],
		public debtIdPairs: CreatedIdPair[],
		public automaticTransactionIdPairs: CreatedIdPair[]
	) {}
}

export class CreatedIdPair {
	constructor(public localId: number, public id: number) {}
}
