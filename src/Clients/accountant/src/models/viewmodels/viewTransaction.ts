import { TransactionType } from "models/viewmodels/transactionType";

export class ViewTransaction {
  constructor(
    public type: TransactionType,
    public typeLabel: string,
    public accountLabel: string,
    public accountValue: string,
    public amount: number,
    public currency: string,
    public originalAmount: number,
    public fromStocks: number,
    public toStocks: number,
    public category: string,
    public description: string,
    public date: string,
    public isEncrypted: boolean,
    public encryptedDescription: string,
    public salt: string,
    public nonce: string,
    public generated: boolean,
    public decryptionPassword: string
  ) {}
}
