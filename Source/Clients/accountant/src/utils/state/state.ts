import { SearchFilters } from "models/viewmodels/searchFilters";
import { TransactionType } from "models/viewmodels/transactionType";
import { DateHelper } from "../../../../shared/src/utils/dateHelper";

export interface State {
  filters: SearchFilters;
}

const from = new Date();
from.setDate(1);
const initialFromDate = DateHelper.format(from);
const initialToDate = DateHelper.format(new Date());

export const initialState: State = {
  filters: new SearchFilters(
    1,
    15,
    initialFromDate,
    initialToDate,
    0,
    0,
    TransactionType.Any,
    null
  ),
};
