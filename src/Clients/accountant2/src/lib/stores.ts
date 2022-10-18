import { writable } from 'svelte/store';

import { DateHelper } from '../../../shared2/utils/dateHelper';
import type { AuthInfo } from '../../../shared2/models/authInfo';
import { AlertState } from '../../../shared2/models/alertState';

import { AppEvents } from '$lib/models/appEvents';
import { SearchFilters } from '$lib/models/viewmodels/searchFilters';
import { TransactionType } from '$lib/models/viewmodels/transactionType';
import { AlertStatus } from '../../../shared2/models/enums/alertEvents';

export const isOnline = writable<boolean>(true);
export const locale = writable('en-US');
export const authInfo = writable<AuthInfo | null>(null);
export const syncStatus = writable<AppEvents>(AppEvents.NotSyncing);
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));

const from = new Date();
from.setDate(1);
const initialFromDate = DateHelper.format(from);
const initialToDate = DateHelper.format(new Date());
export const searchFilters = writable<SearchFilters>(
	new SearchFilters(1, 15, initialFromDate, initialToDate, 0, 0, TransactionType.Any, null)
);
