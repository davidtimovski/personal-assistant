export class NewTransactionModel {
  constructor(
    public mainAccountId: number,
    public categoryId: number,
    public amount: number,
    public currency: string,
    public description: string,
    public date: string,
    public encrypt: boolean,
    public encryptionPassword: string
  ) {}
}
