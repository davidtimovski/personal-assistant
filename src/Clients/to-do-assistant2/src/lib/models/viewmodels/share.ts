export class Share {
	constructor(
		public userId: number,
		public email: string,
		public imageUri: string,
		public isAdmin: boolean,
		public isAccepted: boolean,
		public createdDate: string | null
	) {}
}

export class ShareUserAndPermission {
	constructor(public userId: number, public isAdmin: boolean) {}
}
