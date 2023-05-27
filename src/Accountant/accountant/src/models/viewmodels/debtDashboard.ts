export class DebtDashboard {
  constructor(
    public person: string,
    public userIsDebtor: boolean,
    public description: string,
    public amount: number
  ) {}
}
