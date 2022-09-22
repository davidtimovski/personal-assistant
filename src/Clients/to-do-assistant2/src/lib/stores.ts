import { writable } from 'svelte/store';
import type { User } from 'oidc-client';

import { AlertState } from '../../../shared2/models/alertState';
import { AlertStatus } from '../../../shared2/models/enums/alertEvents';

import { SyncEvents } from './models/syncEvents';
import type { List } from './models/entities';

export const isOnline = writable<boolean>(true);
export const loggedInUser = writable<User | null>(null);
export const syncStatus = writable<SyncEvents>(SyncEvents.NotSyncing);
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
export const lists = writable<List[]>([]);
