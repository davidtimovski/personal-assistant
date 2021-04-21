import { SelectOption } from "./selectOption";
import { Account } from "../entities/account";

export class TransferFundsModel {
  constructor(
    public fromAccountId: number,
    public fromAccount: Account,
    public toAccountId: number,
    public toAccount: Account,
    public amount: number,
    public currency: string,
    public accountOptions: Array<SelectOption>
  ) {}
}
