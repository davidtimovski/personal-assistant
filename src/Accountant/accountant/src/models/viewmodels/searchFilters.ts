import { TransactionType } from "./transactionType";

export class SearchFilters {
  constructor(
    public page: number,
    public pageSize: number,
    public fromDate: string,
    public toDate: string,
    public accountId: number,
    public categoryId: number,
    public type: TransactionType,
    public description: string
  ) {}
}
