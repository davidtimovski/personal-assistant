export class CreateTransaction {
	constructor(
		public fromAccountId: number | null,
		public toAccountId: number | null,
		public categoryId: number | null,
		public amount: number,
		public fromStocks: number | null,
		public toStocks: number | null,
		public currency: string,
		public description: string | null,
		public date: string,
		public isEncrypted: boolean,
		public encryptedDescription: string | null,
		public salt: string | null,
		public nonce: string | null,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}

export class UpdateTransaction {
	constructor(
		public id: number,
		public fromAccountId: number | null,
		public toAccountId: number | null,
		public categoryId: number | null,
		public amount: number,
		public fromStocks: number | null,
		public toStocks: number | null,
		public currency: string,
		public description: string | null,
		public date: string,
		public isEncrypted: boolean,
		public encryptedDescription: string | null,
		public salt: string | null,
		public nonce: string | null,
		public createdDate: Date | null,
		public modifiedDate: Date | null
	) {}
}
