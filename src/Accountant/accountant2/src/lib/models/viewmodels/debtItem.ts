export class DebtItem {
  constructor(
    public id: number,
    public userIsDebtor: boolean,
    public amount: number,
    public currency: string,
    public person: string,
    public created: string,
    public synced: boolean
  ) {}
}
