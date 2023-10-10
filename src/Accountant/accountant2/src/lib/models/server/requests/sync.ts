import type { Category } from '$lib/models/entities/category';
import type { Account } from '$lib/models/entities/account';
import type { TransactionModel } from '$lib/models/entities/transaction';
import type { UpcomingExpense } from '$lib/models/entities/upcomingExpense';
import type { DebtModel } from '$lib/models/entities/debt';
import type { AutomaticTransaction } from '$lib/models/entities/automaticTransaction';

export class GetChanges {
	constructor(public lastSynced: string) {}
}

export class SyncEntities {
	constructor(
		public accounts: Account[],
		public categories: Category[],
		public transactions: TransactionModel[],
		public upcomingExpenses: UpcomingExpense[],
		public debts: DebtModel[],
		public automaticTransactions: AutomaticTransaction[]
	) {}
}
