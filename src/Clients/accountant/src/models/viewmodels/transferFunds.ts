import { SelectOption } from "./selectOption";

export class TransferFundsModel {
  constructor(
    public fromAccountId: number,
    public toAccountId: number,
    public amount: number,
    public currency: string,
    public accountOptions: Array<SelectOption>
  ) {}
}
