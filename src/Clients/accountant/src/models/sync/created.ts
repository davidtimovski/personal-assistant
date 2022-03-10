export class Created {
  accountIdPairs: Array<CreatedIdPair>;
  categoryIdPairs: Array<CreatedIdPair>;
  transactionIdPairs: Array<CreatedIdPair>;
  upcomingExpenseIdPairs: Array<CreatedIdPair>;
  debtIdPairs: Array<CreatedIdPair>;
}

export class CreatedIdPair {
  localId: number;
  id: number;
}
