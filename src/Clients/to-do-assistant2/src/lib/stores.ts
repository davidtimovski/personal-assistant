import { writable } from 'svelte/store';
import type { User } from 'oidc-client';

import { AlertState } from '../../../shared2/models/alertState';
import { AlertStatus } from '../../../shared2/models/enums/alertEvents';

export const isOnline = writable<boolean>(true);
export const loggedInUser = writable<User | null>(null);
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
