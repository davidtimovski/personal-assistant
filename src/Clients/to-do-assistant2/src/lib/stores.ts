import { writable } from 'svelte/store';

import type { AuthInfo } from '../../../shared2/models/authInfo';
import { AlertState } from '../../../shared2/models/alertState';
import { AlertStatus } from '../../../shared2/models/enums/alertEvents';

import { RemoteEvent, RemoteEventType } from './models/remoteEvents';
import type { List } from './models/entities';

export const isOffline = writable<boolean>(false);
export const locale = writable('en-US');
export const authInfo = writable<AuthInfo | null>(null);
export const remoteEvents = writable<RemoteEvent>(new RemoteEvent(RemoteEventType.None, null));
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
export const lists = writable<List[]>([]);
