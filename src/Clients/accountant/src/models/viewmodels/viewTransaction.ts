export class ViewTransaction {
  constructor(
    public type: string,
    public accountLabel: string,
    public accountValue: string,
    public amount: number,
    public currency: string,
    public originalAmount: number,
    public category: string,
    public descriptionInHtml: string,
    public date: string,
    public isEncrypted: boolean,
    public encryptedDescription: string,
    public salt: string,
    public nonce: string,
    public decryptionPassword: string
  ) {}
}
