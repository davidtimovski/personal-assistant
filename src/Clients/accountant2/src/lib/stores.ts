import { writable } from 'svelte/store';
import type { User } from 'oidc-client';
import { AppEvents } from './models/appEvents';
import { SearchFilters } from './models/viewmodels/searchFilters';
import { DateHelper } from '../../../shared2/utils/dateHelper';
import { TransactionType } from './models/viewmodels/transactionType';

export const loggedInUser = writable<User | null>(null);
export const syncStatus = writable<AppEvents>(AppEvents.NotSyncing);

const from = new Date();
from.setDate(1);
const initialFromDate = DateHelper.format(from);
const initialToDate = DateHelper.format(new Date());
export const searchFilters = writable<SearchFilters>(
	new SearchFilters(1, 15, initialFromDate, initialToDate, 0, 0, TransactionType.Any, null)
);
