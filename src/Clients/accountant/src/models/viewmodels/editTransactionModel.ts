export class EditTransactionModel {
  constructor(
    public id: number,
    public fromAccountId: number,
    public toAccountId: number,
    public categoryId: number,
    public amount: number,
    public fromStocks: number,
    public toStocks: number,
    public currency: string,
    public description: string,
    public date: string,
    public isEncrypted: boolean,
    public encryptedDescription: string,
    public salt: string,
    public nonce: string,
    public decryptionPassword: string,
    public encrypt: boolean,
    public encryptionPassword: string,
    public createdDate: Date,
    public synced: boolean
  ) {}
}
