export class AutomaticTransactionItem {
  constructor(
    public id: number,
    public isDeposit: boolean,
    public amount: number,
    public currency: string,
    public category: string,
    public dayInMonth: string,
    public synced: boolean
  ) {}
}
