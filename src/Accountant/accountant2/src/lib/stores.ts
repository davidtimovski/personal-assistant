import { writable } from 'svelte/store';

import { DateHelper } from '../../../../Core/shared2/utils/dateHelper';
import type { AuthInfo } from '../../../../Core/shared2/models/authInfo';
import { AlertState } from '../../../../Core/shared2/models/alertState';
import { AlertStatus } from '../../../../Core/shared2/models/enums/alertEvents';

import { AccountantUser } from '$lib/models/accountantUser';
import { SyncStatus, SyncEvents } from '$lib/models/syncStatus';
import { SearchFilters } from '$lib/models/viewmodels/searchFilters';
import { TransactionType } from '$lib/models/viewmodels/transactionType';
import Variables from '$lib/variables';

export const isOnline = writable<boolean>(true);
export const authInfo = writable<AuthInfo | null>(null);
export const user = writable<AccountantUser>(new AccountantUser('', '', 'en-US', 'en-US', Variables.urls.defaultProfileImageUrl));
export const syncStatus = writable<SyncStatus>(new SyncStatus(SyncEvents.NotSyncing, 0, 0));
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));

const from = new Date();
from.setDate(1);
const initialFromDate = DateHelper.format(from);
const initialToDate = DateHelper.format(new Date());
export const searchFilters = writable<SearchFilters>(new SearchFilters(1, 15, initialFromDate, initialToDate, 0, 0, TransactionType.Any, null));
