export class Created {
  constructor(
    public accountIdPairs: Array<CreatedIdPair>,
    public categoryIdPairs: Array<CreatedIdPair>,
    public transactionIdPairs: Array<CreatedIdPair>,
    public upcomingExpenseIdPairs: Array<CreatedIdPair>,
    public debtIdPairs: Array<CreatedIdPair>
  ) {}
}

export class CreatedIdPair {
  constructor(public localId: number, public id: number) {}
}
