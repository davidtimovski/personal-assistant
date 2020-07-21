import { TransactionType } from "./transactionType";

export class SearchFilters {
  constructor(
    public page: number,
    public pageSize: number,
    public fromDate: string,
    public toDate: string,
    public categoryId: number,
    public accountId: number,
    public type: TransactionType,
    public description: string
  ) {}
}
