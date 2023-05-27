export class AccountItem {
  constructor(
    public id: number,
    public name: string,
    public currency: string,
    public stockPrice: number,
    public stocks: number,
    public balance: number,
    public synced: boolean
  ) {}
}
