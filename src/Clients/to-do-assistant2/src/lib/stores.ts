import { writable } from 'svelte/store';
import type { User } from 'oidc-client';

import { AlertState } from '../../../shared2/models/alertState';
import { AlertStatus } from '../../../shared2/models/enums/alertEvents';

import { RemoteEvent, RemoteEventType } from './models/remoteEvents';
import type { List } from './models/entities';

export const isOnline = writable<boolean>(true);
export const loggedInUser = writable<User | null>(null);
export const remoteEvents = writable<RemoteEvent>(new RemoteEvent(RemoteEventType.None, null));
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
export const lists = writable<List[]>([]);
