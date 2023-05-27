export class EditAutomaticTransactionModel {
  constructor(
    public id: number,
    public isDeposit: boolean,
    public categoryId: number,
    public amount: number,
    public currency: string,
    public description: string,
    public dayInMonth: number,
    public createdDate: Date,
    public synced: boolean
  ) {}
}
