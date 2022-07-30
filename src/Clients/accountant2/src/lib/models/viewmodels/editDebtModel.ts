export class EditDebtModel {
  constructor(
    public id: number,
    public person: string,
    public amount: number,
    public currency: string,
    public description: string,
    public userIsDebtor: boolean,
    public createdDate: Date,
    public synced: boolean
  ) {}
}
