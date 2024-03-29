export class AccountItem {
	constructor(
		public id: number,
		public name: string,
		public currency: string,
		public stockPrice: number | null,
		public stocks: number | null,
		public balance: number,
		public synced: boolean
	) {}
}
